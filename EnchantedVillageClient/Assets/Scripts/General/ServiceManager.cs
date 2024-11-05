using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class ServicesManager : MonoBehaviour
    {
        public static ServicesManager Instance { get; private set; }
        public KeycloakService KeycloakService { get; private set; }
        public ApiService ApiService { get; private set; }
        public GameSyncManager GameSyncManager { get; private set; }
        public SceneTransitionService SceneTransitionService { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServices();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeServices()
        {
            // KeycloakService
            KeycloakService = FindObjectOfType<KeycloakService>();
            if (KeycloakService == null)
            {
                GameObject keycloakObject = new GameObject("KeycloakService");
                keycloakObject.transform.SetParent(transform);
                KeycloakService = keycloakObject.AddComponent<KeycloakService>();
            }
            else
            {
                KeycloakService.transform.SetParent(transform);
            }
            DontDestroyOnLoad(KeycloakService.gameObject);

            // ApiService
            GameObject apiObject = new GameObject("ApiService");
            apiObject.transform.SetParent(transform);
            ApiService = apiObject.AddComponent<ApiService>();

            // GameSyncManager
            GameObject syncObject = new GameObject("GameSyncManager");
            syncObject.transform.SetParent(transform);
            GameSyncManager = syncObject.AddComponent<GameSyncManager>();

            // SceneTransitionService
            GameObject sceneTransitionObject = new GameObject("SceneTransitionService");
            sceneTransitionObject.transform.SetParent(transform);
            SceneTransitionService = sceneTransitionObject.AddComponent<SceneTransitionService>();


            Debug.Log("All services initialized");
        }
    }
}