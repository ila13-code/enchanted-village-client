using UnityEngine;
using System.Collections;
using Unical.Demacs.EnchantedVillage;

public class ArcherController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask buildingLayer;
    public float attackRange = 0.5f;
    public float detectionRange = 10f;
    public Animator animator;

    private int _currentX;
    private int _currentY;
    public int CurrentX => _currentX;
    public int CurrentY => _currentY;

    private BuildGrid _buildGrid;
    private GameObject currentTarget;
    public GameObject CurrentAttackTarget { get; private set; }

    private void Awake()
    {
        _buildGrid = FindObjectOfType<BuildGrid>();
        if (_buildGrid == null)
        {
            Debug.LogError("BuildGrid not found in the scene. Please add a BuildGrid component.");
        }
    }

    void Update()
    {
        if (ArcherManager.Instance.canMoveArchers)  
        {
            if (currentTarget == null)
            {
                FindNearestBuilding();
            }
            else
            {
                MoveTowardsTarget();
            }

            CheckForAttack();
        }
    }

    public void MoveToGridPosition(int x, int y)
    {
        if (_buildGrid.IsPositionInMap(x, y, 1, 1))
        {
            _currentX = x;
            _currentY = y;
            Vector3 newPosition = _buildGrid.GetCenterPosition(x, y, 1, 1);
            StartCoroutine(MoveToPosition(newPosition));
        }
        else
        {
            Debug.LogWarning("Attempted to move to an invalid grid position.");
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            SetMovementAnimation(targetPosition - transform.position);
            yield return null;
        }
        transform.position = targetPosition;
    }

    void FindNearestBuilding()
    {
        Collider2D[] buildingColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, buildingLayer);
        float closestDistance = float.MaxValue;
        GameObject closestBuilding = null;

        foreach (Collider2D collider in buildingColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBuilding = collider.gameObject;
            }
        }

        if (closestBuilding != null)
        {
            currentTarget = closestBuilding;
            (int gridX, int gridY) = _buildGrid.WorldToGridPosition(closestBuilding.transform.position);
            MoveToGridPosition(gridX, gridY);
        }
    }

    void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            (int targetGridX, int targetGridY) = _buildGrid.WorldToGridPosition(currentTarget.transform.position);
            if (_currentX != targetGridX || _currentY != targetGridY)
            {
                MoveToGridPosition(targetGridX, targetGridY);
            }
        }
    }

    void SetMovementAnimation(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        animator.speed = 0.5f; 

        if (angle > 45 && angle <= 135)
            animator.SetTrigger("MoveUp");
        else if (angle > -135 && angle <= -45)
            animator.SetTrigger("MoveDown");
        else if (angle > -45 && angle <= 45)
            animator.SetTrigger("MoveRight");
        else
            animator.SetTrigger("MoveLeft");
    }


    void CheckForAttack()
    {
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange)
        {
            animator.SetTrigger("Attack");
            CurrentAttackTarget = currentTarget;
            if (CurrentAttackTarget != null)
            {
                Debug.Log($"Attacking {CurrentAttackTarget.name}");
                StartCoroutine(PerformAttack());
            }
        }
    }

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(1f);

        if (CurrentAttackTarget != null)
        {
            AttackManager.Instance.ProcessAttack(CurrentAttackTarget.name);
            CurrentAttackTarget.GetComponent<EnemyBuildingsController>().TakeDamage(1);
        }

        animator.SetTrigger("AttackEnd");
        currentTarget = null;
        CurrentAttackTarget = null;
        FindNearestBuilding();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}