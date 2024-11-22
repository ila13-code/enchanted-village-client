using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Unical.Demacs.EnchantedVillage
{
    public class NotificationService : MonoBehaviour
    {
        private static NotificationService instance;
        [SerializeField] private ErrorDialog errorDialogPrefab;  // Cambiato in prefab
        [SerializeField] private ErrorDialog winBattleDialogPrefab;  // Cambiato in prefab
        [SerializeField] private ErrorDialog loseBattleDialogPrefab;  // Cambiato in prefab
        [SerializeField] private TextMeshProUGUI messageText;

        private ErrorDialog currentErrorDialog;
        private ErrorDialog currentWinDialog;
        private ErrorDialog currentLoseDialog;

        public static NotificationService Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("NotificationService");
                    instance = go.AddComponent<NotificationService>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDialogs();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDialogs()
        {
            // Creiamo i dialog e li manteniamo inattivi
            if (errorDialogPrefab != null && currentErrorDialog == null)
            {
                currentErrorDialog = Instantiate(errorDialogPrefab, transform);
                currentErrorDialog.gameObject.SetActive(false);
            }

            if (winBattleDialogPrefab != null && currentWinDialog == null)
            {
                currentWinDialog = Instantiate(winBattleDialogPrefab, transform);
                currentWinDialog.gameObject.SetActive(false);
            }

            if (loseBattleDialogPrefab != null && currentLoseDialog == null)
            {
                currentLoseDialog = Instantiate(loseBattleDialogPrefab, transform);
                currentLoseDialog.gameObject.SetActive(false);
            }
        }

        public void ShowNotification(string message)
        {
            if (currentErrorDialog == null)
            {
                Debug.LogWarning("Error dialog not initialized. Reinitializing...");
                InitializeDialogs();
            }

            if (messageText != null)
            {
                messageText.text = message;
            }
            
            if (currentErrorDialog != null)
            {
                currentErrorDialog.gameObject.SetActive(true);
                currentErrorDialog.Show();
            }
            else
            {
                Debug.LogError("Error dialog is still null after initialization attempt!");
            }
        }

        public void ShowWinBattleNotification()
        {
            if (currentWinDialog == null)
            {
                Debug.LogWarning("Win dialog not initialized. Reinitializing...");
                InitializeDialogs();
            }

            if (currentWinDialog != null)
            {
                currentWinDialog.gameObject.SetActive(true);
                currentWinDialog.Show();
            }
        }

        public void ShowLoseBattleNotification()
        {
            if (currentLoseDialog == null)
            {
                Debug.LogWarning("Lose dialog not initialized. Reinitializing...");
                InitializeDialogs();
            }

            if (currentLoseDialog != null)
            {
                currentLoseDialog.gameObject.SetActive(true);
                currentLoseDialog.Show();
            }
        }

        public void HideAllDialogs()
        {
            if (currentErrorDialog != null) currentErrorDialog.gameObject.SetActive(false);
            if (currentWinDialog != null) currentWinDialog.gameObject.SetActive(false);
            if (currentLoseDialog != null) currentLoseDialog.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reinizializza i dialog quando cambia la scena
            InitializeDialogs();
        }
    }
}