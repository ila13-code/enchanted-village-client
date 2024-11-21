using UnityEngine;
using System.Collections;
using Unical.Demacs.EnchantedVillage;
using static Unical.Demacs.EnchantedVillage.BattleBuilding;

public class ArcherController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask buildingLayer;
    public float attackRange = 5f; 
    public float detectionRange = 20f;
    public Animator animator;

    private int _currentX;
    private int _currentY;
    public int CurrentX => _currentX;
    public int CurrentY => _currentY;

    private BuildGrid _buildGrid;
    private GameObject currentTarget;
    public GameObject CurrentAttackTarget { get; private set; }
    private bool isMoving = false;

    private const float GRID_POSITION_BUFFER = 0.1f;

    private int minGridX = 0;
    private int maxGridX;
    private int minGridY = 0;
    private int maxGridY;

    private float buildingScanInterval = 0.5f;
    private float lastScanTime = 0f;
    private bool isInitialized = false;
    private Vector3 startingPosition;
    private Coroutine scanCoroutine;

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

    private void Start()
    {
        startingPosition = transform.position;
        StartCoroutine(WaitForBuildingsAndInitialize());
    }

    IEnumerator WaitForBuildingsAndInitialize()
    {
        while (BattleMap.Instance == null || !BattleMap.Instance.isDataLoaded)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isInitialized = true;
        FindNearestBuilding();
    }

    private void Awake()
    {
        _buildGrid = FindObjectOfType<BuildGrid>();
        if (_buildGrid == null)
        {
            Debug.LogError("BuildGrid not found in the scene.");
            return;
        }

        maxGridX = _buildGrid.Columns - 1;
        maxGridY = _buildGrid.Rows - 1;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        Vector3 startPos = transform.position;
        (_currentX, _currentY) = _buildGrid.WorldToGridPosition(startPos);
        SetAnimationState(AnimationState.Idle);
    }

    void Update()
    {
        if (!isInitialized) return;

        if (ArcherManager.Instance != null && ArcherManager.Instance.canMoveArchers)
        {
            if (currentTarget == null)
            {
                FindNearestBuilding();
            }
            else if (!isMoving)
            {
                MoveTowardsTarget();
            }

            CheckForAttack();
        }
    }

    void FindSuitableAttackPosition(GameObject target, out int targetX, out int targetY)
    {

        Vector3 targetPos = target.transform.position;
        (int buildingX, int buildingY) = _buildGrid.WorldToGridPosition(targetPos);


        var battleBuilding = target.GetComponent<BattleBuilding>();
        int buildingRows = battleBuilding != null ? battleBuilding.Rows : 1;
        int buildingCols = battleBuilding != null ? battleBuilding.Columns : 1;

        Vector2Int[] possibleOffsets = new Vector2Int[]
        {
            new Vector2Int(-1, 0),  // Sinistra
            new Vector2Int(1, 0),   // Destra
            new Vector2Int(0, -1),  // Sotto
            new Vector2Int(0, 1),   // Sopra
            new Vector2Int(-1, -1), // Diagonale in basso a sinistra
            new Vector2Int(1, -1),  // Diagonale in basso a destra
            new Vector2Int(-1, 1),  // Diagonale in alto a sinistra
            new Vector2Int(1, 1)    
        };

        // Prova tutte le posizioni intorno all'edificio
        foreach (var offset in possibleOffsets)
        {
            // Controlla tutte le celle intorno all'edificio
            for (int row = 0; row < buildingRows; row++)
            {
                for (int col = 0; col < buildingCols; col++)
                {
                    int checkX = buildingX + col + offset.x;
                    int checkY = buildingY + row + offset.y;

                    if (IsValidGridPosition(checkX, checkY) && !IsPositionOccupied(checkX, checkY))
                    {
                        targetX = checkX;
                        targetY = checkY;
                        return;
                    }
                }
            }
        }

        // Se non troviamo una posizione valida, usa la posizione corrente
        targetX = _currentX;
        targetY = _currentY;
    }

    bool IsPositionOccupied(int x, int y)
    {
        // Controlla se la posizione è occupata da un edificio
        var buildings = BattleMap.Instance.GetEnemyBuildings();
        if (buildings != null && buildings[x, y] != null)
        {
            return true;
        }

        // Qui potresti aggiungere altri controlli per altri tipi di ostacoli
        return false;
    }

    void MoveTowardsTarget()
    {
        if (currentTarget != null && !isMoving)
        {
            FindSuitableAttackPosition(currentTarget, out int targetGridX, out int targetGridY);

            if (_currentX != targetGridX || _currentY != targetGridY)
            {
                MoveToGridPosition(targetGridX, targetGridY);
            }
        }
    }

    void UpdateAnimationBasedOnTarget()
    {
        if (currentTarget == null) return;

        // Calcola la direzione verso il target
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;

        // Normalizza l'angolo tra 0 e 360 gradi
        if (angle < 0) angle += 360;

        // Determina l'animazione basata sull'angolo
        AnimationState newState = AnimationState.Idle;

        if (angle >= 315 || angle < 45) // Destra
        {
            newState = AnimationState.WalkRight;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (angle >= 45 && angle < 135) // Su
        {
            newState = AnimationState.WalkUp;
        }
        else if (angle >= 135 && angle < 225) // Sinistra
        {
            newState = AnimationState.WalkRight;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (angle >= 225 && angle < 315) // Giù
        {
            newState = AnimationState.WalkDown;
        }

        SetAnimationState(newState);
    }

    void UpdateAnimationBasedOnGridMovement(int targetX, int targetY)
    {
        if (currentTarget != null)
        {
            UpdateAnimationBasedOnTarget();
            return;
        }

        int dx = targetX - _currentX;
        int dy = targetY - _currentY;

        AnimationState newState = AnimationState.Idle;

        if (dx > 0)
        {
            if (dy > 0)
            {
                newState = AnimationState.WalkRightUp;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (dy < 0)
            {
                newState = AnimationState.WalkRightDown;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                newState = AnimationState.WalkRight;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else if (dx < 0)
        {
            if (dy > 0)
            {
                newState = AnimationState.WalkRightUp;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (dy < 0)
            {
                newState = AnimationState.WalkRightDown;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                newState = AnimationState.WalkRight;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            if (dy > 0)
            {
                newState = AnimationState.WalkUp;
            }
            else if (dy < 0)
            {
                newState = AnimationState.WalkDown;
            }
        }

        SetAnimationState(newState);
    }

    public bool IsValidGridPosition(int x, int y)
    {
        return _buildGrid.IsPositionInMap(x, y, 1, 1);
    }

    public void MoveToGridPosition(int targetX, int targetY)
    {
        targetX = Mathf.Clamp(targetX, minGridX, maxGridX);
        targetY = Mathf.Clamp(targetY, minGridY, maxGridY);

        if (IsValidGridPosition(targetX, targetY))
        {
            if (!isMoving)
            {
                UpdateAnimationBasedOnGridMovement(targetX, targetY);
                Vector3 newPosition = _buildGrid.GetCenterPosition1(targetX, targetY, 1, 1);
                StartCoroutine(MoveToPosition(newPosition, targetX, targetY));
            }
        }
        else
        {
            for (int radius = 1; radius <= 3; radius++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    for (int offsetY = -radius; offsetY <= radius; offsetY++)
                    {
                        int newX = targetX + offsetX;
                        int newY = targetY + offsetY;

                        if (IsValidGridPosition(newX, newY) && !IsPositionOccupied(newX, newY))
                        {
                            MoveToGridPosition(newX, newY);
                            return;
                        }
                    }
                }
            }

            Vector3 nearestPoint = _buildGrid.GetNearestPointOnGrid(transform.position);
            (int nearestX, int nearestY) = _buildGrid.WorldToGridPosition(nearestPoint);

            if (IsValidGridPosition(nearestX, nearestY) && (nearestX != _currentX || nearestY != _currentY))
            {
                MoveToGridPosition(nearestX, nearestY);
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPosition, int targetX, int targetY)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            yield return null;
        }

        transform.position = targetPosition;
        _currentX = targetX;
        _currentY = targetY;
        StopMoving();
    }

    void CheckForAttack()
    {
        if (!isMoving && currentTarget != null)
        {
            var enemyBuilding = currentTarget.GetComponent<EnemyBuildingsController>();
            if (enemyBuilding == null || !enemyBuilding.IsAlive())
            {
                currentTarget = null;
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= attackRange)
            {
                StopMoving();
                UpdateAnimationBasedOnTarget(); // Aggiorna l'orientamento prima dell'attacco
                SetAnimationState(AnimationState.Attack);
                CurrentAttackTarget = currentTarget;
                StartCoroutine(PerformAttack());
            }
        }
    }

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.1f);

        if (CurrentAttackTarget != null && CurrentAttackTarget.gameObject != null)
        {
            var enemyBuilding = CurrentAttackTarget.GetComponent<EnemyBuildingsController>();
            if (enemyBuilding != null && enemyBuilding.IsAlive())
            {
                yield return new WaitForSeconds(0.5f);

                if (CurrentAttackTarget != null &&
                    CurrentAttackTarget.gameObject != null &&
                    enemyBuilding != null &&
                    enemyBuilding.IsAlive())
                {
                    AttackManager.Instance?.ProcessAttack(CurrentAttackTarget.name);
                    enemyBuilding.TakeDamage(1);
                }

                yield return new WaitForSeconds(0.4f);
            }
        }

        SetAnimationState(AnimationState.Idle);
        currentTarget = null;
        CurrentAttackTarget = null;

        yield return new WaitForSeconds(0.2f);

        if (this != null && gameObject != null)
        {
            FindNearestBuilding();
        }
    }

    void FindNearestBuilding()
    {
        if (!isInitialized || Time.time - lastScanTime < buildingScanInterval) return;
        lastScanTime = Time.time;

        float closestDistance = float.MaxValue;
        GameObject closestBuilding = null;

        var enemyBuildings = BattleMap.Instance.GetEnemyBuildings();
        if (enemyBuildings == null) return;

        int searchRadius = Mathf.CeilToInt(detectionRange);
        int startX = Mathf.Max(0, _currentX - searchRadius);
        int endX = Mathf.Min(maxGridX, _currentX + searchRadius);
        int startY = Mathf.Max(0, _currentY - searchRadius);
        int endY = Mathf.Min(maxGridY, _currentY + searchRadius);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                var buildingInCell = enemyBuildings[x, y];
                if (buildingInCell == null || buildingInCell is BuildingEnemyPlaceholder) continue;

                if (buildingInCell.gameObject == null) continue;

                Vector3 buildingPos = buildingInCell.transform.position;
                float distance = Vector3.Distance(transform.position, buildingPos);

                if (distance <= detectionRange && distance < closestDistance)
                {
                    var enemyBuildingController = buildingInCell.GetComponent<EnemyBuildingsController>();
                    if (enemyBuildingController != null && enemyBuildingController.IsAlive())
                    {
                        closestDistance = distance;
                        closestBuilding = buildingInCell.gameObject;
                    }
                }
            }
        }

        if (closestBuilding != null && closestBuilding != currentTarget)
        {
            currentTarget = closestBuilding;
            if (!isMoving)
            {
                MoveTowardsTarget();
            }
        }
        else if (closestBuilding == null && !isMoving)
        {
            ReturnToStartPosition();
        }
    }

    void ReturnToStartPosition()
    {
        if (!isMoving)
        {
            (int startX, int startY) = _buildGrid.WorldToGridPosition(startingPosition);
            if (_currentX != startX || _currentY != startY)
            {
                MoveToGridPosition(startX, startY);
            }
        }
    }

    void SetAnimationState(AnimationState state)
    {
        if (animator == null) return;
        animator.SetInteger(ANIM_STATE_PARAM, (int)state);
    }

    void StopMoving()
    {
        isMoving = false;
        if (currentTarget != null)
        {
            UpdateAnimationBasedOnTarget();
        }
        else
        {
            SetAnimationState(AnimationState.Idle);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (ArcherManager.Instance != null)
        {
            ArcherManager.Instance.RemoveArcher(this);
        }
    }
}