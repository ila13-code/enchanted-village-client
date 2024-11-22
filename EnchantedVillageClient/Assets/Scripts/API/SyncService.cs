using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        public IEnumerator SyncGameData(Action onComplete = null, Action<string> onError = null)
        {
            if (isSyncing)
            {
                onComplete?.Invoke();
                yield break;
            }

            isSyncing = true;
            Debug.Log("=== INIZIO SINCRONIZZAZIONE ===");

            if (!ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? true)
            {
                onError?.Invoke("Non autenticato");
                isSyncing = false;
                yield break;
            }

            GameInformation localData = null;

            try
            {
                var buildings = PlayerPrefsController.Instance.GetBuildings();
                Debug.Log($"Buildings locali prima della sync: {JsonConvert.SerializeObject(buildings, Formatting.Indented)}");

                localData = new GameInformation
                {
                    level = PlayerPrefsController.Instance.Level,
                    experiencePoints = PlayerPrefsController.Instance.Exp,
                    elixir = PlayerPrefsController.Instance.Elixir,
                    gold = PlayerPrefsController.Instance.Gold,
                    buildings = buildings.Select(b => b.Clone()).ToList()
                };

                Debug.Log($"Dati locali preparati per l'invio: {JsonConvert.SerializeObject(localData, Formatting.Indented)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Errore nella preparazione dei dati locali: {e}");
                onError?.Invoke(e.Message);
                isSyncing = false;
                yield break;
            }

            bool syncComplete = false;
            string syncError = null;

            yield return StartCoroutine(ApiService.Instance.UpdateGameInformation(
                localData,
                onSuccess: (serverData) =>
                {
                    try
                    {
                        Debug.Log($"Dati ricevuti dal server: {JsonConvert.SerializeObject(serverData, Formatting.Indented)}");
                        MergeGameData(serverData);
                        lastSyncTime = Time.time;
                        syncComplete = true;

                        // Verifica finale
                        var finalBuildings = PlayerPrefsController.Instance.GetBuildings();
                        Debug.Log($"Buildings dopo il merge: {JsonConvert.SerializeObject(finalBuildings, Formatting.Indented)}");
                    }
                    catch (Exception e)
                    {
                        syncError = e.Message;
                        Debug.LogError($"Errore nel merge dei dati: {e}");
                    }
                },
                onError: (error) =>
                {
                    syncError = error;
                    Debug.LogWarning($"Sync failed: {error}");
                }
            ));

            if (syncComplete)
            {
                Debug.Log("Sincronizzazione completata con successo");
                onComplete?.Invoke();
            }
            else if (syncError != null)
            {
                onError?.Invoke(syncError);
            }

            Debug.Log("=== FINE SINCRONIZZAZIONE ===");
            isSyncing = false;
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

            // Prima aggiungiamo tutti gli edifici locali
            if (localBuildings != null)
            {
                foreach (var building in localBuildings)
                {
                    buildingMap[building.GetUniqueId()] = building;
                }
            }

            // Poi confrontiamo con i dati del server
            if (serverBuildings != null)
            {
                foreach (var serverBuilding in serverBuildings)
                {
                    string buildingId = serverBuilding.GetUniqueId();

                    if (buildingMap.TryGetValue(buildingId, out var localBuilding))
                    {
                        // Campo di addestramento (prefabIndex == 4)
                        if (serverBuilding.getPrefabIndex() == 4 && localBuilding.getPrefabIndex() == 4)
                        {
                            var localTroops = localBuilding.getTroopsData() ?? new List<TroopsData>();
                            var serverTroops = serverBuilding.getTroopsData() ?? new List<TroopsData>();

                            // Merge delle truppe mantenendo sia quelle locali che quelle del server
                            var mergedTroops = new List<TroopsData>();

                            // Aggiungi tutte le truppe locali
                            mergedTroops.AddRange(localTroops);

                            // Aggiungi le truppe del server che non sono già presenti localmente
                            foreach (var serverTroop in serverTroops)
                            {
                                bool troopExists = mergedTroops.Any(localTroop =>
                                    localTroop._x == serverTroop._x &&
                                    localTroop._y == serverTroop._y &&
                                    localTroop._z == serverTroop._z &&
                                    localTroop._type == serverTroop._type);

                                if (!troopExists)
                                {
                                    mergedTroops.Add(serverTroop);
                                }
                            }

                            // Limita il numero massimo di truppe a 5
                            if (mergedTroops.Count > 5)
                            {
                                mergedTroops = mergedTroops.Take(5).ToList();
                            }

                            // Crea un nuovo BuildingData con le truppe unite
                            var mergedBuilding = new BuildingData(
                                localBuilding.GetUniqueId(),
                                localBuilding.getPrefabIndex(),
                                localBuilding.getX(),
                                localBuilding.getY(),
                                localBuilding.getHealth());
                            mergedBuilding.setTroopsData(mergedTroops);

                            buildingMap[buildingId] = mergedBuilding;
                        }
                        else
                        {
                            // Per altri tipi di edifici, mantieni il comportamento esistente
                            buildingMap[buildingId] = serverBuilding;
                        }
                    }
                    else
                    {
                        // Se l'edificio non esiste localmente, aggiungi quello del server
                        buildingMap[buildingId] = serverBuilding;
                    }
                }
            }

            mergedBuildings.AddRange(buildingMap.Values);
            return mergedBuildings;
        }

    }
}
