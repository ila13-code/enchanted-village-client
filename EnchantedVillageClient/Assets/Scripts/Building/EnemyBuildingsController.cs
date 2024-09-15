using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unical.Demacs.EnchantedVillage
{
    //classe che gestisce i danni subiti dagli edifici nemici
    //se l'edificio raggiunge 0 di vita, viene distrutto
    public class EnemyBuildingsController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        //funzione che sottrae i danni subiti dall'edificio
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }
        }

        //funzione che distrugge l'edificio
        private void DestroyBuilding()
        {
            Debug.Log($"{gameObject.name} has been destroyed!");

            //todo: aggiungere effetti visivi e sonori
            Destroy(gameObject);
        }

    }
}