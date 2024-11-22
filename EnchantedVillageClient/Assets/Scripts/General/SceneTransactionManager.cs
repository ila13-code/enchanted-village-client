using Unical.Demacs.EnchantedVillage;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;


namespace Unical.Demacs.EnchantedVillage
{
    public class SceneTransitionService : MonoBehaviour
    {
        public void ChangeScene(int sceneIndex, System.Action preTransitionAction = null)
        {
            StartCoroutine(ChangeSceneRoutine(sceneIndex, preTransitionAction));
        }

        public void ChangeSceneNoSync(int sceneIndex, System.Action preTransitionAction = null)
        {
            StartCoroutine(ChangeSceneRoutineNoSync(sceneIndex, preTransitionAction));
        }

        private IEnumerator ChangeSceneRoutine(int sceneIndex, System.Action preTransitionAction)
        {
            // Esegui azioni pre-transizione
            preTransitionAction?.Invoke();

            // Se siamo online, aspetta la sincronizzazione
            if (ServicesManager.Instance?.KeycloakService?.IsAuthenticated() ?? false)
            {
                bool syncComplete = false;
                yield return StartCoroutine(GameSyncManager.Instance.SyncGameData(
                    () => syncComplete = true,
                    (error) =>
                    {
                        Debug.LogError($"Errore sincronizzazione: {error}");
                        syncComplete = true;
                    }
                ));

                while (!syncComplete)
                {
                    yield return null;
                }
            }

            // Carica la nuova scena
            SceneManager.LoadSceneAsync(sceneIndex);
        }


        private IEnumerator ChangeSceneRoutineNoSync(int sceneIndex, System.Action preTransitionAction)
        {
            // Esegui azioni pre-transizione
            preTransitionAction?.Invoke();

            // Carica la nuova scena
            SceneManager.LoadSceneAsync(sceneIndex);

            yield return null;
        }
    }
}