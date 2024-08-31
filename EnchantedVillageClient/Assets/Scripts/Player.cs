using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class Player : MonoBehaviour
    {
        private int level;
        private int experiencePoints;

        private void Start()
        {
            if(this.IsNewGame())
            {
                level = 1;
                experiencePoints = 0;
                PlayerPrefsController.Instance.Elixir = 300;
                PlayerPrefsController.Instance.Gold = 300;
            }
            else
                LoadPlayerData();
         
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
            return PlayerPrefsController.Instance.Elixir == 0 && PlayerPrefsController.Instance.Gold == 0 && PlayerPrefsController.Instance.GetBuildings().Count == 0;
        }

        private void LoadPlayerData()
        {
            level = PlayerPrefsController.Instance.Level;
            experiencePoints = PlayerPrefsController.Instance.Exp;
            
        }
    }
}