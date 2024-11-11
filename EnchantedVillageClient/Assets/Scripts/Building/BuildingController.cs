using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    //classe che gestisce il movimento degli edifici
    public class BuildingController : MonoBehaviour
    {
        private bool isDragging = false;
        private bool isMoving = false;
        private Vector3 offset;
        private Building building;
        private BuildGrid buildGrid;
        private Camera mainCamera;
        private Renderer baseRenderer;

        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private GameObject particles;

        private Vector3 initialMousePosition;
        private float dragThreshold = 0.1f;



        private void Start()
        {
            building = GetComponent<Building>();
            buildGrid = FindObjectOfType<BuildGrid>();
            mainCamera = Camera.main;
            baseRenderer = GetComponentInChildren<Renderer>();
        }

        //funzione che gestisce il click del mouse sull'edificio
        private void OnMouseDown()
        {
            initialMousePosition = GetMouseWorldPosition();
            isDragging = true;
            isMoving = false;
            offset = transform.position - initialMousePosition;
            BuildingMovementEvents.TriggerBuildingDragStart();
        }

        //funzione che gestisce il trascinamento dell'edificio
        //se l'edificio viene trascinato, viene aggiornata la posizione
        //distingue tra trascinamento e click (per la raccolta di risorse)
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

                if (Vector3.Distance(initialMousePosition, GetMouseWorldPosition()) > dragThreshold)
                {
                    if (isMoving == false)
                    {
                        ServicesManager.Instance.AudioManager.PlaceBuilding();
                        ServicesManager.Instance.AudioManager.SetMusicVolume(0.7f);
                    }
                    isMoving = true;
                    transform.position = newPosition;
                    UpdatePlacementVisualization();
                }
            }
        }

        //funzione che gestisce il rilascio del mouse
        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;

                if (!isMoving)
                {
                    // Colleziona risorse
                    ResourceCollector resourceCollector = GetComponent<ResourceCollector>();
                    if (resourceCollector != null)
                    {
                        if (building.PrefabIndex == 10) // Colleziono elisir
                        {
                            Debug.Log($"Colleziono elisir: {resourceCollector.Resources}");
                            PlayerPrefsController.Instance.Elixir += resourceCollector.Resources;
                            resourceCollector.Resources = 0;
                            if (particles != null)
                            {
                                ServicesManager.Instance.AudioManager.CollectResource();
                                ServicesManager.Instance.AudioManager.SetMusicVolume(0.7f);
                                particles.SetActive(false);
                            }
                        }
                        else if (building.PrefabIndex == 12) // Colleziono oro
                        {
                            Debug.Log($"Colleziono oro: {resourceCollector.Resources}");
                            PlayerPrefsController.Instance.Elixir += resourceCollector.Resources;
                            resourceCollector.Resources = 0;
                            if (particles != null)
                            {
                                ServicesManager.Instance.AudioManager.CollectResource();
                                ServicesManager.Instance.AudioManager.SetMusicVolume(0.7f);
                                particles.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ResourceCollector non trovato sul GameObject.");
                    }
                }
                else //sposto l'edificio
                {
                    if (CanPlaceBuilding()) // Se posso posizionare l'edificio
                    {
                        SnapToGrid(); // Posiziono l'edificio
                        baseRenderer.material = validPlacementMaterial; // Cambio il materiale
                    }
                    else
                    {
                        baseRenderer.material = invalidPlacementMaterial; // Cambio il materiale se non posso posizionare l'edificio
                    }
                }

                BuildingMovementEvents.TriggerBuildingDragEnd(); // Trigger dell'evento di fine trascinamento
            }
        }

        private void UpdatePlacementVisualization() // Aggiorna la visualizzazione del posizionamento
        {
            baseRenderer.material = CanPlaceBuilding() ? validPlacementMaterial : invalidPlacementMaterial;
        }

        private bool CanPlaceBuilding() // Verifica se posso posizionare l'edificio
        {
            (int gridX, int gridY) = GetGridCoordinates();
            bool isInMap = buildGrid.IsPositionInMap(gridX, gridY, building.Rows, building.Columns);
            if (!isInMap)
            {
                return false;
            }
            return !HasCollisions();
        }

        private (int, int) GetGridCoordinates() // Ottiene le coordinate della griglia a partire dalla posizione dell'edificio
        {
            Vector3 localBuildingPosition = buildGrid.transform.InverseTransformPoint(transform.position);
            int gridX = Mathf.FloorToInt(localBuildingPosition.x / buildGrid.CellSize);
            int gridY = Mathf.FloorToInt(localBuildingPosition.z / buildGrid.CellSize);
            return (gridX, gridY);
        }

        private bool HasCollisions() // Verifica se ci sono collisioni
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

        private void SnapToGrid() // Posiziona l'edificio sulla griglia
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

        public void Confirm() // Conferma il posizionamento dell'edificio
        {
            Debug.Log("Confirm method called");

            if (!CanPlaceBuilding())
            {
                Debug.LogWarning("Cannot confirm building placement: invalid position");
                baseRenderer.material = invalidPlacementMaterial;
            }
            else
            {
                ServicesManager.Instance.AudioManager.PlaceBuilding();
                ServicesManager.Instance.AudioManager.SetMusicVolume(0.7f);
                building.Confirm(true);
            }
        }
    }
}
