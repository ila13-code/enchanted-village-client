using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            PlayerPrefsController.Instance.ClearAllData();
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
            PlayerBuildings = PlayerPrefsController.Instance.GetBuildings();
            if(PlayerBuildings.Length == 0)
            {
                PlayerBuildings = new Building[45, 45];
            }
        }

    }
}