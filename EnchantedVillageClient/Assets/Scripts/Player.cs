using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unical.Demacs.EnchantedVillage
{
    public class Player : MonoBehaviour
    {
        private int experiencePoints;
        private int experienceToNextLevel = 100;

        private void Start()
        {
            experiencePoints = 0;
        }

        public void AddExperience(int amount)
        {
            experiencePoints += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            PlayerPrefsController.Instance.Level=experiencePoints;
            
        }

        private void Update()
        {
            // Per esempio, aggiungi esperienza quando il giocatore preme il tasto spazio
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.AddExperience(20);
            }
        }
    }
}
