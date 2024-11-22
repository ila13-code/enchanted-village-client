using UnityEngine;
using System.Collections;
using static Unical.Demacs.EnchantedVillage.BattleBuilding;

namespace Unical.Demacs.EnchantedVillage
{
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
        private Troops targetTroops; 
        private bool isTargetDead = false;


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
            FindNearestTarget();
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
                    FindNearestTarget();
                }
                else if (!isMoving)
                {
                    MoveTowardsTarget();
                }

                CheckForAttack();
            }
        }

        void FindNearestTarget()
        {
            if (!isInitialized || Time.time - lastScanTime < buildingScanInterval) return;
            lastScanTime = Time.time;

            float closestDistance = float.MaxValue;
            GameObject closestTarget = null;
            bool foundEnemyTroop = false;

            // Prima cerca le truppe nemiche
            var enemyTroops = FindObjectsOfType<EnemyTroopController>();
            foreach (var enemyTroop in enemyTroops)
            {
                if (enemyTroop != null && enemyTroop.gameObject.activeInHierarchy)
                {
                    // Verifica che la truppa nemica sia viva
                    var troopsComponent = enemyTroop.GetComponent<Troops>();
                    if (troopsComponent != null && troopsComponent.CurrentHealth <= 0) continue;

                    float distance = Vector3.Distance(transform.position, enemyTroop.transform.position);
                    if (distance <= detectionRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = enemyTroop.gameObject;
                        foundEnemyTroop = true;
                    }
                }
            }


            // Se non ci sono truppe nemiche vicine, cerca gli edifici
            if (!foundEnemyTroop)
            {
                var enemyBuildings = BattleMap.Instance.GetEnemyBuildings();
                if (enemyBuildings != null)
                {
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
                                    closestTarget = buildingInCell.gameObject;
                                }
                            }
                        }
                    }
                }
            }

            if (closestTarget != null && closestTarget != currentTarget)
            {
                currentTarget = closestTarget;
                targetTroops = currentTarget.GetComponent<Troops>();
                isTargetDead = false;
                if (!isMoving)
                {
                    MoveTowardsTarget();
                }
            }
            else if (closestTarget == null && !isMoving)
            {
                ReturnToStartPosition();
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
            new Vector2Int(1, 1)    // Diagonale in alto a destra
            };

            foreach (var offset in possibleOffsets)
            {
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

            targetX = _currentX;
            targetY = _currentY;
        }

        bool IsPositionOccupied(int x, int y)
        {
            var buildings = BattleMap.Instance.GetEnemyBuildings();
            if (buildings != null && buildings[x, y] != null)
            {
                return true;
            }
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

            Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;

            if (angle < 0) angle += 360;

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
                // Verifica se il target è ancora vivo
                var enemyTroop = currentTarget.GetComponent<EnemyTroopController>();
                if (enemyTroop != null)
                {
                    var troopsHealth = currentTarget.GetComponent<Troops>();
                    if (troopsHealth == null || troopsHealth.CurrentHealth <= 0)
                    {
                        StopAttackAndReset();
                        return;
                    }
                }

                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distance <= attackRange)
                {
                    StopMoving();
                    UpdateAnimationBasedOnTarget();
                    SetAnimationState(AnimationState.Attack);
                    CurrentAttackTarget = currentTarget;
                    StartCoroutine(PerformAttack());
                }
            }
        }

            IEnumerator PerformAttack()
            {
            if (CurrentAttackTarget == null)
            {
                StopAttackAndReset();
                yield break;
            }

            // Verifica se il target è una truppa nemica
            var enemyTroop = CurrentAttackTarget.GetComponent<EnemyTroopController>();
            if (enemyTroop != null)
            {
                var enemyHealth = CurrentAttackTarget.GetComponent<Troops>();
                if (enemyHealth == null || enemyHealth.CurrentHealth <= 0)
                {
                    StopAttackAndReset();
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);

                // Verifica nuovamente prima di infliggere danno
                if (CurrentAttackTarget != null && CurrentAttackTarget.gameObject != null &&
                    enemyHealth != null && enemyHealth.CurrentHealth > 0)
                {
                    enemyHealth.TakeDamage(1);

                    // Se il nemico è morto dopo questo attacco
                    if (enemyHealth.CurrentHealth <= 0)
                    {
                        isTargetDead = true;
                        StopAttackAndReset();
                        yield break;
                    }
                }
                else
                {
                    StopAttackAndReset();
                    yield break;
                }
            }
            else
            {
                // Logica per gli edifici (resta invariata)
                var enemyBuilding = CurrentAttackTarget.GetComponent<EnemyBuildingsController>();
                if (enemyBuilding != null && enemyBuilding.IsAlive())
                {
                    yield return new WaitForSeconds(0.5f);
                    if (CurrentAttackTarget != null && CurrentAttackTarget.gameObject != null)
                    {
                        AttackManager.Instance?.ProcessAttack(CurrentAttackTarget.name);
                        enemyBuilding.TakeDamage(1);
                    }
                }
                else
                {
                    StopAttackAndReset();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.4f);

            // Se il target è morto, cerca un nuovo target
            if (isTargetDead || CurrentAttackTarget == null)
            {
                StopAttackAndReset();
            }
            else
            {
                FindNearestTarget();
            }

            yield return new WaitForSeconds(0.2f);
        }


     private void StopAttackAndReset()
    {
        SetAnimationState(AnimationState.Idle);
        if (currentTarget != null)
        {
            var enemyTroops = currentTarget.GetComponent<Troops>();
            if (enemyTroops != null && enemyTroops.CurrentHealth <= 0)
            {
                isTargetDead = true;
            }
        }
        currentTarget = null;
        CurrentAttackTarget = null;
        targetTroops = null;
        FindNearestTarget();
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

        public void RemoveTarget(GameObject target)
        {
            if (currentTarget == target)
            {
                currentTarget = null;
                CurrentAttackTarget = null;
            }
        }
    }
}