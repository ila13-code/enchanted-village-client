using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class BattleBuilding : MonoBehaviour
    {
        public class BuildingEnemyPlaceholder : BattleBuilding
        {
            public BattleBuilding ParentBuilding { get; set; }
        }

        [SerializeField] private int _rows = 1;
        [SerializeField] private int _columns = 1;
        [SerializeField] private int _prefabIndex;
   

        private int _currentX;
        private int _currentY;
        private string _id;
        private BuildGrid _buildGrid;
 
        public int Rows => _rows;
        public int Columns => _columns;
        public int CurrentX => _currentX;
        public int CurrentY => _currentY;
        public string Id
        {
            get => _id;
            set => _id = value;
        }
        public string Name { get; set; }
        public int PrefabIndex => _prefabIndex;

        private void Start()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();
        }

        public void PlaceOnGrid(int x, int y)
        {
            if (_buildGrid == null)
            {
                _buildGrid = FindObjectOfType<BuildGrid>();
                if (_buildGrid == null)
                {
                    Debug.LogError("BuildGrid not found!");
                    return;
                }
            }
            _currentX = x;
            _currentY = y;
            Vector3 position = _buildGrid.GetCenterPosition(x, y, _rows, _columns);
            transform.position = position;
        }

     
    }
}