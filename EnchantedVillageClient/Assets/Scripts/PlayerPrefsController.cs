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
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string LevelKey = "PlayerLevel";
        private const string ElixirKey = "PlayerElixir";
        private const string GoldKey = "PlayerGold";
        private const string BuildingsKey = "PlayerBuildings";

        public event Action<int> OnLevelChanged;
        public event Action<int> OnElixirChanged;
        public event Action<int> OnGoldChanged;

        [System.Serializable]
        public class Building
        {
            public string Type;
            public Vector2 Position;
        }

        public int Level
        {
            get { return PlayerPrefs.GetInt(LevelKey, 1); }
            set
            {
                PlayerPrefs.SetInt(LevelKey, value);
                PlayerPrefs.Save();
                OnLevelChanged?.Invoke(value);
            }
        }

        public int Elixir
        {
            get { return PlayerPrefs.GetInt(ElixirKey, 0); }
            set
            {
                PlayerPrefs.SetInt(ElixirKey, value);
                PlayerPrefs.Save();
                OnElixirChanged?.Invoke(value);
            }
        }

        public int Gold
        {
            get { return PlayerPrefs.GetInt(GoldKey, 0); }
            set
            {
                PlayerPrefs.SetInt(GoldKey, value);
                PlayerPrefs.Save();
                OnGoldChanged?.Invoke(value);
            }
        }

        public List<Building> GetBuildings()
        {
            string json = PlayerPrefs.GetString(BuildingsKey, "[]");
            return JsonConvert.DeserializeObject<List<Building>>(json);
        }

        public void SaveBuildings(List<Building> buildings)
        {
            string json = JsonConvert.SerializeObject(buildings);
            PlayerPrefs.SetString(BuildingsKey, json);
            PlayerPrefs.Save();
        }

        public void SaveAllData(int level, int elixir, int gold, List<Building> buildings)
        {
            Level = level;
            Elixir = elixir;
            Gold = gold;
            SaveBuildings(buildings);
        }

        public void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
