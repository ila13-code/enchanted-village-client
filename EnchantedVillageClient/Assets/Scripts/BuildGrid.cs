using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class BuildGrid : MonoBehaviour
    {
        private int _nRows = 45;
        private int _nCols = 45;
        private float _cellSize = 1f;

        public float CellSize => _cellSize;
        public int Rows => _nRows;
        public int Columns => _nCols;

        public Vector3 GetNearestPointOnGrid(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            int xCount = Mathf.RoundToInt(localPosition.x / _cellSize);
            int zCount = Mathf.RoundToInt(localPosition.z / _cellSize);

            Vector3 result = new Vector3(
                xCount * _cellSize,
                0,
                zCount * _cellSize
            );

            return transform.TransformPoint(result);
        }

        public Vector3 GetCenterPosition(int x, int y, int rows, int columns)
        {
            Vector3 localStartPosition = new Vector3(x * _cellSize, 0, y * _cellSize);
            Vector3 localCenterOffset = new Vector3(columns * _cellSize / 2f, 0, rows * _cellSize / 2f);
            Vector3 localCenterPosition = localStartPosition + localCenterOffset;
            return transform.TransformPoint(localCenterPosition);
        }

        public (int, int) WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            int gridX = Mathf.FloorToInt(localPosition.x / _cellSize);
            int gridY = Mathf.FloorToInt(localPosition.z / _cellSize);
            return (gridX, gridY);
        }

        public bool IsPositionInMap(int gridX, int gridY, int buildingRows, int buildingColumns)
        {
            
            int maxGridX = gridX + buildingColumns - 1;
            int maxGridY = gridY + buildingRows - 1;

            bool isInsideHorizontalBounds = gridX >= 0 && maxGridX < this.Columns;
            bool isInsideVerticalBounds = gridY >= 0 && maxGridY < this.Rows;
            return isInsideHorizontalBounds && isInsideVerticalBounds;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for (int k = 0; k <= _nRows; k++)
            {
                Vector3 startPoint = transform.TransformPoint(new Vector3(0, 0, k * _cellSize));
                Vector3 endPoint = transform.TransformPoint(new Vector3(_nCols * _cellSize, 0, k * _cellSize));
                Gizmos.DrawLine(startPoint, endPoint);
            }
            for (int k = 0; k <= _nCols; k++)
            {
                Vector3 startPoint = transform.TransformPoint(new Vector3(k * _cellSize, 0, 0));
                Vector3 endPoint = transform.TransformPoint(new Vector3(k * _cellSize, 0, _nRows * _cellSize));
                Gizmos.DrawLine(startPoint, endPoint);
            }
        }
    }
}