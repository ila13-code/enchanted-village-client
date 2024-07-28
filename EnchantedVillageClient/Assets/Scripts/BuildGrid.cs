namespace Unical.Demacs.EnchantedVillage
{
    using UnityEngine;

    public class BuildGrid : MonoBehaviour
    {
        private int _nRows = 45;
        private int _nCols = 45;
        private float _cellSize = 1f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for(int k=0; k<=_nRows; k++)
            {
                Vector3 point=transform.position+transform.forward.normalized*_cellSize*(float)k;
                Gizmos.DrawLine(point, point+transform.right.normalized*_cellSize*(float)_nCols);
            }
            for (int k = 0; k <= _nCols; k++)
            {
                Vector3 point = transform.position + transform.right.normalized * _cellSize * (float)k;
                Gizmos.DrawLine(point, point + transform.forward.normalized * _cellSize * (float)_nRows);
            }
        }
    }
#endif
}
