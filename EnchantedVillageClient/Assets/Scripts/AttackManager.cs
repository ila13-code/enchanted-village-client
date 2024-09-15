using UnityEngine;
using System.Collections.Generic;

namespace Unical.Demacs.EnchantedVillage
{
    public class AttackManager : MonoBehaviour
    {
        private static AttackManager instance;
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
                { "TownCenter", (30, 50, 100) },
                { "Cannon", (5, 0, 0) },
                { "GoldCollector", (12, 200, 0) },
                { "GoldStorage", (20, 300, 0) },
                { "ElixirCollector", (12, 0, 200) },
                { "ElixirStorage", (20, 0, 300) },
                { "Wall", (0, 0, 0) },
                { "Flag", (10, 0, 0) }
            };
        }

        public void ProcessAttack(string buildingName)
        {
            buildingName = buildingName.ToLower();

            foreach (var reward in buildingRewards)
            {
                if (buildingName.Contains(reward.Key))
                {
                    Exp += reward.Value.exp;
                    Elixir += reward.Value.elixir;
                    Gold += reward.Value.gold;

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
    }
}