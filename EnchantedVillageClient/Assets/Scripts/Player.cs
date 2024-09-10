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
        private Transform buildingsContainer;

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
            }
            else
            {
                Debug.LogError("Map non trovato nella scena.");
            }
        }

        private void Start()
        {

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
            return PlayerPrefsController.Instance.Elixir == 0 && PlayerPrefsController.Instance.Gold == 0 ;
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

                Debug.Log($"Caricamento edificio: PrefabIndex={data.getPrefabIndex()}, X={data.getX()}, Y={data.getY()}");

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
                

                if (building == null)
                {
                    Debug.LogError($"Impossibile istanziare l'edificio con indice {data.getPrefabIndex()}");
                    continue;
                }

                try
                {
                    building.Confirm();
                }
                catch (NullReferenceException e)
                {
                    Debug.LogError($"NullReferenceException durante la conferma dell'edificio: {e.Message}");
                    Debug.LogError($"StackTrace: {e.StackTrace}");
                    continue;
                }

                building.PlaceOnGrid(data.getX(), data.getY());

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
                            var placeholder = gameObject.AddComponent<BuildingPlaceholder>();
                            placeholder.ParentBuilding = building;
                            PlayerBuildings[data.getX() + i, data.getY() + j] = placeholder;
                        }
                    }
                }

                Debug.Log($"Edificio caricato con successo: PrefabIndex={data.getPrefabIndex()}, X={data.getX()}, Y={data.getY()}");
            }

            Debug.Log("Caricamento degli edifici completato.");
        }
        public void OnApplicationQuit()
        {
            PlayerPrefsController.Instance.SaveAllData(level, experiencePoints, PlayerPrefsController.Instance.Elixir, PlayerPrefsController.Instance.Gold, PlayerPrefsController.Instance.GetBuildings());
        }
    }
}