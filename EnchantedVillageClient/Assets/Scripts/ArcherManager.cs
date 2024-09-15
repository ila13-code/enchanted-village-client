using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

public class ArcherManager : MonoBehaviour
{
    public static ArcherManager Instance;
    private List<ArcherController> archers = new List<ArcherController>();
    private BuildGrid buildGrid;

    public bool canMoveArchers = false;  // Nuova variabile

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        buildGrid = FindObjectOfType<BuildGrid>();
    }

    private void Start()
    {
        archers.AddRange(FindObjectsOfType<ArcherController>());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked");
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 nearestGridPoint = buildGrid.GetNearestPointOnGrid(mouseWorldPos);
            (int gridX, int gridY) = buildGrid.WorldToGridPosition(nearestGridPoint);

            MoveAllArchersToPosition(gridX, gridY);
        }
    }

    private void MoveAllArchersToPosition(int x, int y)
    {
        canMoveArchers = true;  // Permetti il movimento
        foreach (var archer in archers)
        {
            archer.MoveToGridPosition(x, y);
        }
    }
}
