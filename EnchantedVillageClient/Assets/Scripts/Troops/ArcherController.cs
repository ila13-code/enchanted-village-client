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
        private Vector2Int? reservedPosition = null;
        private float lastMovementTime = 0f;
        private const float MOVEMENT_COOLDOWN = 1f;
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

        void Start()
        {
            StartCoroutine(InitializeAndFindTarget());
        }

        IEnumerator InitializeAndFindTarget()
        {
            // Timeout per evitare loop infiniti in caso di problemi con BattleMap
            float timeout = 2f; // Tempo massimo di attesa (3 secondi)
            float elapsedTime = 0f;

            while ((BattleMap.Instance == null || !BattleMap.Instance.isDataLoaded) && elapsedTime < timeout)
            {
                Debug.Log("Aspettando BattleMap...");
                yield return new WaitForSeconds(0.5f);
                elapsedTime += 0.5f;
            }

            if (BattleMap.Instance == null || !BattleMap.Instance.isDataLoaded)
            {
                Debug.LogWarning("BattleMap non disponibile o non caricato correttamente. Procedendo con fallback manuale.");
                isInitialized = true;

                // Inizia subito a cercare manualmente
                FindNearestTarget();
                yield break;
            }

            // Se BattleMap è disponibile e caricato
            Debug.Log("BattleMap inizializzato correttamente.");
            isInitialized = true;
            FindNearestTarget();
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
            float closestDistance = float.MaxValue;
            GameObject closestTarget = null;
            bool foundEnemyTroop = false;
            // Se BattleMap.Instance non è disponibile o gli edifici non sono presenti, cerca manualmente

            if (BattleMap.Instance==null)
            {
                closestTarget = FindNearestBuildingManually();
            }
            else
            { 
            if (!isInitialized || Time.time - lastScanTime < buildingScanInterval) return;
            lastScanTime = Time.time;

             closestDistance = float.MaxValue;
             closestTarget = null;
             foundEnemyTroop = false;

            // Cerca truppe nemiche
            var enemyTroops = FindObjectsOfType<EnemyTroopController>();
            foreach (var enemyTroop in enemyTroops)
            {
                if (enemyTroop != null && enemyTroop.gameObject.activeInHierarchy)
                {
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

            // Cerca edifici nemici
            if (!foundEnemyTroop)
            {
                if (BattleMap.Instance != null)
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
            // Se abbiamo già una posizione riservata valida, la manteniamo
            if (reservedPosition.HasValue && !IsPositionOccupied(reservedPosition.Value.x, reservedPosition.Value.y))
            {
                targetX = reservedPosition.Value.x;
                targetY = reservedPosition.Value.y;
                return;
            }

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
                            reservedPosition = new Vector2Int(targetX, targetY);
                            return;
                        }
                    }
                }
            }

            // Se non troviamo una posizione migliore, rimaniamo dove siamo
            targetX = _currentX;
            targetY = _currentY;
        }

        bool IsPositionOccupied(int x, int y)
        {
            // Controlla edifici nemici e placeholder
            var enemyBuildings = BattleMap.Instance?.GetEnemyBuildings();
            if (enemyBuildings != null && enemyBuildings[x, y] != null)
            {
                return true;
            }

            // Controlla edifici nemici attraverso EnemyBuildingController
            var enemyBuildingsInScene = Physics.OverlapSphere(_buildGrid.GetCenterPosition1(x, y, 1, 1), 2f, buildingLayer);
            foreach (var collider in enemyBuildingsInScene)
            {
                if (collider.GetComponent<EnemyBuildingsController>() != null ||
                    collider.GetComponent<BuildingEnemyPlaceholder>() != null)
                {
                    return true;
                }
            }

            // Controlla altri arcieri e le loro posizioni riservate
            var allArchers = FindObjectsOfType<ArcherController>();
            foreach (var archer in allArchers)
            {
                if (archer != this)
                {
                    if ((archer.CurrentX == x && archer.CurrentY == y) ||
                        (archer.reservedPosition.HasValue &&
                         archer.reservedPosition.Value.x == x &&
                         archer.reservedPosition.Value.y == y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void MoveTowardsTarget()
        {
            if (currentTarget == null || isMoving) return;

            // Previene movimenti troppo frequenti
            if (Time.time - lastMovementTime < MOVEMENT_COOLDOWN) return;

            FindSuitableAttackPosition(currentTarget, out int targetGridX, out int targetGridY);

            // Muoviti solo se la nuova posizione è diversa e non sei già nel range di attacco
            if ((_currentX != targetGridX || _currentY != targetGridY) &&
                Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
            {
                lastMovementTime = Time.time;
                MoveToGridPosition(targetGridX, targetGridY);
            }
        }

        void UpdateAnimationBasedOnTarget()
        {
            if (currentTarget == null) return;

            // Calcoliamo la direzione nel world space
            Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;

            // Compensiamo la rotazione di 45 gradi della griglia
            // Ruotiamo la direzione di -45 gradi per allinearla con gli assi della griglia
            float angleRad = -45f * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angleRad);
            float sinAngle = Mathf.Sin(angleRad);
            Vector3 rotatedDirection = new Vector3(
                directionToTarget.x * cosAngle - directionToTarget.z * sinAngle,
                0,
                directionToTarget.x * sinAngle + directionToTarget.z * cosAngle
            );

            // Calcoliamo l'angolo della direzione ruotata
            float angle = Mathf.Atan2(rotatedDirection.z, rotatedDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            AnimationState newState = AnimationState.Idle;

            // Ora possiamo usare l'angolo compensato per determinare la direzione dell'animazione
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

            // Riserva la nuova posizione prima di iniziare il movimento
            reservedPosition = new Vector2Int(targetX, targetY);

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

            // Aggiorna la posizione riservata alla posizione finale
            reservedPosition = new Vector2Int(_currentX, _currentY);

            StopMoving();
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
                // Se non c'è un target, rilascia la posizione riservata
                reservedPosition = null;
            }
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
                        AttackManager.Instance?.ProcessAttack(enemyBuilding.GetUniqueId(), CurrentAttackTarget.name);
                       
                    }
                    enemyBuilding.TakeDamage(1);
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

        private GameObject FindNearestBuildingManually()
        {
            var enemyBuildings = FindObjectsOfType<EnemyBuildingsController>();
            Debug.Log("Enemy Buildings: " + enemyBuildings.Length);
            GameObject closestBuilding = null;
            float closestDistance = float.MaxValue;

            foreach (var building in enemyBuildings)
            {
                if (building.IsAlive())
                {
                    float distance = Vector2.Distance(transform.position, building.transform.position);
                    if (distance <= detectionRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = building.gameObject;
                    }
                }
            }

            return closestBuilding;
        }
    }
}