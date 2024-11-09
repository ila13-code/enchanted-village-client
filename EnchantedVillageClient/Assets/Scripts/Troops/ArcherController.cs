using UnityEngine;
using System.Collections;
using Unical.Demacs.EnchantedVillage;
using static Unical.Demacs.EnchantedVillage.BattleBuilding;

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

    private const float GRID_POSITION_BUFFER = 0.1f;

    // Limiti della griglia
    private int minGridX = 0;
    private int maxGridX;
    private int minGridY = 0;
    private int maxGridY;


    private float buildingScanInterval = 0.5f;
    private float lastScanTime = 0f;
    private bool isInitialized = false;
    private Vector3 startingPosition;
    private Coroutine scanCoroutine;

    // Animation States
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
        // Aspetta che BattleMap sia pronto
        while (BattleMap.Instance == null || !BattleMap.Instance.isDataLoaded)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isInitialized = true;
        FindNearestBuilding();
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

    private void Awake()
    {
        _buildGrid = FindObjectOfType<BuildGrid>();
        if (_buildGrid == null)
        {
            Debug.LogError("BuildGrid not found in the scene. Please add a BuildGrid component.");
            return;
        }

        // Inizializza i limiti della griglia usando le proprietà corrette
        maxGridX = _buildGrid.Columns - 1;
        maxGridY = _buildGrid.Rows - 1;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Initialize starting position
        Vector3 startPos = transform.position;
        (_currentX, _currentY) = _buildGrid.WorldToGridPosition(startPos);
        SetAnimationState(AnimationState.Idle);
    }



    void UpdateAnimationBasedOnGridMovement(int targetX, int targetY)
    {
        int dx = targetX - _currentX;
        int dy = targetY - _currentY;

        AnimationState newState = AnimationState.Idle;

        // Movimento orizzontale
        if (dx > 0) // Destra
        {
            if (dy > 0) // Diagonale su-destra
            {
                newState = AnimationState.WalkRightUp;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (dy < 0) // Diagonale giù-destra
            {
                newState = AnimationState.WalkRightDown;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else // Destra pura
            {
                newState = AnimationState.WalkRight;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else if (dx < 0) // Sinistra
        {
            if (dy > 0) // Diagonale su-sinistra
            {
                newState = AnimationState.WalkRightUp;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (dy < 0) // Diagonale giù-sinistra
            {
                newState = AnimationState.WalkRightDown;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else // Sinistra pura
            {
                newState = AnimationState.WalkRight;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else // Movimento verticale puro
        {
            if (dy > 0) // Su
            {
                newState = AnimationState.WalkUp;
            }
            else if (dy < 0) // Giù
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

                        if (IsValidGridPosition(newX, newY))
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
            else
            {
                Debug.LogWarning($"No valid position found near grid position ({targetX}, {targetY}). Current pos: ({_currentX}, {_currentY})");
            }
        }
    }

    void MoveTowardsTarget()
    {
        if (currentTarget != null && !isMoving)
        {
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 nearestPoint = _buildGrid.GetNearestPointOnGrid(targetPos);
            (int targetGridX, int targetGridY) = _buildGrid.WorldToGridPosition(nearestPoint);

            if (_currentX != targetGridX || _currentY != targetGridY)
            {
                MoveToGridPosition(targetGridX, targetGridY);
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

    IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.1f);

        // Verifica se il target esiste e non è stato distrutto
        if (CurrentAttackTarget != null && CurrentAttackTarget.gameObject != null)
        {
            var enemyBuilding = CurrentAttackTarget.GetComponent<EnemyBuildingsController>();
            if (enemyBuilding != null && enemyBuilding.IsAlive())
            {
                yield return new WaitForSeconds(0.5f);

                // Verifica nuovamente prima di infliggere danno
                if (CurrentAttackTarget != null &&
                    CurrentAttackTarget.gameObject != null &&
                    enemyBuilding != null &&
                    enemyBuilding.IsAlive())
                {
                    AttackManager.Instance?.ProcessAttack(CurrentAttackTarget.name);
                    enemyBuilding.TakeDamage(1);
                }

                // Aspetta che l'animazione di attacco finisca
                yield return new WaitForSeconds(0.4f);
            }
        }

        // Reset dello stato
        SetAnimationState(AnimationState.Idle);
        currentTarget = null;
        CurrentAttackTarget = null;

        // Piccolo delay prima di cercare un nuovo target
        yield return new WaitForSeconds(0.2f);

        // Controlla se l'archer esiste ancora prima di cercare un nuovo target
        if (this != null && gameObject != null)
        {
            FindNearestBuilding();
        }
    }

    void CheckForAttack()
    {
        if (!isMoving && currentTarget != null)
        {
            // Verifica se il target è ancora valido
            var enemyBuilding = currentTarget.GetComponent<EnemyBuildingsController>();
            if (enemyBuilding == null || !enemyBuilding.IsAlive())
            {
                currentTarget = null;
                return;
            }

            // Verifica la distanza solo se il target è valido
            if (Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange)
            {
                StopMoving();
                SetAnimationState(AnimationState.Attack);
                CurrentAttackTarget = currentTarget;
                StartCoroutine(PerformAttack());
            }
        }
    }

    void FindNearestBuilding()
    {
        if (!isInitialized || Time.time - lastScanTime < buildingScanInterval) return;
        lastScanTime = Time.time;

        float closestDistance = float.MaxValue;
        GameObject closestBuilding = null;

        // Prendi la griglia degli edifici da BattleMap
        var enemyBuildings = BattleMap.Instance.GetEnemyBuildings();
        if (enemyBuildings == null) return;

        // Calcola l'area di ricerca basata sulla posizione corrente dell'arciere
        int searchRadius = Mathf.CeilToInt(detectionRange);
        int startX = Mathf.Max(0, _currentX - searchRadius);
        int endX = Mathf.Min(maxGridX, _currentX + searchRadius);
        int startY = Mathf.Max(0, _currentY - searchRadius);
        int endY = Mathf.Min(maxGridY, _currentY + searchRadius);

        // Cerca nell'area definita
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                var buildingInCell = enemyBuildings[x, y];
                if (buildingInCell == null || buildingInCell is BuildingEnemyPlaceholder) continue;

                // Ignora placeholder e prendi solo l'edificio principale
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

        // Se troviamo un edificio e non è il nostro target attuale
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
            // Se non troviamo edifici e non ci stiamo muovendo, torna alla posizione iniziale
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
        SetAnimationState(AnimationState.Idle);
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
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
        }
    }
}