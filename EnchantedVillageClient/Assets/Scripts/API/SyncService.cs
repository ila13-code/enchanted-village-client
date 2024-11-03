using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Unical.Demacs.EnchantedVillage
{
    public class GameSyncManager : MonoBehaviour
    {
        private const float AUTO_SYNC_INTERVAL = 300f; // 5 minutes
        private float lastSyncTime;
        private bool isSyncing;

        public static GameSyncManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false)
            {
                if (Time.time - lastSyncTime >= AUTO_SYNC_INTERVAL && !isSyncing)
                {
                    StartCoroutine(SyncGameData());
                }
            }
        }

        public IEnumerator SyncGameData()
        {
            if (isSyncing) yield break;

            isSyncing = true;

            try
            {
                if (!ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? true)
                {
                    Debug.Log("Not authenticated, skipping sync");
                    yield break;
                }

                var localData = new GameInformation
                {
                    level = PlayerPrefsController.Instance.Level,
                    experiencePoints = PlayerPrefsController.Instance.Exp,
                    elixir = PlayerPrefsController.Instance.Elixir,
                    gold = PlayerPrefsController.Instance.Gold,
                    buildings = PlayerPrefsController.Instance.GetBuildings()
                };

                bool syncComplete = false;
                string syncError = null;

                yield return StartCoroutine(ApiService.Instance.UpdateGameInformation(
                    localData,
                    onSuccess: (serverData) =>
                    {
                        MergeGameData(serverData);
                        lastSyncTime = Time.time;
                        syncComplete = true;
                    },
                    onError: (error) =>
                    {
                        syncError = error;
                        Debug.LogWarning($"Sync failed: {error}. Continuing with local data.");
                    }
                ));

                if (syncComplete)
                {
                    Debug.Log("Game sync completed successfully");
                }
                else if (syncError != null)
                {
                    Debug.LogWarning($"Game sync failed: {syncError}");
                }
            }
            finally
            {
                isSyncing = false;
            }
        }

        private void MergeGameData(GameInformation serverData)
        {
            if (serverData == null) return;

            PlayerPrefsController.Instance.Level = Mathf.Max(PlayerPrefsController.Instance.Level, serverData.level);
            PlayerPrefsController.Instance.Exp = Mathf.Max(PlayerPrefsController.Instance.Exp, serverData.experiencePoints);
            PlayerPrefsController.Instance.Elixir = Mathf.Max(PlayerPrefsController.Instance.Elixir, serverData.elixir);
            PlayerPrefsController.Instance.Gold = Mathf.Max(PlayerPrefsController.Instance.Gold, serverData.gold);

            if (serverData.buildings != null && serverData.buildings.Count > 0)
            {
                var currentBuildings = PlayerPrefsController.Instance.GetBuildings();
                var mergedBuildings = MergeBuildings(currentBuildings, serverData.buildings);
                PlayerPrefsController.Instance.SaveBuildings(mergedBuildings);
            }
        }


        private List<BuildingData> MergeBuildings(List<BuildingData> localBuildings, List<BuildingData> serverBuildings)
        {
            var mergedBuildings = new List<BuildingData>();
            var buildingMap = new Dictionary<string, BuildingData>();

            if (localBuildings != null)
            {
                foreach (var building in localBuildings)
                {
                    buildingMap[building.GetUniqueId()] = building;
                }
            }

            if (serverBuildings != null)
            {
                foreach (var serverBuilding in serverBuildings)
                {
                    if (buildingMap.TryGetValue(serverBuilding.GetUniqueId(), out var localBuilding))
                    {
                        if (serverBuilding.getPrefabIndex() == 4 && localBuilding.getPrefabIndex() == 4)
                        {
                            if ((serverBuilding.getTroopsData()?.Count ?? 0) > (localBuilding.getTroopsData()?.Count ?? 0))
                            {
                                buildingMap[serverBuilding.GetUniqueId()] = serverBuilding;
                            }
                        }
                        else
                        {
                            buildingMap[serverBuilding.GetUniqueId()] = serverBuilding;
                        }
                    }
                    else
                    {
                        buildingMap[serverBuilding.GetUniqueId()] = serverBuilding;
                    }
                }
            }

            mergedBuildings.AddRange(buildingMap.Values);
            return mergedBuildings;
        }
    }
}
