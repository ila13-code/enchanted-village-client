using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static Unical.Demacs.EnchantedVillage.Building;
using System;
using UnityEditor.Build;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;

namespace Unical.Demacs.EnchantedVillage
{
    public class Player : MonoBehaviour
    {
        private static Player instance = null;
        private int level;
        private int experiencePoints;
        private Building[,] PlayerBuildings;
        [SerializeField] private Troops[] troops;
        private Transform buildingsContainer;
        private Transform troopsContainer;
        private bool isDataLoaded = false;
        private bool useLocalData = false;

        public static Player Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Player>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("Player");
                        instance = go.AddComponent<Player>();
                    }
                }
                return instance;
            }
        }


        public Building[,] GetPlayerBuildings()
        {
            return PlayerBuildings;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                InitializeContainers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadGame();
        }



        private void InitializeContainers()
        {
            GameObject map = GameObject.Find("Map");
            if (map != null)
            {
                buildingsContainer = map.transform.Find("Buildings").transform;
                troopsContainer = map.transform.Find("Troops").transform;
            }
            else
            {
                Debug.Log("Map non trovato nella scena.");
            }
        }

        public void LoadGame()
        {
            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false && !useLocalData)
            {
                Debug.Log("Tentativo di caricamento dal server...");
                StartCoroutine(LoadOrCreateServerGameWithFallback());
            }
            else
            {
                Debug.Log("Usando dati locali...");
                LoadLocalGame();
            }
        }


        private IEnumerator LoadOrCreateServerGameWithFallback()
        {
            bool operationComplete = false;
            bool serverError = false;

            yield return StartCoroutine(ApiService.Instance.GetGameInformation(
                onSuccess: (gameInfo) =>
                {
                    if (gameInfo != null)
                    {
                        try
                        {
                            Debug.Log("Caricamento dati dal server...");
                            LoadGameFromServerData(gameInfo);
                            operationComplete = true;
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"Errore nel caricamento dei dati dal server: {e}");
                            serverError = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Nessun dato trovato sul server. Creazione nuovo gioco...");
                        StartCoroutine(CreateNewServerGameAsync((success) => {
                            if (!success)
                            {
                                serverError = true;
                            }
                            operationComplete = true;
                        }));
                    }
                },
                onError: (error) =>
                {
                    Debug.Log($"Errore nel caricamento dei dati: {error}");
                    serverError = true;
                    operationComplete = true;
                }
            ));

            while (!operationComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (serverError)
            {
                Debug.Log("Fallback ai dati locali dopo errore server");
                useLocalData = true;  // Usa i dati locali per il resto della sessione
                LoadLocalGame();
            }
        }

        private void LoadLocalGame()
        {
            if (IsNewGame())
            {
                Debug.Log("Creazione nuovo gioco locale...");
                InitializeNewGame();
            }
            else
            {
                Debug.Log("Caricamento dati locali esistenti...");
                LoadPlayerData();
            }
        }

        public void SaveGame()
        {
            if (!isDataLoaded) return;

            SaveLocalGame();  // Salva sempre localmente per sicurezza

            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false && !useLocalData)
            {
                StartCoroutine(SaveGameWithFallback());
            }
        }

        private IEnumerator SaveGameWithFallback()
        {
            bool syncComplete = false;
            bool syncError = false;

            yield return StartCoroutine(GameSyncManager.Instance.SyncGameData(
                () => syncComplete = true,
                (error) => {
                    Debug.Log($"Errore durante il salvataggio sul server: {error}");
                    syncError = true;
                    syncComplete = true;
                }
            ));

            while (!syncComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (syncError)
            {
                Debug.Log("Fallback al salvataggio locale dopo errore server");
                useLocalData = true;  // Usa i dati locali per il resto della sessione
            }
        }

        private IEnumerator LoadOrCreateServerGame()
        {
            bool operationComplete = false;

            yield return StartCoroutine(ApiService.Instance.GetGameInformation(
                onSuccess: (gameInfo) =>
                {
                    if (gameInfo != null)
                    {
                        Debug.Log("Loading game from server data...");
                        LoadGameFromServerData(gameInfo);
                        operationComplete = true;
                    }
                    else
                    {
                        Debug.Log("No server data found. Creating new game...");
                        StartCoroutine(CreateNewServerGameAsync((success) => {
                            operationComplete = true;
                        }));
                    }
                },
                onError: (error) =>
                {
                    Debug.Log($"Error loading game data: {error}");
                    if (error.Contains("404"))
                    {
                        StartCoroutine(CreateNewServerGameAsync((success) => {
                            operationComplete = true;
                        }));
                    }
                    else
                    {
                        // Fallback to local data
                        if (IsNewGame())
                        {
                            InitializeNewGame();
                        }
                        else
                        {
                            LoadPlayerData();
                        }
                        operationComplete = true;
                    }
                }
            ));

            // Wait for all operations to complete
            while (!operationComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator CreateNewServerGameAsync(Action<bool> onComplete)
        {
            // Prepare initial game data without creating objects
            var gameInfo = new GameInformation
            {
                level = 1,
                experiencePoints = 0,
                elixir = 300,
                gold = 300,
                buildings = new List<BuildingData>
                {
                    new BuildingData(
                        System.Guid.NewGuid().ToString(),
                        13, // indice del prefab iniziale
                        22,  // posizione x
                        20   // posizione y
                    )
                }
            };

            bool serverOperationComplete = false;
            bool serverOperationSuccess = false;
            GameInformation serverResponse = null;

            yield return StartCoroutine(ApiService.Instance.CreateGameInformation(
                gameInfo,
                onSuccess: (response) => {
                    serverOperationComplete = true;
                    serverOperationSuccess = true;
                    serverResponse = response;
                    Debug.Log("New game created on server successfully");
                },
                onError: (error) => {
                    serverOperationComplete = true;
                    serverOperationSuccess = false;
                    Debug.Log($"Error creating new game on server: {error}");
                }
            ));

            while (!serverOperationComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (serverOperationSuccess && serverResponse != null)
            {
                LoadGameFromServerData(serverResponse);
            }
            else
            {
                Debug.Log("Failed to create game on server, initializing locally");
                InitializeNewGame();
            }

            onComplete?.Invoke(serverOperationSuccess);
        }

        private void InitializeNewGame()
        {
            level = 1;
            experiencePoints = 0;
            PlayerPrefsController.Instance.Elixir = 300;
            PlayerPrefsController.Instance.Gold = 300;
            PlayerBuildings = new Building[45, 45];

            // Create initial building
            Vector3 position = Vector3.zero;
            Building building = Instantiate(UIController.Instance.Buildings[13], position, Quaternion.identity, buildingsContainer);
            building.Id = System.Guid.NewGuid().ToString();
            building.PlaceOnGrid(0, 0);
            PlayerBuildings[0, 0] = building;

            isDataLoaded = true;
            SaveGame();
        }

        private bool IsNewGame()
        {
            return PlayerPrefsController.Instance.Elixir == 0 &&
                   PlayerPrefsController.Instance.Gold == 0;
        }

        public void LoadPlayerData()
        {
            level = PlayerPrefsController.Instance.Level;
            experiencePoints = PlayerPrefsController.Instance.Exp;
            List<BuildingData> buildingsList = PlayerPrefsController.Instance.GetBuildings();

            PlayerBuildings = new Building[45, 45];

            if (buildingsList == null || buildingsList.Count == 0)
            {
                Debug.LogWarning("Nessun edificio da caricare. Inizializzazione di una griglia vuota.");
                return;
            }

            LoadBuildingsFromData(buildingsList);
            isDataLoaded = true;
        }

        private void LoadGameFromServerData(GameInformation gameInfo)
        {
            level = gameInfo.level;
            experiencePoints = gameInfo.experiencePoints;
            PlayerPrefsController.Instance.Elixir = gameInfo.elixir;
            PlayerPrefsController.Instance.Gold = gameInfo.gold;
            PlayerBuildings = new Building[45, 45];

            LoadBuildingsFromData(gameInfo.buildings);
            isDataLoaded = true;
        }

        private void LoadBuildingsFromData(IList<BuildingData> buildings)
        {
            if (buildings == null)
            {
                Debug.Log("Lista edifici null");
                return;
            }

            Debug.Log($"Numero di edifici da caricare: {buildings.Count}");

            foreach (BuildingData data in buildings)
            {
                if (data == null)
                {
                    Debug.Log("BuildingData null trovato nella lista.");
                    continue;
                }

                if (data.getPrefabIndex() < 0 || data.getPrefabIndex() >= UIController.Instance.Buildings.Length)
                {
                    Debug.Log($"Indice del prefab non valido: {data.getPrefabIndex()}");
                    continue;
                }

                Vector3 position = new Vector3(data.getX(), 0, data.getY());
                Building buildingPrefab = UIController.Instance.Buildings[data.getPrefabIndex()];

                if (buildingPrefab == null)
                {
                    Debug.Log($"Prefab dell'edificio non trovato per l'indice {data.getPrefabIndex()}");
                    continue;
                }

                Building building = Instantiate(buildingPrefab, position, Quaternion.identity, buildingsContainer);
                building.Id = data.GetUniqueId();

                if (building.PrefabIndex == 4) // Training base
                {
                    building.name = building.Id;
                    LoadTroopsForTrainingBase(data, building);
                }

                try
                {
                    building.ConfirmLoadBuildings();
                }
                catch (Exception e)
                {   
                    Debug.Log($"Errore durante la conferma dell'edificio: {e.Message}");
                    Debug.LogError($"StackTrace: {e.StackTrace}");
                    continue;
                }

                building.PlaceOnGrid(data.getX(), data.getY());

                // Aggiorna la griglia con l'edificio e i suoi placeholder
                for (int i = 0; i < building.Rows && (data.getX() + i) < 45; i++)
                {
                    for (int j = 0; j < building.Columns && (data.getY() + j) < 45; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            PlayerBuildings[data.getX() + i, data.getY() + j] = building;
                        }
                        else
                        {
                            var placeholder = building.gameObject.AddComponent<BuildingPlaceholder>();
                            placeholder.ParentBuilding = building;
                            PlayerBuildings[data.getX() + i, data.getY() + j] = placeholder;
                        }
                    }
                }
            }

            Debug.Log("Caricamento degli edifici completato.");
        }




        private void LoadTroopsForTrainingBase(BuildingData data, Building building)
        {
            List<TroopsData> troopsData = data.getTroopsData();
            if (troopsData == null || troopsData.Count == 0)
            {
                Debug.Log($"Nessuna truppa da caricare per l'edificio {data.GetUniqueId()}");
                return;
            }

            Debug.Log($"Caricamento truppe per l'edificio {data.GetUniqueId()}. Numero di truppe: {troopsData.Count}");

            for (int i = 0; i < troopsData.Count; i++)
            {
                TroopsData troopData = troopsData[i];

                Vector3 localPosition = new Vector3(troopData.getX(), troopData.getY(), troopData.getZ());

                Troops troopInstance = Instantiate(
                    troops[troopData.getType()],
                    localPosition,
                    Quaternion.identity,
                    building.transform
                );

                troopInstance.transform.localPosition = localPosition;

                Debug.Log($"Truppa caricata: Tipo={troopData.getType()}, Posizione locale={localPosition}");
            }
        }


        public void AddExperience(int amount)
        {
            experiencePoints += amount;
            CheckLevelUp();
            SaveGame();
        }

        private void CheckLevelUp()
        {
            while (experiencePoints >= ExperienceForNextLevel(level))
            {
                experiencePoints -= ExperienceForNextLevel(level);
                level++;
            }
            PlayerPrefsController.Instance.Level = level;
            PlayerPrefsController.Instance.Exp = experiencePoints;
        }

        private int ExperienceForNextLevel(int currentLevel)
        {
            const int baseExp = 100;
            const int multiplier = 2;
            const int offset = 10;
            return (int)(baseExp * Mathf.Log(multiplier * currentLevel + offset));
        }



        public void SaveLocalGame()
        {
            PlayerPrefsController.Instance.SaveAllData(
                level,
                experiencePoints,
                PlayerPrefsController.Instance.Elixir,
                PlayerPrefsController.Instance.Gold,
                PlayerPrefsController.Instance.GetBuildings()
            );
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }


    }
}