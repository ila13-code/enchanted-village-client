using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unical.Demacs.EnchantedVillage
{
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
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            SaveAllData(Level, Exp, Elixir, Gold, GetBuildings());
            DestroyInstance();
        }

        private const string LevelKey = "PlayerLevel";
        private const string ExpKey = "ExpKey";
        private const string ElixirKey = "PlayerElixir";
        private const string GoldKey = "PlayerGold";
        private const string BuildingsKey = "PlayerBuildings";

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

        public Building[,] GetBuildings()
        {
            string json = PlayerPrefs.GetString(BuildingsKey, "[]");
            return JsonConvert.DeserializeObject<Building[,]>(json);
        }

        public void SaveBuildings(Building[,] buildings)
        {
            string json = JsonConvert.SerializeObject(buildings);
            PlayerPrefs.SetString(BuildingsKey, json);
        }

        public void SaveAllData(int level, int exp, int elixir, int gold, Building[,] buildings)
        {
            Level = level;
            Exp = exp;
            Elixir = elixir;
            Gold = gold;
            SaveBuildings(buildings);
            PlayerPrefs.Save();
        }

        public void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public int ExperienceForNextLevel(int level)
        {
            const int a = 100;
            const int b = 2;
            const int c = 10;

            return (int)(a * Mathf.Log(b * level + c));
        }

        public void InitializeNewGame()
        {
            Level = 1;
            Exp = 0;
            Gold = 300;
            Elixir = 300;
            SaveBuildings(new Building[45,45]);
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
    }
}