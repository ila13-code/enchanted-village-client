using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unical.Demacs.EnchantedVillage.BattleBuilding;

namespace Unical.Demacs.EnchantedVillage
{
    public class BattleMap : MonoBehaviour
    {
        private static BattleMap instance = null;
        private BattleBuilding[,] EnemyBuildings;
        private Transform buildingsContainer;
        private bool isDataLoaded = false;
        private string ENEMY_EMAIL = "admin@admin.com";

        public static BattleMap Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<BattleMap>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("BattleMap");
                        instance = go.AddComponent<BattleMap>();
                    }
                }
                return instance;
            }
        }

        public BattleBuilding[,] GetEnemyBuildings()  
        {
            return EnemyBuildings;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                ENEMY_EMAIL = PlayerPrefs.GetString("battleFriendEmail", ENEMY_EMAIL);
                InitializeContainers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false)
            {
                LoadEnemyMap();
            }
        }

        private void InitializeContainers()
        {
            GameObject enemyMap = GameObject.Find("MapEnemy");
            if (enemyMap != null)
            {
                buildingsContainer = enemyMap.transform.Find("Buildings").transform;
            }
            else
            {
                Debug.LogError("MapEnemy non trovato nella scena.");
            }
        }

        public void LoadEnemyMap()
        {
            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false)
            {
                Debug.Log("Caricamento mappa nemica dal server...");
                StartCoroutine(LoadEnemyMapFromServer());
            }
        }

        private IEnumerator LoadEnemyMapFromServer()
        {
            bool operationComplete = false;

            yield return StartCoroutine(ApiService.Instance.GetGameInformationByEmail(
                ENEMY_EMAIL,
                onSuccess: (gameInfo) =>
                {
                    if (gameInfo != null)
                    {
                        Debug.Log("Caricamento dati nemici dal server...");
                        LoadEnemyMapFromServerData(gameInfo);
                    }
                    else
                    {
                        Debug.LogError("Nessun dato nemico trovato sul server.");
                    }
                    operationComplete = true;
                },
                onError: (error) =>
                {
                    Debug.LogError($"Errore nel caricamento dei dati nemici: {error}");
                    operationComplete = true;
                }
            ));

            while (!operationComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void LoadEnemyMapFromServerData(GameInformation gameInfo)
        {
            EnemyBuildings = new BattleBuilding[45, 45];  
            LoadBuildingsFromData(gameInfo.buildings);
            isDataLoaded = true;
        }

        private void LoadBuildingsFromData(IList<BuildingData> buildings)
        {
            if (buildings == null)
            {
                Debug.LogError("Lista edifici nemici null");
                return;
            }

            Debug.Log($"Numero di edifici nemici da caricare: {buildings.Count}");

            foreach (BuildingData data in buildings)
            {
                if (data == null || data.getPrefabIndex() < 0 ||
                    data.getPrefabIndex() >= UIController.Instance.EnemyBuildings.Length)
                {
                    continue;
                }

                Vector3 position = new Vector3(data.getX(), 0, data.getY());
                BattleBuilding buildingPrefab = UIController.Instance.EnemyBuildings[data.getPrefabIndex()];

                if (buildingPrefab == null)
                {
                    continue;
                }

                BattleBuilding building = Instantiate(buildingPrefab, position, Quaternion.identity, buildingsContainer);
                building.Id = data.GetUniqueId();

                if (building.PrefabIndex == 4) // Training base
                {
                    building.name = building.Id;
                    LoadTroopsForTrainingBase(data, building);
                }

                building.PlaceOnGrid(data.getX(), data.getY());

                // Aggiorna la griglia con l'edificio e i suoi placeholder
                for (int i = 0; i < building.Rows && (data.getX() + i) < 45; i++)
                {
                    for (int j = 0; j < building.Columns && (data.getY() + j) < 45; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            EnemyBuildings[data.getX() + i, data.getY() + j] = building;
                        }
                        else
                        {
                            var placeholder = building.gameObject.AddComponent<BuildingEnemyPlaceholder>();  
                            placeholder.ParentBuilding = building;
                            EnemyBuildings[data.getX() + i, data.getY() + j] = placeholder; 
                        }
                    }
                }
            }

            Debug.Log("Caricamento degli edifici nemici completato.");
        }

        private void LoadTroopsForTrainingBase(BuildingData data, BattleBuilding building)  
        {
            List<TroopsData> troopsData = data.getTroopsData();
            if (troopsData == null || troopsData.Count == 0)
            {
                return;
            }

            Transform troopsContainer = building.transform.Find("TroopsContainer");
            if (troopsContainer == null)
            {
                GameObject container = new GameObject("TroopsContainer");
                container.transform.SetParent(building.transform);
                troopsContainer = container.transform;
            }

            // TODO: Implementare la logica per le truppe nemiche
        }
    }
}