using UnityEngine;
using System.Collections.Generic;
using static BattleInformation;
using System.Linq;

namespace Unical.Demacs.EnchantedVillage
{
    public class AttackManager : MonoBehaviour
    {
        private static AttackManager instance;
        private List<BattleDestroyed> destroyedBuildings = new List<BattleDestroyed>();
        public static AttackManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AttackManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(AttackManager).Name;
                        instance = obj.AddComponent<AttackManager>();
                    }
                }
                return instance;
            }
        }

        public int Exp { get; private set; }
        public int Elixir { get; private set; }
        public int Gold { get; private set; }

        public int PercentageDestroyed { get; private set; }

        private Dictionary<string, (int exp, int elixir, int gold)> buildingRewards;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
                InitializeBuildingRewards();
            }
        }

        private void InitializeBuildingRewards()
        {
            buildingRewards = new Dictionary<string, (int, int, int)>
            {
                { "towncenter", (30, 50, 100) },
                { "cannon", (5, 0, 0) },
                { "goldcollector", (12, 200, 0) },
                { "goldstorage", (20, 300, 0) },
                { "elixircollector", (12, 0, 200) },
                { "elixirstorage", (20, 0, 300) },
                { "wall", (0, 0, 0) },
                { "flag", (10, 0, 0) }
            };
        }

        public void ProcessAttack(string buildingId,string buildingName)
        {
            buildingName = buildingName.ToLower();

            if (!string.IsNullOrEmpty(buildingId) && !destroyedBuildings.Any(b => b.uniqueId == buildingId))
            {
                destroyedBuildings.Add(new BattleDestroyed(buildingId));
                SaveDestroyedBuildingsToPrefs();
            }


            foreach (var reward in buildingRewards)
            {
                if (buildingName.Contains(reward.Key))
                {
                    Exp += reward.Value.exp;
                    Elixir += reward.Value.elixir;
                    Gold += reward.Value.gold;
                    PercentageDestroyed = 70;

                    PlayerPrefs.SetInt("ExpReward", Exp);
                    PlayerPrefs.SetInt("ElixirStolen", Elixir);
                    PlayerPrefs.SetInt("GoldStolen", Gold);
                    PlayerPrefs.SetInt("PercentageDestroyed", PercentageDestroyed);

                    Debug.Log($"Attack completed on {buildingName}. Resources increased - EXP: {Exp}, Elixir: {Elixir}, Gold: {Gold}");
                    return;
                }
            }

            Exp += 25;
            Elixir += 25;
            Gold += 25;
            Debug.Log($"Attack completed on unknown building type: {buildingName}. Default resources increase applied.");
        }

        public int GetExp() => Exp;
        public int GetElixir() => Elixir;
        public int GetGold() => Gold;


        private void SaveDestroyedBuildingsToPrefs()
        {
            string destroyedBuildingsJson = JsonUtility.ToJson(new { buildings = destroyedBuildings });
            PlayerPrefs.SetString("DestroyedBuildings", destroyedBuildingsJson);
            Debug.Log($"Saved destroyed buildings: {destroyedBuildingsJson}");
        }

        public List<BattleDestroyed> GetDestroyedBuildings()
        {
            return new List<BattleDestroyed>(destroyedBuildings);
        }

        private void LoadDestroyedBuildingsFromPrefs()
        {
            string savedBuildingsJson = PlayerPrefs.GetString("DestroyedBuildings", "");
            if (!string.IsNullOrEmpty(savedBuildingsJson))
            {
                var wrapper = JsonUtility.FromJson<DestroyedBuildingsWrapper>(savedBuildingsJson);
                destroyedBuildings = wrapper.buildings;
            }
        }

        // Classe wrapper per la serializzazione
        [System.Serializable]
        private class DestroyedBuildingsWrapper
        {
            public List<BattleDestroyed> buildings;
        }
    }
}
