using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{

    public class Building : MonoBehaviour
    {
        [System.Serializable]
        public class Level
        {
            public int level;
            public Sprite icon;
            public GameObject mesh;
        }

        public class BuildingPlaceholder : Building
        {
            public Building ParentBuilding { get; set; }
        }
        private Renderer baseRenderer;

        [SerializeField] private Material placedBuildingMaterial;
        [SerializeField] private int _rows = 1;
        [SerializeField] private int _columns = 1;
        [SerializeField] private Level[] levels;
        [SerializeField] private GameObject _button;

        [SerializeField] private int _prefabIndex;
        private int _currentX;
        private int _currentY;
        private bool _isConfirmed;
        private bool _isMoving;
        private BuildGrid _buildGrid;

        public int Rows => _rows;
        public int Columns => _columns;
        public int CurrentX => _currentX;
        public int CurrentY => _currentY;

        public string Name { get; set; } 

        public bool IsConfirmed => _isConfirmed;
        public int PrefabIndex => _prefabIndex;

        private void Start()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();
            baseRenderer = GetComponentInChildren<Renderer>();
            if (IsConfirmed) //se il building è stato confermato vuol dire che c'era già
            {
                baseRenderer.material = placedBuildingMaterial;
                _button.SetActive(false);
            }
        }

        public void PlaceOnGrid(int x, int y)
        {
            _currentX = x;
            _currentY = y;
            Vector3 position = _buildGrid.GetCenterPosition(x, y, _rows, _columns);
            transform.position = position;
        }

        public void Rotate()
        {
            transform.Rotate(Vector3.up, 90);
            int temp = _rows;
            _rows = _columns;
            _columns = temp;
            PlaceOnGrid(_currentX, _currentY);
        }


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


        public void Confirm(Boolean create)
        {
            try
            {
                _buildGrid = FindObjectOfType<BuildGrid>();
                baseRenderer = GetComponentInChildren<Renderer>();
                if (baseRenderer == null)
                {
                    Debug.LogError($"baseRenderer è null per l'edificio {gameObject.name}");
                    return;
                }
                if (placedBuildingMaterial == null)
                {
                    Debug.LogError($"placedBuildingMaterial è null per l'edificio {gameObject.name}");
                    return;
                }
                baseRenderer.material = placedBuildingMaterial;

                _isConfirmed = true;
                _isMoving = false;

                if (_button == null)
                {
                    Debug.LogError($"_button è null per l'edificio {gameObject.name}");
                }
                else
                {
                    _button.SetActive(false);
                }

                if (PlayerPrefsController.Instance == null)
                {
                    Debug.LogError("PlayerPrefsController.Instance è null");
                    return;
                }
                List<BuildingData> list = PlayerPrefsController.Instance.GetBuildings();
                if (list == null)
                {
                    Debug.LogError("La lista di BuildingData è null");
                    list = new List<BuildingData>();
                }

                if (_prefabIndex < 0)
                {
                    Debug.LogError($"_prefabIndex non valido: {_prefabIndex}");
                    return;
                }

                
                var existingBuilding = list.FirstOrDefault(b => b.getPrefabIndex() == _prefabIndex);

                if (existingBuilding != null)
                {
                    existingBuilding.setX(_currentX);
                    existingBuilding.setY(_currentY);
                    Debug.Log($"Aggiornato: {_prefabIndex} {_currentX} {_currentY}");
                }
                else if (create)
                {
                    // Aggiungi un nuovo edificio solo se non esiste e create è true
                    list.Add(new BuildingData(_prefabIndex, _currentX, _currentY));
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
                    var buildingToRemove = list.FirstOrDefault(b => b.getPrefabIndex() == _prefabIndex);
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