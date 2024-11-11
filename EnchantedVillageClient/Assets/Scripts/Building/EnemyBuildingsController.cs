using System.Collections;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class EnemyBuildingsController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 1000;
        [SerializeField] private Material destroyedMaterial; 
        [SerializeField] private float damagedAlpha = 0.3f; 

        private int currentHealth;
        private bool isDestroyed = false;
        private MeshRenderer meshRenderer;
        private Material originalMaterial;
        private Material destroyedMaterialInstance;

        private void Start()
        {
            currentHealth = maxHealth;
            meshRenderer = transform.Find("Mesh_1").GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
                destroyedMaterialInstance = new Material(destroyedMaterial);
            }
            else
            {
                Debug.LogError("MeshRenderer non trovato nel Mesh_1 child", gameObject);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

            if (meshRenderer == null) return;

            if (currentHealth <= maxHealth * 0.5f && currentHealth > 0)
            {
                float healthPercentage = (float)currentHealth / maxHealth;
                float currentAlpha = Mathf.Lerp(1f, damagedAlpha, healthPercentage * 2);

                if (meshRenderer.material != destroyedMaterialInstance)
                {
                    meshRenderer.material = destroyedMaterialInstance;
                }

                SetMaterialAlpha(currentAlpha);
            }
            else if (currentHealth > maxHealth * 0.5f)
            {
                meshRenderer.material = originalMaterial;
            }

            if (currentHealth <= 0 && !isDestroyed)
            {
                isDestroyed = true;
                if (meshRenderer.material != destroyedMaterialInstance)
                {
                    meshRenderer.material = destroyedMaterialInstance;
                }
                SetMaterialAlpha(1f);
            }
        }

        private void SetMaterialAlpha(float alpha)
        {
            if (destroyedMaterialInstance == null) return;
            if (destroyedMaterialInstance.HasProperty("_Color"))
            {
                Color color = destroyedMaterialInstance.GetColor("_Color");
                color.a = alpha;
                destroyedMaterialInstance.SetColor("_Color", color);
            }
            else if (destroyedMaterialInstance.HasProperty("_BaseColor"))
            {
                Color color = destroyedMaterialInstance.GetColor("_BaseColor");
                color.a = alpha;
                destroyedMaterialInstance.SetColor("_BaseColor", color);
            }
            else
            {
                destroyedMaterialInstance.SetFloat("_Alpha", alpha);
            }
        }

 


        private void OnDestroy()
        {
            if (destroyedMaterialInstance != null)
            {
                Destroy(destroyedMaterialInstance);
            }
        }

        public bool IsAlive()
        {
            return currentHealth > 0 && !isDestroyed;
        }
    }
}