using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;
using UnityEngine.AI;

public class ArcherIA : MonoBehaviour {
    NavMeshAgent agent;
    Troops troops;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;

    private float detectionRadius = 30f;
    private float attackRange = 6f;
    private float updateRate = 0.5f;

    private Transform currentTarget;
    private bool isAttacking;
    private LayerMask targetLayer;
    private Vector2 lastPosition;
    private AnimationState currentState = AnimationState.Idle;
    private int damage = 15;
    private bool move = true;

    private enum AnimationState {
        Idle = 0,
        WalkDown = 1,
        WalkRight = 2,
        WalkRightDown = 3,
        WalkRightUp = 4,
        WalkUp = 5,
        Attack = 6
    }

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        troops = GetComponent<Troops>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        lastPosition = transform.position;
        agent.speed = 5;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        targetLayer = LayerMask.GetMask("Building");

        StartCoroutine(WaitForBuildingsAndInitialize());
    }

    IEnumerator WaitForBuildingsAndInitialize() {
        while (BattleMap.Instance == null || !BattleMap.Instance.isDataLoaded) {
            yield return new WaitForSeconds(0.5f);
        }

        isInitialized = true;
        
    }

    void Update() {
        if (!move || troops.CurrentHealth <= 0 || !agent.isOnNavMesh || !isInitialized) return;

        if (currentTarget == null || !IsValidTarget(currentTarget) || !IsTargetInRange(currentTarget)) {
            FindTarget();
            Debug.Log($"Current target: {currentTarget?.name}");

            if (currentTarget == null) {
                agent.isStopped = true;
                move = false;
                SetAnimationState(AnimationState.Idle);

                return; 
            }
        }

        UpdateAnimation();

        if (currentTarget != null) {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange) {
                if (!isAttacking) {
                    agent.isStopped = true;
                    StartCoroutine(AttackRoutine());
                }
            }
            else {
                agent.isStopped = false;
                Vector3 targetPosition = currentTarget.position;
                targetPosition.z = transform.position.z;
                Debug.Log($"Moving to target: {currentTarget.name} in position: {currentTarget.position}");
                Debug.Log($"Archer position: {lastPosition}");
                agent.SetDestination(targetPosition);
            }
        }

        lastPosition = transform.position;
    }

    private bool IsValidTarget(Transform target) {
        if (target == null) return false;

        var enemyTroop = target.GetComponent<EnemyTroopController>();
        if (enemyTroop != null) {
            return enemyTroop.GetHealth() > 0;
        }

        var building = target.GetComponent<EnemyBuildingsController>();
        if (building != null) {
            return building.IsAlive();
        }

        return false;
    }

    private bool IsTargetInRange(Transform target) {
        return target != null && Vector2.Distance(transform.position, target.position) <= detectionRadius;
    }

    private void FindTarget() {
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        Debug.Log($"Found {allColliders.Length} total objects in range on Building layer");

        var enemies = allColliders
            .Select(c => c.GetComponent<EnemyTroopController>())
            .Where(c => c != null && c.GetHealth() > 0) 
            .ToArray();

        Debug.Log($"Found {enemies.Length} enemies in range");

        if (enemies.Length > 0) {
            Transform closestEnemy = enemies
                .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
                .First()
                .transform;

            if (currentTarget == null || currentTarget != closestEnemy) {
                currentTarget = closestEnemy;
                Debug.Log($"Found enemy target: {currentTarget.name}");
            }
        }
        else {
            var buildings = allColliders
                .Select(c => c.GetComponent<EnemyBuildingsController>())
                .Where(c => c != null && c.IsAlive())
                .ToArray();

            Debug.Log($"Found {buildings.Length} buildings in range");

            if (buildings.Length > 0) {
                Transform closestBuilding = buildings
                    .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
                    .First()
                    .transform;

                if (currentTarget == null || currentTarget != closestBuilding) {
                    currentTarget = closestBuilding;
                    Debug.Log($"Found building target: {currentTarget.name}");
                }
            }
            else {
                currentTarget = null;
                Debug.Log("No targets found");

            }
        }

    }

    private void UpdateAnimation() {
        if (isAttacking) {
            SetAnimationState(AnimationState.Attack);
            return;
        }

        Vector2 movement = (Vector2)transform.position - lastPosition;

        if (movement.magnitude < 0.01f) {
            SetAnimationState(AnimationState.Idle);
            return;
        }

        movement = movement.normalized;
        float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) {
            SetAnimationState(AnimationState.WalkRight);
            spriteRenderer.flipX = false;
        }
        else if (angle >= 22.5f && angle < 67.5f) {
            SetAnimationState(AnimationState.WalkRightUp);
            spriteRenderer.flipX = false;
        }
        else if (angle >= 67.5f && angle < 112.5f) {
            SetAnimationState(AnimationState.WalkUp);
        }
        else if (angle >= 112.5f && angle < 157.5f) {
            SetAnimationState(AnimationState.WalkRightUp);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 157.5f && angle < 202.5f) {
            SetAnimationState(AnimationState.WalkRight);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 202.5f && angle < 247.5f) {
            SetAnimationState(AnimationState.WalkRightDown);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 247.5f && angle < 292.5f) {
            SetAnimationState(AnimationState.WalkDown);
        }
        else {
            SetAnimationState(AnimationState.WalkRightDown);
            spriteRenderer.flipX = false;
        }
    }

    private void SetAnimationState(AnimationState state) {
        if (currentState != state) {
            currentState = state;
            animator.SetInteger("State", (int)state);
        }
    }

    private IEnumerator AttackRoutine() {
        isAttacking = true;
        Debug.Log("Starting attack routine");

        if (troops.CurrentHealth > 0) {
            if (currentTarget == null || !IsValidTarget(currentTarget)) {
                Debug.Log("Target is invalid or dead, searching for new target");
                FindTarget();
                yield return new WaitForSeconds(1);
            }

            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > attackRange) {
                Debug.Log($"Target out of range. Distance: {distanceToTarget}, Attack Range: {attackRange}");
            }

            var enemyTroops = currentTarget.GetComponent<EnemyTroopController>();
            if (enemyTroops != null) {
                Debug.Log($"Attacking enemy troop: {enemyTroops.name} with current health: {enemyTroops.GetHealth()}");
                enemyTroops.TakeDamage(damage);
                yield return new WaitForSeconds(1);
            }
            else {
                var building = currentTarget.GetComponent<EnemyBuildingsController>();

                if (building != null && building.IsAlive()) {
                    Debug.Log($"Attacking building: {building.name}");
                    AttackManager.Instance?.ProcessAttack(building.GetUniqueId(), building.name);
                    building.TakeDamage(damage);
                    yield return new WaitForSeconds(1);
                }
            }
        }

        Debug.Log("Attack routine ended");
        currentTarget = null;
        isAttacking = false;
        SetAnimationState(AnimationState.Idle);
    }

    private IEnumerator UpdateTargeting() {
        while (troops.CurrentHealth > 0) {
            FindTarget();
            yield return new WaitForSeconds(updateRate);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentTarget != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}