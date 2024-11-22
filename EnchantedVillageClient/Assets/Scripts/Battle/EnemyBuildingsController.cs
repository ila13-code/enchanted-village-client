using UnityEngine;
using UnityEngine.UI;

namespace Unical.Demacs.EnchantedVillage
{
    public class EnemyBuildingsController : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 1000;
        [SerializeField] private Material destroyedMaterial;
        [SerializeField] private float damagedAlpha = 0.3f;
        private string UniqueId { get; set; }

        [Header("Health Bar UI")]
        [SerializeField] private GameObject healthBarPrefab;
        private float healthBarHeight = 2f;
        private GameObject healthBarInstance;
        private Image healthFillImage;
        private Transform cameraTransform;

        private int currentHealth;
        private bool isDestroyed = false;
        private bool isUnderAttack = false;
        private MeshRenderer meshRenderer;
        private Material originalMaterial;
        private Material destroyedMaterialInstance;
        private BuildGrid buildGrid;
        private BattleBuilding battleBuilding;

        private void Start()
        {
            UniqueId = battleBuilding.Id;

            currentHealth = maxHealth;
            meshRenderer = transform.Find("Mesh_1").GetComponent<MeshRenderer>();
            buildGrid = FindObjectOfType<BuildGrid>();
            battleBuilding = GetComponent<BattleBuilding>();

            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
                destroyedMaterialInstance = new Material(destroyedMaterial);
            }

            CreateHealthBar();
            if (healthBarInstance != null)
            {
                healthBarInstance.SetActive(false);
            }
        }

        private void CreateHealthBar()
        {
            if (healthBarPrefab != null && battleBuilding != null && buildGrid != null)
            {
                // Calcola la posizione sulla griglia
                Vector3 buildingPosition = buildGrid.GetCenterPosition(
                    battleBuilding.CurrentX,
                    battleBuilding.CurrentY,
                    battleBuilding.Rows,
                    battleBuilding.Columns
                );

                // Crea la barra della vita e posizionala
                healthBarInstance = Instantiate(healthBarPrefab);
                healthBarInstance.transform.SetParent(transform);

                // Posiziona la barra sopra l'edificio
                Vector3 healthBarPosition = new Vector3(
                    buildingPosition.x,
                    buildingPosition.y + healthBarHeight,
                    buildingPosition.z
                );
                healthBarInstance.transform.position = healthBarPosition;

                // Imposta una scala appropriata
                healthBarInstance.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

                // Configura il Canvas
                Canvas canvas = healthBarInstance.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = Camera.main;
                    canvas.sortingOrder = 1000;

                    CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.dynamicPixelsPerUnit = 100f;
                    }
                }

                // Trova il riferimento all'immagine del riempimento
                healthFillImage = healthBarInstance.transform.Find("Background/Filler").GetComponent<Image>();
                if (healthFillImage == null)
                {
                    Debug.LogError($"[{gameObject.name}] Fill Image non trovata!", this);
                }

                cameraTransform = Camera.main.transform;
                UpdateHealthBar();
            }
            else
            {
                if (healthBarPrefab == null) Debug.LogError($"[{gameObject.name}] HealthBarPrefab non assegnato!");
                if (battleBuilding == null) Debug.LogError($"[{gameObject.name}] BattleBuilding non trovato!");
                if (buildGrid == null) Debug.LogError($"[{gameObject.name}] BuildGrid non trovato!");
            }
        }

        private void LateUpdate()
        {
            if (healthBarInstance != null && !isDestroyed && cameraTransform != null)
            {
                healthBarInstance.transform.rotation = cameraTransform.rotation;
            }
        }

        private void UpdateHealthBar()
        {
            if (healthFillImage != null)
            {
                float healthPercentage = (float)currentHealth / maxHealth;
                healthFillImage.fillAmount = healthPercentage;

                if (healthPercentage <= 0.3f)
                    healthFillImage.color = Color.red;
                else if (healthPercentage <= 0.6f)
                    healthFillImage.color = Color.yellow;
                else
                    healthFillImage.color = Color.green;
            }
        }

        public void StartAttack()
        {
            if (!isDestroyed && healthBarInstance != null)
            {
                isUnderAttack = true;
                healthBarInstance.SetActive(true);
            }
        }

        public void EndAttack()
        {
            if (healthBarInstance != null)
            {
                isUnderAttack = false;
                healthBarInstance.SetActive(false);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            // Se è il primo colpo ricevuto, mostra la health bar
            if (!isUnderAttack)
            {
                StartAttack();
            }

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

            UpdateHealthBar();

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

                if (healthBarInstance != null)
                {
                    healthBarInstance.SetActive(false);
                }
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

            if (healthBarInstance != null)
            {
                Destroy(healthBarInstance);
            }
        }

        public bool IsAlive()
        {
            return currentHealth > 0 && !isDestroyed;
        }

        public string GetUniqueId()
        {
            return UniqueId;
        }
    }
}