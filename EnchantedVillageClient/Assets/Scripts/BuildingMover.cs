using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class BuildingMover : MonoBehaviour
    {
        private bool isDragging = false;
        private Vector3 offset;
        private Building building;
        private BuildGrid buildGrid;
        private Camera mainCamera;
        private Renderer baseRenderer;
        private BoxCollider buildingCollider;

        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        private void Start()
        {
            building = GetComponent<Building>();
            buildGrid = FindObjectOfType<BuildGrid>();
            mainCamera = Camera.main;
            baseRenderer = GetComponentInChildren<Renderer>();
            buildingCollider = GetComponentInChildren<BoxCollider>();
        }

        private void OnMouseDown()
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();
            BuildingMovementEvents.TriggerBuildingDragStart();
        }

        private void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                Vector3 newPosition = new Vector3(
                    mousePos.x + offset.x,
                    transform.position.y,
                    mousePos.z + offset.z
                );
                transform.position = newPosition;

                UpdatePlacementVisualization();
            }
        }

        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                if (CanPlaceBuilding())
                {
                    SnapToGrid();
                    baseRenderer.material = validPlacementMaterial;
                }
                else
                {
                    // L'edificio rimane "in mano" all'utente
                    baseRenderer.material = invalidPlacementMaterial;
                }
                BuildingMovementEvents.TriggerBuildingDragEnd();
            }
        }

        private void UpdatePlacementVisualization()
        {
            baseRenderer.material = CanPlaceBuilding() ? validPlacementMaterial : invalidPlacementMaterial;
        }

        private bool CanPlaceBuilding()
        {
            (int gridX, int gridY) = GetGridCoordinates();

            bool isInMap = buildGrid.IsPositionInMap(gridX, gridY, building.Rows, building.Columns);
            if (!isInMap)
            {
                return false;
            }

            return !HasCollisions();
        }

        private (int, int) GetGridCoordinates()
        {
            Vector3 localBuildingPosition = buildGrid.transform.InverseTransformPoint(transform.position);
            int gridX = Mathf.FloorToInt(localBuildingPosition.x / buildGrid.CellSize);
            int gridY = Mathf.FloorToInt(localBuildingPosition.z / buildGrid.CellSize);
            return (gridX, gridY);
        }

        private bool HasCollisions()
        {
            // L'utente non ha nessun edificio, non possono esserci collisioni
            if (Player.Instance.GetPlayerBuildings() == null)
            {
                return false;
            }

            (int gridX, int gridY) = GetGridCoordinates();
            Building[,] playerBuildings = Player.Instance.GetPlayerBuildings();
            for (int x = 0; x < building.Columns; x++)
            {
                for (int y = 0; y < building.Rows; y++)
                {
                    Debug.Log($"Checking for collision at {gridX + x}, {gridY + y}");
                    Debug.Log("Player buildings: " + playerBuildings.Length);
                    if (playerBuildings[gridX + x, gridY + y] != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SnapToGrid()
        {
            (int gridX, int gridY) = GetGridCoordinates();

            gridX = Mathf.Clamp(gridX, 0, buildGrid.Columns - building.Columns);
            gridY = Mathf.Clamp(gridY, 0, buildGrid.Rows - building.Rows);

            Vector3 snappedPosition = buildGrid.GetCenterPosition(gridX, gridY, building.Rows, building.Columns);
            transform.position = snappedPosition;

            building.UpdateGridPosition(gridX, gridY);
        }

        private Vector3 GetMouseWorldPosition()
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }
    }
}