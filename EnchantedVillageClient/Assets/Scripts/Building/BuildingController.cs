using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class BuildingController : MonoBehaviour
    {
        private bool isDragging = false;
        private Vector3 offset;
        private Building building;
        private BuildGrid buildGrid;
        private Camera mainCamera;
        private Renderer baseRenderer;

        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        private void Start()
        {
            building = GetComponent<Building>();
            buildGrid = FindObjectOfType<BuildGrid>();
            mainCamera = Camera.main;
            baseRenderer = GetComponentInChildren<Renderer>();
            SnapToGrid();
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
            if (Player.Instance == null || Player.Instance.GetPlayerBuildings() == null || buildGrid == null)
            {
                Debug.LogError("Critical components are null in HasCollisions");
                return false;
            }

            (int gridX, int gridY) = GetGridCoordinates();
            Building[,] playerBuildings = Player.Instance.GetPlayerBuildings();

            for (int x = gridX - 1; x < gridX + building.Columns + 1; x++)
            {
                for (int y = gridY - 1; y < gridY + building.Rows + 1; y++)
                {
                    if (x < 0 || y < 0 || x >= buildGrid.Columns || y >= buildGrid.Rows)
                        continue;

                    var cellContent = playerBuildings[x, y];
                    if (cellContent != null && cellContent != building)
                    {
                        Building otherBuilding = cellContent;

                        if (cellContent is Building.BuildingPlaceholder placeholder)
                        {
                            otherBuilding = placeholder.ParentBuilding;
                        }

 
                        if (otherBuilding == building)
                            continue;

                        int minDistanceX = otherBuilding.Columns / 2 + building.Columns / 2 + 1;
                        int minDistanceY = otherBuilding.Rows / 2 + building.Rows / 2 + 1;

                        if (Mathf.Abs(gridX + building.Columns / 2 - (otherBuilding.CurrentX + otherBuilding.Columns / 2)) < minDistanceX &&
                            Mathf.Abs(gridY + building.Rows / 2 - (otherBuilding.CurrentY + otherBuilding.Rows / 2)) < minDistanceY)
                        {
                            return true; 
                        }
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

        public void Confirm()
        {
            if (!CanPlaceBuilding())
            {
                Debug.LogWarning("Cannot confirm building placement: invalid position");
                baseRenderer.material = invalidPlacementMaterial;
            }
            else
                building.Confirm();
        }

    }
}