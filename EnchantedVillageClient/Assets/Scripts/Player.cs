using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Unical.Demacs.EnchantedVillage.Building;

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

        public static Player Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Player>();
                    if (instance == null)
                    {
                        instance = new Player();
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
            GameObject map = GameObject.Find("Map");
            if (map != null)
            {
                buildingsContainer = map.transform.Find("Buildings").transform;
                troopsContainer = map.transform.Find("Troops").transform; // Initialize troops container
            }
            else
            {
                Debug.LogError("Map non trovato nella scena.");
            }
        }

        private void Start()
        {
            //PlayerPrefsController.Instance.ClearAllData();
            if (this.IsNewGame())
            {
                NewGame();
            }
            else
            {
                PlayerPrefsController.Instance.Gold = 38904892;
                LoadPlayerData();
            }
        }

        private void NewGame()
        {
            level = 1;
            experiencePoints = 0;
            PlayerPrefsController.Instance.Elixir = 300;
            PlayerPrefsController.Instance.Gold = 300;
            PlayerBuildings = new Building[45, 45];
            Vector3 position = Vector3.zero;
            Building building = Instantiate(UIController.Instance.Buildings[13], position, Quaternion.identity, buildingsContainer);
        }

        public void AddExperience(int amount)
        {
            experiencePoints += amount;
            CheckLevelUp();
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
            const int a = 100;
            const int b = 2;
            const int c = 10;
            return (int)(a * Mathf.Log(b * currentLevel + c));
        }

        private bool IsNewGame()
        {
            return PlayerPrefsController.Instance.Elixir == 0 && PlayerPrefsController.Instance.Gold == 0;
        }

        private void LoadPlayerData()
        {
            level = PlayerPrefsController.Instance.Level;
            experiencePoints = PlayerPrefsController.Instance.Exp;
            List<BuildingData> list = PlayerPrefsController.Instance.GetBuildings();

            PlayerBuildings = new Building[45, 45];

            if (list == null || list.Count == 0)
            {
                Debug.LogWarning("Nessun edificio da caricare. Inizializzazione di una griglia vuota.");
                return;
            }

            Debug.Log($"Numero di edifici da caricare: {list.Count}");

            foreach (BuildingData data in list)
            {
                if (data == null)
                {
                    Debug.LogError("BuildingData null trovato nella lista.");
                    continue;
                }

                Debug.Log($"Caricamento edificio: {data.GetUniqueId()} PrefabIndex = {data.getPrefabIndex()}, X={data.getX()}, Y={data.getY()}");

                if (data.getPrefabIndex() < 0 || data.getPrefabIndex() >= UIController.Instance.Buildings.Length)
                {
                    Debug.LogError($"Indice del prefab non valido: {data.getPrefabIndex()}");
                    continue;
                }

                Vector3 position = new Vector3(data.getX(), 0, data.getY());
                Building buildingPrefab = UIController.Instance.Buildings[data.getPrefabIndex()];

                if (buildingPrefab == null)
                {
                    Debug.LogError($"Prefab dell'edificio non trovato per l'indice {data.getPrefabIndex()}");
                    continue;
                }

                Building building = Instantiate(buildingPrefab, position, Quaternion.identity, buildingsContainer);
                building.Id = data.GetUniqueId();
                if(building.PrefabIndex == 4) // Training base
                {
                    building.name = building.Id;
                }

                if (building == null)
                {
                    Debug.LogError($"Impossibile istanziare l'edificio con indice {data.getPrefabIndex()}");
                    continue;
                }

                try
                {
                    building.ConfirmLoadBuildings();
                }
                catch (NullReferenceException e)
                {
                    Debug.LogError($"NullReferenceException durante la conferma dell'edificio: {e.Message}");
                    Debug.LogError($"StackTrace: {e.StackTrace}");
                    continue;
                }

                building.PlaceOnGrid(data.getX(), data.getY());

                // Caricamento delle truppe per il campo di addestramento
                if (data.getPrefabIndex() == 4) // Indice del campo di addestramento
                {
                    LoadTroopsForTrainingBase(data, building);
                }

                // Controllo dei limiti della griglia
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

                Debug.Log($"Edificio caricato con successo: PrefabIndex={data.getPrefabIndex()}, X={data.getX()}, Y={data.getY()}");
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
                Vector3 spawnPosition = new Vector3(data.getX(), 1f, data.getY());
                Transform troopsContainer = building.transform.Find("TroopsContainer");
                if (troopsContainer == null)
                {
                    GameObject container = new GameObject("TroopsContainer");
                    container.transform.SetParent(building.transform);
                    troopsContainer = container.transform;
                }
                Troops troopInstance = Instantiate(troops[troopData.getType()], spawnPosition, Quaternion.identity, troopsContainer);
                troopInstance.PlaceOnGrid(data.getX(), data.getY(), i + 1);
                Debug.Log($"Truppa caricata: Tipo={troopData.getType()}, Posizione={i + 1}");
            }
            
        }

        public void OnApplicationQuit()
        {
            PlayerPrefsController.Instance.SaveAllData(level, experiencePoints, PlayerPrefsController.Instance.Elixir, PlayerPrefsController.Instance.Gold, PlayerPrefsController.Instance.GetBuildings());
        }
    }
}