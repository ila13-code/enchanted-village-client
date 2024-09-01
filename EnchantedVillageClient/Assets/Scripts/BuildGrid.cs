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
            position -= transform.position;
            int xCount = Mathf.RoundToInt(position.x / _cellSize);
            int yCount = Mathf.RoundToInt(position.z / _cellSize);

            Vector3 result = new Vector3(
                (float)xCount * _cellSize,
                0,
                (float)yCount * _cellSize
            );

            result += transform.position;
            return result;
        }

        public Vector3 GetCenterPosition(int x, int y, int rows, int columns)
        {
            Vector3 position = GetStartPosition(x, y);
            position += (transform.right * columns * _cellSize / 2f) + (transform.forward * rows * _cellSize / 2f);
            return position;
        }

        private Vector3 GetStartPosition(int x, int y)
        {
            Vector3 position = transform.position;
            position += (transform.right * _cellSize * x) + (transform.forward * _cellSize * y);
            return position;
        }

        public bool IsPositionInMap(Vector3 position, int x, int y, int rows, int columns)
        {
            position = transform.InverseTransformPoint(position);
            Rect rect = new Rect(x, y, columns, rows);
            return rect.Contains(new Vector2(position.x, position.z));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for (int k = 0; k <= _nRows; k++)
            {
                Vector3 point = transform.position + transform.forward * _cellSize * k;
                Gizmos.DrawLine(point, point + transform.right * _cellSize * _nCols);
            }
            for (int k = 0; k <= _nCols; k++)
            {
                Vector3 point = transform.position + transform.right * _cellSize * k;
                Gizmos.DrawLine(point, point + transform.forward * _cellSize * _nRows);
            }
        }
#endif
    }
}