using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unical.Demacs.EnchantedVillage;

public class EnemyTroopController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float minDistanceFromOtherTroops = 1.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private BuildGrid buildGrid;
    private int currentX;
    private int currentY;
    private bool isMoving = false;
    private bool isAttacking = false;
    private float lastAttackTime;
    private GameObject currentTarget;
    private static Dictionary<GameObject, int> targetAssignments = new Dictionary<GameObject, int>();
    private static List<EnemyTroopController> allTroops = new List<EnemyTroopController>();

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

    private const string ANIM_STATE_PARAM = "State";
    private const int DEATH_STATE = 7;
    private int health = 100;
    private bool isDead = false;

    private void Awake()
    {
        allTroops.Add(this);
    }

    private void Start()
    {
        buildGrid = FindObjectOfType<BuildGrid>();
        if (buildGrid == null)
        {
            Debug.LogError("BuildGrid not found!");
            return;
        }

        Vector3 startPos = transform.position;
        (currentX, currentY) = buildGrid.WorldToGridPosition(startPos);

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        StartCoroutine(ScanForTargets());
    }

    private void OnDestroy()
    {
        if (currentTarget != null)
        {
            ReleaseTarget(currentTarget);
        }
        allTroops.Remove(this);
    }

    private void Update()
    {
        if (isDead) return;

        if (currentTarget != null && !isAttacking)
        {
            if (!IsTargetValid(currentTarget))
            {
                ReleaseTarget(currentTarget);
                currentTarget = null;
                return;
            }

            if (IsInAttackRange(currentTarget.transform.position))
            {
                StartAttack();
            }
            else if (!isMoving)
            {
                MoveTowardsTarget();
            }
        }
    }

    private static void AssignTarget(GameObject target, EnemyTroopController troop)
    {
        if (!targetAssignments.ContainsKey(target))
        {
            targetAssignments[target] = 1;
        }
        else
        {
            targetAssignments[target]++;
        }
    }

    private static void ReleaseTarget(GameObject target)
    {
        if (targetAssignments.ContainsKey(target))
        {
            targetAssignments[target]--;
            if (targetAssignments[target] <= 0)
            {
                targetAssignments.Remove(target);
            }
        }
    }

    private IEnumerator ScanForTargets()
    {
        while (!isDead)
        {
            if (currentTarget == null || !IsTargetValid(currentTarget))
            {
                if (currentTarget != null)
                {
                    ReleaseTarget(currentTarget);
                    currentTarget = null;
                }
                FindNewTarget();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void FindNewTarget()
    {
        if (isDead) return;

        var potentialTargets = FindObjectsOfType<ArcherController>()
            .Where(a => a != null &&
                   a.gameObject.activeInHierarchy &&
                   a.GetComponent<Troops>()?.CurrentHealth > 0)
            .Select(a => new
            {
                Archer = a,
                Distance = Vector3.Distance(transform.position, a.transform.position)
            })
            .Where(a => a.Distance <= detectionRange)
            .OrderBy(a => GetTargetPriority(a.Archer.gameObject, a.Distance))
            .ToList();

        foreach (var target in potentialTargets)
        {
            if (!targetAssignments.ContainsKey(target.Archer.gameObject) ||
                targetAssignments[target.Archer.gameObject] < 1)
            {
                if (currentTarget != null)
                {
                    ReleaseTarget(currentTarget);
                }
                currentTarget = target.Archer.gameObject;
                AssignTarget(currentTarget, this);
                UpdateAnimationBasedOnTarget();
                break;
            }
        }
    }

    private float GetTargetPriority(GameObject target, float distance)
    {
        float priority = distance;
        if (targetAssignments.ContainsKey(target))
        {
            priority += targetAssignments[target] * 5f;
        }
        return priority;
    }

    private bool IsTargetValid(GameObject target)
    {
        if (target == null || !target.activeInHierarchy) return false;

        var troopsHealth = target.GetComponent<Troops>();
        if (troopsHealth != null)
        {
            return troopsHealth.CurrentHealth > 0;
        }
        return false;
    }

    private bool IsInAttackRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= attackRange;
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 targetPos = currentTarget.transform.position;
        (int targetX, int targetY) = buildGrid.WorldToGridPosition(targetPos);

        FindSuitableAttackPosition(targetX, targetY, out int moveToX, out int moveToY);

        if (moveToX != currentX || moveToY != currentY)
        {
            StartCoroutine(MoveToPosition(moveToX, moveToY));
        }
    }

    private void FindSuitableAttackPosition(int targetX, int targetY, out int moveToX, out int moveToY)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(0, 1),
            new Vector2Int(-1, -1), new Vector2Int(-1, 1),
            new Vector2Int(1, -1), new Vector2Int(1, 1)
        };

        directions = directions.OrderBy(x => Random.value).ToList();

        foreach (var dir in directions)
        {
            int checkX = targetX + dir.x;
            int checkY = targetY + dir.y;

            if (IsValidPosition(checkX, checkY) && !IsPositionOccupiedByOtherTroops(checkX, checkY))
            {
                moveToX = checkX;
                moveToY = checkY;
                return;
            }
        }

        for (int radius = 2; radius <= 3; radius++)
        {
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                for (int offsetY = -radius; offsetY <= radius; offsetY++)
                {
                    int checkX = targetX + offsetX;
                    int checkY = targetY + offsetY;

                    if (IsValidPosition(checkX, checkY) && !IsPositionOccupiedByOtherTroops(checkX, checkY))
                    {
                        moveToX = checkX;
                        moveToY = checkY;
                        return;
                    }
                }
            }
        }

        moveToX = currentX;
        moveToY = currentY;
    }

    private bool IsPositionOccupiedByOtherTroops(int x, int y)
    {
        Vector3 position = buildGrid.GetCenterPosition1(x, y, 1, 1);

        foreach (var troop in allTroops)
        {
            if (troop != this && !troop.isMoving &&
                Vector3.Distance(troop.transform.position, position) < minDistanceFromOtherTroops)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidPosition(int x, int y)
    {
        if (!buildGrid.IsPositionInMap(x, y, 1, 1)) return false;

        Vector3 position = buildGrid.GetCenterPosition1(x, y, 1, 1);
        Collider[] colliders = Physics.OverlapSphere(position, 0.4f);

        return colliders.All(c => c.gameObject.GetComponent<EnemyTroopController>() == null);
    }

    private IEnumerator MoveToPosition(int targetX, int targetY)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = buildGrid.GetCenterPosition1(targetX, targetY, 1, 1);

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        UpdateAnimationBasedOnMovement(targetX - currentX, targetY - currentY);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            yield return null;
        }

        transform.position = targetPosition;
        currentX = targetX;
        currentY = targetY;
        isMoving = false;

        UpdateAnimationBasedOnTarget();
    }

    private void StartAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        isAttacking = true;
        SetAnimationState(AnimationState.Attack);
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.5f);

        if (currentTarget != null && IsInAttackRange(currentTarget.transform.position) && IsTargetValid(currentTarget))
        {
            var troopsHealth = currentTarget.GetComponent<Troops>();
            if (troopsHealth != null && troopsHealth.CurrentHealth > 0)
            {
                troopsHealth.TakeDamage(damage);
                Debug.Log($"Attacco eseguito! Danno inflitto: {damage}");
            }
            else
            {
                ReleaseTarget(currentTarget);
                currentTarget = null;
            }
        }

        lastAttackTime = Time.time;
        isAttacking = false;
        SetAnimationState(AnimationState.Idle);
    }

    private void UpdateAnimationBasedOnTarget()
    {
        if (currentTarget == null) return;

        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 315 || angle < 45)
        {
            SetAnimationState(AnimationState.WalkRight);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (angle >= 45 && angle < 135)
        {
            SetAnimationState(AnimationState.WalkUp);
        }
        else if (angle >= 135 && angle < 225)
        {
            SetAnimationState(AnimationState.WalkRight);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            SetAnimationState(AnimationState.WalkDown);
        }
    }

    private void UpdateAnimationBasedOnMovement(int dx, int dy)
    {
        AnimationState newState = AnimationState.Idle;

        if (dx > 0)
        {
            newState = dy > 0 ? AnimationState.WalkRightUp :
                      dy < 0 ? AnimationState.WalkRightDown :
                      AnimationState.WalkRight;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (dx < 0)
        {
            newState = dy > 0 ? AnimationState.WalkRightUp :
                      dy < 0 ? AnimationState.WalkRightDown :
                      AnimationState.WalkRight;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            newState = dy > 0 ? AnimationState.WalkUp :
                      dy < 0 ? AnimationState.WalkDown :
                      AnimationState.Idle;
        }

        SetAnimationState(newState);
    }

    private void SetAnimationState(AnimationState state)
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE_PARAM, (int)state);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Rilascia il target corrente
        if (currentTarget != null)
        {
            ReleaseTarget(currentTarget);
            currentTarget = null;
        }

        // Ferma tutti i comportamenti
        StopAllCoroutines();
        isMoving = false;
        isAttacking = false;

        // Disabilita il controller
        enabled = false;

        // Rimuovi dalle truppe attive
        allTroops.Remove(this);

        // Attiva l'animazione di morte
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE_PARAM, DEATH_STATE);
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);

        if (animator != null)
        {
            animator.speed = 0;
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}