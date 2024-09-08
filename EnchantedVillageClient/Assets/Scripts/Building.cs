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
        [SerializeField] private int _rows = 1;
        [SerializeField] private int _columns = 1;
        [SerializeField] private Level[] levels;

        private int _currentX;
        private int _currentY;
        private BuildGrid _buildGrid;

        public int Rows => _rows;
        public int Columns => _columns;
        public int CurrentX => _currentX;
        public int CurrentY => _currentY;

        public string Name { get; set; } 

        private void Start()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();
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

        public Vector3 GetCenterPosition()
        {
            return _buildGrid.GetCenterPosition(_currentX, _currentY, _rows, _columns);
        }

        public void UpdateGridPosition(int x, int y)
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
}