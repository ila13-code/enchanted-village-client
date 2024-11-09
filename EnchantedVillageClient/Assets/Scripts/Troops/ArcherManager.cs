using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;

public class ArcherManager : MonoBehaviour
{
    public static ArcherManager Instance;
    private List<ArcherController> archers = new List<ArcherController>();
    private BuildGrid buildGrid;
    public bool canMoveArchers = false;

    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private Transform archersContainer;
    [SerializeField] private int maxArchers = 10;
    [SerializeField] private int archersPerSpawn = 3;

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

        if (archersContainer == null)
        {
            GameObject container = new GameObject("ArchersContainer");
            archersContainer = container.transform;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float hitDistance))
            {
                Vector3 hitPoint = ray.GetPoint(hitDistance);
                Vector3 nearestGridPoint = buildGrid.GetNearestPointOnGrid(hitPoint);
                (int gridX, int gridY) = buildGrid.WorldToGridPosition(nearestGridPoint);

                Debug.Log($"Click Position: World={hitPoint}, Grid=({gridX}, {gridY})");
                SpawnArchersAtGrid(gridX, gridY);
            }
        }
    }

    private void SpawnArchersAtGrid(int centerX, int centerY)
    {
        if (archers.Count >= maxArchers)
        {
            Debug.Log("Numero massimo di arcieri raggiunto!");
            return;
        }

        // Posizioni relative per lo spawn degli arcieri (modello a croce)
        Vector2Int[] spawnOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Centro
            new Vector2Int(1, 0),   // Destra
            new Vector2Int(-1, 0),  // Sinistra
            new Vector2Int(0, 1),   // Sopra
            new Vector2Int(0, -1),  // Sotto
        };

        int archersSpawned = 0;
        int maxToSpawn = Mathf.Min(archersPerSpawn, maxArchers - archers.Count);

        foreach (Vector2Int offset in spawnOffsets)
        {
            if (archersSpawned >= maxToSpawn) break;

            int spawnX = centerX + offset.x;
            int spawnY = centerY + offset.y;

            Debug.Log($"Trying to spawn at grid position: ({spawnX}, {spawnY})");

            if (buildGrid.IsPositionInMap(spawnX, spawnY, 1, 1))
            {
                Vector3 spawnPosition = buildGrid.GetCenterPosition1(spawnX, spawnY, 1, 1);

                GameObject archerObj = Instantiate(archerPrefab, spawnPosition, Quaternion.identity, archersContainer);
                ArcherController archer = archerObj.GetComponent<ArcherController>();

                if (archer != null)
                {
                    archers.Add(archer);
                    canMoveArchers = true;
                    archersSpawned++;
                    Debug.Log($"Successfully spawned archer at ({spawnX}, {spawnY})");
                }
            }
            else
            {
                Debug.Log($"Position ({spawnX}, {spawnY}) is invalid for spawning");
            }
        }

        if (archersSpawned > 0)
        {
            StartAttacking();
        }
    }

    private void StartAttacking()
    {
        foreach (var archer in archers)
        {
            if (archer != null)
            {
                archer.gameObject.SetActive(true);
                canMoveArchers = true;
            }
        }
    }

    public void RemoveArcher(ArcherController archer)
    {
        if (archer != null && archers.Contains(archer))
        {
            archers.Remove(archer);
            Destroy(archer.gameObject);
        }
    }

    private void OnDestroy()
    {
        foreach (var archer in archers.ToArray())
        {
            if (archer != null)
            {
                Destroy(archer.gameObject);
            }
        }
        archers.Clear();
    }
}