using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unical.Demacs.EnchantedVillage
{
    // Classe che gestisce il salvataggio e il caricamento dei dati del giocatore
    public class PlayerPrefsController : MonoBehaviour
    {
        private static PlayerPrefsController _instance;
        public static PlayerPrefsController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerPrefsController");
                    _instance = go.AddComponent<PlayerPrefsController>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string LevelKey = "PlayerLevel";
        private const string ExpKey = "ExpKey";
        private const string ElixirKey = "PlayerElixir";
        private const string GoldKey = "PlayerGold";
        private const string BuildingsKey = "PlayerBuildings";
        private const string enemyEmail = "enemyEmail";

        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public event Action<int> OnElixirChanged;
        public event Action<int> OnGoldChanged;

        public int Level
        {
            get { return PlayerPrefs.GetInt(LevelKey, 1); }
            set
            {
                PlayerPrefs.SetInt(LevelKey, value);
                OnLevelChanged?.Invoke(value);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public int Exp
        {
            get { return PlayerPrefs.GetInt(ExpKey, 0); }
            set
            {
                int currentLevel = Level;
                while (value >= ExperienceForNextLevel(currentLevel))
                {
                    value -= ExperienceForNextLevel(currentLevel);
                    currentLevel++;
                }

                PlayerPrefs.SetInt(ExpKey, value);
                Level = currentLevel;
                OnExpChanged?.Invoke(value);
            }
        }

        public int Elixir
        {
            get { return PlayerPrefs.GetInt(ElixirKey, 0); }
            set
            {
                PlayerPrefs.SetInt(ElixirKey, value);
                OnElixirChanged?.Invoke(value);
            }
        }

        public int Gold
        {
            get { return PlayerPrefs.GetInt(GoldKey, 0); }
            set
            {
                PlayerPrefs.SetInt(GoldKey, value);
                OnGoldChanged?.Invoke(value);
            }
        }


        public string EnemyEmail
        {
            get { return PlayerPrefs.GetString(enemyEmail, ""); }
            set
            {
                PlayerPrefs.SetString(enemyEmail, value);
            }
        }

        // Recupera la lista di edifici salvata
        public List<BuildingData> GetBuildings()
        {
            string json = PlayerPrefs.GetString(BuildingsKey, "[]");
            var buildings = JsonConvert.DeserializeObject<List<BuildingData>>(json);

            if (buildings == null || buildings.Count == 0)
            {
                Debug.LogWarning("Nessun edificio recuperato dai PlayerPrefs.");
                return new List<BuildingData>();
            }

            return buildings;
        }

        // Salva la lista di edifici
        public void SaveBuildings(List<BuildingData> buildings)
        {
            string json = JsonConvert.SerializeObject(buildings);
            PlayerPrefs.SetString(BuildingsKey, json);
        }

        // Salva tutti i dati del giocatore
        public void SaveAllData(int level, int exp, int elixir, int gold, List<BuildingData> buildings)
        {
            Level = level;
            Exp = exp;
            Elixir = elixir;
            Gold = gold;

            if (buildings != null && buildings.Count > 0)
            {
                SaveBuildings(buildings);
            }
            else
            {
                Debug.LogWarning("Tentativo di salvare una lista di edifici vuota o nulla.");
            }

            PlayerPrefs.Save();
        }

        // Cancella tutti i dati salvati
        public void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        // Calcola l'esperienza necessaria per raggiungere il livello successivo
        public int ExperienceForNextLevel(int level)
        {
            const int a = 100;
            const int b = 2;
            const int c = 10;

            return (int)(a * Mathf.Log(b * level + c));
        }

        // Inizializza i valori di default per un nuovo gioco
        public void InitializeNewGame()
        {
            Level = 1;
            Exp = 0;
            Gold = 300;
            Elixir = 300;
            SaveBuildings(new List<BuildingData>());
            PlayerPrefs.Save();
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        // Distrugge l'istanza quando l'applicazione viene chiusa
        private void OnApplicationQuit()
        {
            DestroyInstance();
        }
    }
}