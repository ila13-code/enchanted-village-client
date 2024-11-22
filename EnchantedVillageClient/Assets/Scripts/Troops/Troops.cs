using System.Collections;
using System.Collections.Generic;
using Unical.Demacs.EnchantedVillage;
using UnityEngine;
using UnityEngine.UI;

namespace Unical.Demacs.EnchantedVillage
{
    public class Troops : MonoBehaviour
    {
        [Header("Unit Settings")]
        private int _rows = 1;
        private int _columns = 1;
        private int _currentX;
        private int _currentY;
        [SerializeField] private int maxHealth = 100;
        private int _currentHealth;

        [Header("Animation")]
        [SerializeField] private float deathDuration = 1f;
        private Animator animator;
        private const string STATE_PARAM = "State";
        private const int DEATH_STATE = 7;

        [Header("Health Bar")]
        [SerializeField] private GameObject healthBarPrefab;
        private float healthBarHeight = 1.5f;
        private GameObject healthBarInstance;
        private Image healthFillImage;
        private Transform cameraTransform;

        public int CurrentX => _currentX;
        public int CurrentY => _currentY;
        public int CurrentHealth => _currentHealth;

        private BuildGrid _buildGrid;
        private bool isDestroyed = false;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _buildGrid = FindObjectOfType<BuildGrid>();
            cameraTransform = Camera.main.transform;
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            CreateHealthBar();
        }

        private void CreateHealthBar()
        {
            if (healthBarPrefab != null && _buildGrid != null)
            {
                // Crea la health bar
                healthBarInstance = Instantiate(healthBarPrefab);
                healthBarInstance.transform.SetParent(transform);

                // Posiziona la barra sopra l'unità
                healthBarInstance.transform.localPosition = new Vector3(0, healthBarHeight, 0);

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

                // Trova e configura l'immagine del riempimento
                healthFillImage = healthBarInstance.transform.Find("Background/Filler").GetComponent<Image>();
                if (healthFillImage != null)
                {
                    // Imposta il colore blu per la barra della vita
                    healthFillImage.color = new Color(0.3f, 0.5f, 1f); // Blu chiaro
                }

                UpdateHealthBar();
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
                float healthPercentage = (float)_currentHealth / maxHealth;
                healthFillImage.fillAmount = healthPercentage;

                // Sfumature di blu in base alla vita
                if (healthPercentage <= 0.3f)
                    healthFillImage.color = new Color(0.2f, 0.3f, 0.8f); // Blu scuro
                else if (healthPercentage <= 0.6f)
                    healthFillImage.color = new Color(0.3f, 0.5f, 1f);   // Blu medio
                else
                    healthFillImage.color = new Color(0.4f, 0.7f, 1f);   // Blu chiaro
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            _currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage. Current health: {_currentHealth}");

            UpdateHealthBar();

            if (_currentHealth <= 0 && !isDestroyed)
            {
                isDestroyed = true;
                Die();
            }
        }

        private void Die()
        {
            if (healthBarInstance != null)
            {
                healthBarInstance.SetActive(false);
                Destroy(healthBarInstance);
            }

            // Disabilita il controller nemico e il collider
            var enemyController = GetComponent<EnemyTroopController>();
            if (enemyController != null)
            {
                enemyController.enabled = false;
            }

            // Disabilita il collider per evitare che venga rilevato come target
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Disabilita questo script
            this.enabled = false;

            // Comunica a tutti gli arcieri che questo target è morto
            var allArchers = FindObjectsOfType<ArcherController>();
            foreach (var archer in allArchers)
            {
                if (archer.CurrentAttackTarget == this.gameObject)
                {
                    archer.RemoveTarget(this.gameObject);
                }
            }

            // Attiva l'animazione di morte
            if (animator != null)
            {
                animator.SetInteger(STATE_PARAM, DEATH_STATE);
                StartCoroutine(DeathSequence());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator DeathSequence()
        {
            // Aspetta che finisca l'animazione di morte
            yield return new WaitForSeconds(deathDuration);
            if (animator != null)
            {
                animator.speed = 0;
            }
        }

        public void PlaceOnGrid(int x, int y, int numberOfTroops)
        {
            _currentX = x;
            _currentY = y;
            if (numberOfTroops > 3)
            {
                switch (numberOfTroops)
                {
                    case 4:
                        numberOfTroops = 1;
                        break;
                    case 5:
                        numberOfTroops = 2;
                        break;
                }
                y += numberOfTroops;
            }
            Vector3 position = _buildGrid.GetCenterPosition1(x + numberOfTroops, y, _rows, _columns);
            transform.position = position;
            Debug.Log("Troops placed at " + position.x + ", " + position.y);
        }

        private void OnDestroy()
        {
            if (healthBarInstance != null)
            {
                Destroy(healthBarInstance);
            }
        }
    }
}