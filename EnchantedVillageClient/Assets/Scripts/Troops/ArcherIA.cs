using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;
using UnityEngine.AI;

public class ArcherIA : MonoBehaviour
{
    NavMeshAgent agent;
    Troops troops;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 100f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float updateRate = 0.5f;

    private Transform currentTarget;
    private bool isAttacking;
    private LayerMask targetLayer;
    private Vector2 lastPosition;
    private AnimationState currentState = AnimationState.Idle;

    private enum AnimationState
    {
        Idle = 0,
        WalkDown = 1,
        WalkRight = 2,
        WalkRightDown = 3,
        WalkRightUp = 4,
        WalkUp = 5,
        Attack = 6
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        troops = GetComponent<Troops>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        lastPosition = transform.position;

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        targetLayer = LayerMask.GetMask("Building");

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("Agent is not on NavMesh! Position: " + transform.position);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Debug.Log("Agent moved to nearest NavMesh position: " + hit.position);
            }
        }

        StartCoroutine(UpdateTargeting());
    }

    void Update()
    {
        if (troops.CurrentHealth <= 0 || !agent.isOnNavMesh) return;

        UpdateAnimation();

        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange)
            {
                agent.isStopped = true;
                if (!isAttacking)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                agent.isStopped = false;
                Vector3 targetPosition = currentTarget.position;
                targetPosition.z = transform.position.z;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
        else
        {
            SetAnimationState(AnimationState.Idle);
        }

        lastPosition = transform.position;
    }

    private void FindTarget()
    {
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        Debug.Log($"Found {allColliders.Length} total objects in range on Building layer");

        // Prima cerca nemici
        var enemies = allColliders
            .Where(c => c.GetComponent<EnemyTroopController>() != null)
            .ToArray();

        Debug.Log($"Found {enemies.Length} enemies in range");

        if (enemies.Length > 0)
        {
            Transform closestEnemy = enemies
                .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
                .First()
                .transform;

            if (currentTarget == null || currentTarget != closestEnemy)
            {
                currentTarget = closestEnemy;
                Debug.Log($"Found enemy target: {currentTarget.name}");
            }
        }
        else
        {
            var buildings = allColliders
                .Where(c => c.GetComponent<EnemyBuildingsController>() != null)
                .ToArray();

            Debug.Log($"Found {buildings.Length} buildings in range");

            if (buildings.Length > 0)
            {
                Transform closestBuilding = buildings
                    .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
                    .First()
                    .transform;

                if (currentTarget == null || currentTarget != closestBuilding)
                {
                    currentTarget = closestBuilding;
                    Debug.Log($"Found building target: {currentTarget.name}");
                }
            }
            else
            {
                currentTarget = null;
                Debug.Log("No targets found");
            }
        }

        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.position, Color.red, 0.5f);
        }
    }

    private void UpdateAnimation()
    {
        if (isAttacking)
        {
            SetAnimationState(AnimationState.Attack);
            return;
        }

        Vector2 movement = (Vector2)transform.position - lastPosition;

        if (movement.magnitude < 0.01f)
        {
            SetAnimationState(AnimationState.Idle);
            return;
        }

        // Normalizza il movimento
        movement = movement.normalized;

        // Calcola l'angolo del movimento
        float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        // Aggiusta l'angolo per essere tra 0 e 360
        if (angle < 0) angle += 360;

        // Imposta lo stato dell'animazione in base all'angolo
        if (angle >= 337.5f || angle < 22.5f) // Destra
        {
            SetAnimationState(AnimationState.WalkRight);
            spriteRenderer.flipX = false;
        }
        else if (angle >= 22.5f && angle < 67.5f) // Destra-Su
        {
            SetAnimationState(AnimationState.WalkRightUp);
            spriteRenderer.flipX = false;
        }
        else if (angle >= 67.5f && angle < 112.5f) // Su
        {
            SetAnimationState(AnimationState.WalkUp);
        }
        else if (angle >= 112.5f && angle < 157.5f) // Sinistra-Su
        {
            SetAnimationState(AnimationState.WalkRightUp);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 157.5f && angle < 202.5f) // Sinistra
        {
            SetAnimationState(AnimationState.WalkRight);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 202.5f && angle < 247.5f) // Sinistra-Giù
        {
            SetAnimationState(AnimationState.WalkRightDown);
            spriteRenderer.flipX = true;
        }
        else if (angle >= 247.5f && angle < 292.5f) // Giù
        {
            SetAnimationState(AnimationState.WalkDown);
        }
        else // Destra-Giù
        {
            SetAnimationState(AnimationState.WalkRightDown);
            spriteRenderer.flipX = false;
        }
    }

    private void SetAnimationState(AnimationState state)
    {
        if (currentState != state)
        {
            currentState = state;
            animator.SetInteger("State", (int)state);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        while (currentTarget != null &&
               Vector2.Distance(transform.position, currentTarget.position) <= attackRange &&
               troops.CurrentHealth > 0)
        {
            // Orienta il personaggio verso il target
            Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            // Imposta il flip in base alla direzione
            spriteRenderer.flipX = Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270;

            SetAnimationState(AnimationState.Attack);

            if (currentTarget.TryGetComponent<EnemyTroopController>(out EnemyTroopController enemyTroops))
            {
                enemyTroops.TakeDamage(5);
            }
            else if (currentTarget.TryGetComponent<EnemyBuildingsController>(out EnemyBuildingsController building))
            {
                if (building.IsAlive())
                {
                    AttackManager.Instance?.ProcessAttack(building.GetUniqueId(), building.name);
                    building.TakeDamage(5);
                }
               }

            yield return new WaitForSeconds(1);
        }

        isAttacking = false;
        SetAnimationState(AnimationState.Idle);
    }

    private IEnumerator UpdateTargeting()
    {
        while (troops.CurrentHealth > 0)
        {
            FindTarget();
            yield return new WaitForSeconds(updateRate);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualizza il raggio di rilevamento
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualizza il raggio di attacco
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Se c'è un target, disegna una linea verso di esso
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}