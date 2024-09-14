using System.Collections;
using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

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

    public void PlaceOnGrid(int x, int y)
    {
        _currentX = x;
        _currentY = y;
        Vector3 position = _buildGrid.GetCenterPosition(x, y, _rows, _columns);
        transform.position = position;
    }
}
