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
    private bool isMoving = false;
    private string currentMoveDirection = "";

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

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            SetMovementAnimation(direction);
            yield return null;
        }
        transform.position = targetPosition;
        StopMoving();
    }

    void SetMovementAnimation(Vector3 direction)
    {
        string newDirection = GetDirectionString(direction);

        Debug.Log($"Setting movement animation: {newDirection}");

        if (newDirection != currentMoveDirection || !isMoving)
        {
            if (!string.IsNullOrEmpty(currentMoveDirection))
            {
                animator.ResetTrigger(currentMoveDirection);
            }

            animator.SetTrigger(newDirection);
            Debug.Log($"Trigger set: {newDirection}");
            currentMoveDirection = newDirection;
            isMoving = true;
        }

        animator.speed = 1f;
    }

    string GetDirectionString(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;


        //todo: fixare i valori per le direzioni
        if (angle > 67.5f && angle <= 112.5f)
            return "MoveUp";
        else if (angle > 22.5f && angle <= 67.5f)
            return "MoveRightUp";
        else if (angle > -22.5f && angle <= 22.5f)
            return "MoveRight";
        else if (angle > -67.5f && angle <= -22.5f)
            return "MoveRightDown";
        else if (angle > -112.5f && angle <= -67.5f)
            return "MoveDown";
        else
            return "MoveRight"; 
    }

    void StopMoving()
    {
        if (!string.IsNullOrEmpty(currentMoveDirection))
        {
            animator.ResetTrigger(currentMoveDirection);
        }
        animator.SetTrigger("StopMoving");
        Debug.Log("Stop moving triggered"); // Debug log
        currentMoveDirection = "";
        isMoving = false;
    }

    void CheckForAttack()
    {
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange)
        {
            StopMoving(); 
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