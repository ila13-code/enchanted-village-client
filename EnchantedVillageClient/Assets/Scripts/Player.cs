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
            level = PlayerPrefsController.Instance.Level;
            experiencePoints = PlayerPrefsController.Instance.Exp;
            PlayerPrefsController.Instance.Elixir = 300;
            PlayerPrefsController.Instance.Gold = 300;
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
            // Constants for the log growth model
            const int a = 100;
            const int b = 2;
            const int c = 10;

            return (int)(a * Mathf.Log(b * currentLevel + c));
        }

        private void Update()
        {
    
            
        }

        private void OnApplicationQuit()
        {
            PlayerPrefsController.DestroyInstance();
        }
    }
}
