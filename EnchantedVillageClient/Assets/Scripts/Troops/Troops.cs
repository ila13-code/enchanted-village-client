using System.Collections;
using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;
using UnityEngine.UIElements;
namespace Unical.Demacs.EnchantedVillage
{
    public class Troops : MonoBehaviour
    {
        private int _rows = 1;
        private int _columns = 1;
        private int _currentX;
        private int _currentY;
        public int CurrentX => _currentX;
        public int CurrentY => _currentY;
        private BuildGrid _buildGrid;

        private void Awake()
        {
            _buildGrid = FindObjectOfType<BuildGrid>();

        }

        public void PlaceOnGrid(int x, int y, int numberOfTroops)
        {
            _currentX = x;
            _currentY = y;
            if (numberOfTroops > 3)
            {
                switch (numberOfTroops)
                {
                    case 4:
                        numberOfTroops = 1;
                        break;
                    case 5:
                        numberOfTroops = 2;
                        break;
                }
                y += numberOfTroops;
            }
            Vector3 position = _buildGrid.GetCenterPosition1(x + numberOfTroops, y, _rows, _columns);
            transform.position = position;
            Debug.Log("Troops placed at " + position.x + ", " + position.y);
        }
    }
}