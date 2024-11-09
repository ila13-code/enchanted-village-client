using System.Collections;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class EnemyBuildingsController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;
        private bool isDestroyed = false;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

            if (currentHealth <= 0 && !isDestroyed)
            {
                isDestroyed = true;
                DestroyBuilding();
            }
        }

        private void DestroyBuilding()
        {
            Debug.Log($"{gameObject.name} has been destroyed!");
            //todo: aggiungere effetti visivi e sonori

            // Notifica eventuali observer prima della distruzione
            SendMessage("OnBuildingDestroyed", gameObject, SendMessageOptions.DontRequireReceiver);

            // Aggiungi un piccolo delay prima della distruzione effettiva
            StartCoroutine(DestroyWithDelay());
        }

        private IEnumerator DestroyWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return currentHealth > 0 && !isDestroyed;
        }
    }
}