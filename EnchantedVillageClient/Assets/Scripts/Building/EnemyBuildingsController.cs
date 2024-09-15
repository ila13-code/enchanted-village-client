using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unical.Demacs.EnchantedVillage
{
    public class EnemyBuildingsController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }
        }

        private void DestroyBuilding()
        {
            Debug.Log($"{gameObject.name} has been destroyed!");

            Destroy(gameObject);
        }

    }
}