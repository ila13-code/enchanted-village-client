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

        private void Start()
        {
            building = GetComponent<Building>();
            buildGrid = FindObjectOfType<BuildGrid>();
            mainCamera = Camera.main;
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
            }
        }

        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                SnapToGrid();
                BuildingMovementEvents.TriggerBuildingDragEnd();
            }
        }

        private void SnapToGrid()
        {
            Vector3 buildingPosition = transform.position - buildGrid.transform.position;

            int gridX = Mathf.FloorToInt(buildingPosition.x / buildGrid.CellSize);
            int gridY = Mathf.FloorToInt(buildingPosition.z / buildGrid.CellSize);

            gridX = Mathf.Clamp(gridX, 0, buildGrid.Columns - building.Columns);
            gridY = Mathf.Clamp(gridY, 0, buildGrid.Rows - building.Rows);

            Vector3 snappedPosition = buildGrid.transform.position +
                new Vector3(gridX * buildGrid.CellSize, 0, gridY * buildGrid.CellSize);

            snappedPosition += new Vector3(building.Columns * buildGrid.CellSize / 2, 0, building.Rows * buildGrid.CellSize / 2);

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