using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{

    public class Building : MonoBehaviour
    {
        public class BuildingPlaceholder : Building
        {
            public Building ParentBuilding { get; set; }
        }
        private Renderer baseRenderer;

        [SerializeField] private Material placedBuildingMaterial;
        [SerializeField] private int _rows = 1;
        [SerializeField] private int _columns = 1;
        [SerializeField] private GameObject _button;

        [SerializeField] private int _prefabIndex;
        private int _currentX;
        private int _currentY;
        private bool _isConfirmed;
        private bool _isMoving;
        private int _currentHealth;
        private String _id;
        private BuildGrid _buildGrid;

        public int Rows => _rows;
        public int Columns => _columns;
        public int CurrentX => _currentX;
        public int CurrentY => _currentY;

        public String Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name { get; set; } 

        public bool IsConfirmed => _isConfirmed;
        public int PrefabIndex => _prefabIndex;

        private void Start()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();
            baseRenderer = GetComponentInChildren<Renderer>();
            if (IsConfirmed) //se il building � stato confermato vuol dire che c'era gi�
            {
                baseRenderer.material = placedBuildingMaterial;
                _button.SetActive(false);
            }
        }

        // Funzione che posiziona l'edificio sulla griglia
        public void PlaceOnGrid(int x, int y)
        {
            _currentX = x;
            _currentY = y;
            _currentHealth = 100;
            Vector3 position = _buildGrid.GetCenterPosition(x, y, _rows, _columns);
            transform.position = position;
        }

        //todo: integrare col pulsante di rotazione
        public void Rotate()
        {
            transform.Rotate(Vector3.up, 90);
            int temp = _rows;
            _rows = _columns;
            _columns = temp;
            PlaceOnGrid(_currentX, _currentY);
        }

        // Funzione che aggiorna la posizione dell'edificio sulla griglia
        public void UpdateGridPosition(int x, int y)
        {
            if (_isConfirmed)
            {
                _isConfirmed = false;
            }
            if (!_isConfirmed)
            {
                _button.SetActive(true);
                _isMoving = true;
            }
            if (_isMoving)
            { 
                if (_currentX >= 0 && _currentY >= 0)
                {
                    for (int i = 0; i < this.Rows; i++)
                    {
                        for (int j = 0; j < this.Columns; j++)
                        {
                            var cell = Player.Instance.GetPlayerBuildings()[_currentX + i, _currentY + j];
                            if (cell is BuildingPlaceholder placeholder)
                            {
                                Destroy(placeholder);
                            }
                            Player.Instance.GetPlayerBuildings()[_currentX + i, _currentY + j] = null;
                        }
                    }
                }

                _currentX = x;
                _currentY = y;

                for (int i = 0; i < this.Rows; i++)
                {
                    for (int j = 0; j < this.Columns; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            Player.Instance.GetPlayerBuildings()[x + i, y + j] = this;
                        }
                        else
                        {
                            var placeholder = gameObject.AddComponent<BuildingPlaceholder>();
                            placeholder.ParentBuilding = this;
                            Player.Instance.GetPlayerBuildings()[x + i, y + j] = placeholder;
                        }
                    }
                }
            }
        }

        // Funzione che conferma la posizione dell'edificio sulla griglia e salva le modifiche nel PlayerPrefs
        public void Confirm(Boolean create)
        {
            try
            {
                _buildGrid = FindObjectOfType<BuildGrid>();
                baseRenderer = GetComponentInChildren<Renderer>();
                if (baseRenderer == null)
                {
                    Debug.LogError($"baseRenderer � null per l'edificio {gameObject.name}");
                    return;
                }
                if (placedBuildingMaterial == null)
                {
                    Debug.LogError($"placedBuildingMaterial � null per l'edificio {gameObject.name}");
                    return;
                }
                baseRenderer.material = placedBuildingMaterial;

                _isConfirmed = true;
                _isMoving = false;

                if (_button == null)
                {
                    Debug.LogError($"_button � null per l'edificio {gameObject.name}");
                }
                else
                {
                    _button.SetActive(false);
                }

                if (PlayerPrefsController.Instance == null)
                {
                    Debug.LogError("PlayerPrefsController.Instance � null");
                    return;
                }
                List<BuildingData> list = PlayerPrefsController.Instance.GetBuildings();
                if (list == null)
                {
                    Debug.LogError("La lista di BuildingData � null");
                    list = new List<BuildingData>();
                }

                if (_prefabIndex < 0 && _prefabIndex>13)
                {
                    Debug.LogError($"_prefabIndex non valido: {_prefabIndex}");
                    return;
                }


                var existingBuilding = list.FirstOrDefault(b => b.GetUniqueId() == _id);

                if (existingBuilding != null)
                {
                    Debug.Log($"Edificio gi� esistente: {_prefabIndex} {existingBuilding.getX()} {existingBuilding.getY()}");

                    // Crea un nuovo BuildingData mantenendo i dati delle truppe esistenti
                    var updatedBuildingData = new BuildingData(this.Id, _prefabIndex, _currentX, _currentY, _currentHealth);

                    // Se l'edificio � una Training Base (assumendo che _prefabIndex 4 sia la Training Base)
                    if (_prefabIndex == 4)
                    {
                        // Trasferisci i dati delle truppe esistenti al nuovo BuildingData
                        updatedBuildingData.setTroopsData(existingBuilding.getTroopsData());

                    }

                    list.Remove(existingBuilding);
                    list.Add(updatedBuildingData);
                    Debug.Log($"Aggiornato: {_prefabIndex} {_currentX} {_currentY}");
                }
                else if (create)
                {
                    // Aggiungi un nuovo edificio solo se non esiste e create � true
                    list.Add(new BuildingData(this.Id, _prefabIndex, _currentX, _currentY, _currentHealth));
                    Debug.Log($"Aggiunto: {_prefabIndex} {_currentX} {_currentY}");
                }

                PlayerPrefsController.Instance.SaveBuildings(list);
                Debug.Log($"Edificio confermato: {gameObject.name}");
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"NullReferenceException in Confirm per l'edificio {gameObject.name}: {e.Message}");
                Debug.LogError($"StackTrace: {e.StackTrace}");
                return;
            }
        }

        public void ConfirmLoadBuildings()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();
            baseRenderer = GetComponentInChildren<Renderer>();
            if (baseRenderer == null)
            {
                Debug.LogError($"baseRenderer � null per l'edificio {gameObject.name}");
                return;
            }
            if (placedBuildingMaterial == null)
            {
                Debug.LogError($"placedBuildingMaterial � null per l'edificio {gameObject.name}");
                return;
            }
            baseRenderer.material = placedBuildingMaterial;

            _isConfirmed = true;
            _isMoving = false;

            if (_button == null)
            {
                Debug.LogError($"_button � null per l'edificio {gameObject.name}");
            }
            else
            {
                _button.SetActive(false);
            }
        }

        // Funzione che rimuove l'edificio dalla griglia e dai dati salvati
        public void Cancel()
        {
            if (_currentX >= 0 && _currentY >= 0)
            {
                // Rimuovi l'edificio dalla griglia di gioco
                for (int i = 0; i < this.Rows; i++)
                {
                    for (int j = 0; j < this.Columns; j++)
                    {
                        var cell = Player.Instance.GetPlayerBuildings()[_currentX + i, _currentY + j];
                        if (cell is BuildingPlaceholder placeholder)
                        {
                            Destroy(placeholder);
                        }
                        Player.Instance.GetPlayerBuildings()[_currentX + i, _currentY + j] = null;
                    }
                }
            }

            // Rimuovi l'edificio dai dati salvati
            if (PlayerPrefsController.Instance != null)
            {
                List<BuildingData> list = PlayerPrefsController.Instance.GetBuildings();
                if (list != null)
                {
                    var buildingToRemove = list.FirstOrDefault(b => b.GetUniqueId() == _id);
                    if (buildingToRemove != null)
                    {
                        list.Remove(buildingToRemove);
                        PlayerPrefsController.Instance.SaveBuildings(list);
                        Debug.Log($"Edificio rimosso: {_prefabIndex}");
                    }
                }
            }

            // Distruggi il GameObject associato all'edificio
            Destroy(gameObject);
        }


    }
}