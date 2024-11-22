
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
namespace Unical.Demacs.EnchantedVillage
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject lose;
        [SerializeField] private GameObject win;

        public void PlayGame()
        {
            SceneManager.LoadSceneAsync(1);
        }
        public void BattleFriend(TMP_InputField emailInput)
        {
            if (string.IsNullOrEmpty(emailInput.text))
            {
                NotificationService.Instance.ShowNotification("Please enter a friend's email address");
                return;
            }

            string friendEmail = emailInput.text.Trim();
            /*if (friendEmail == PlayerPrefs.GetString("userEmail"))
            {
                NotificationService.Instance.ShowNotification("You cannot battle against yourself!");
                return;
            }*/

            StartCoroutine(ApiService.Instance.ExistsByEmail(friendEmail,
                exists => {
                    if (exists)
                    {
                        ServicesManager.Instance.SceneTransitionService.ChangeSceneNoSync(3, () => {
                            Player.Instance.SaveLocalGame();
                            PlayerPrefs.SetString("battleFriendEmail", friendEmail);
                        });
                    }
                    else
                    {
                        NotificationService.Instance.ShowNotification($"No player found with email: {friendEmail}");
                    }
                },
                error => {
                    NotificationService.Instance.ShowNotification($"Error researching player");
                   
                }
            ));
        }

        public void BattleDemo()
        {
            ServicesManager.Instance.SceneTransitionService.ChangeScene(2, () => {
                Player.Instance.SaveLocalGame();
            });
        }

        public void Home()
        {
            Debug.Log("[Home] Starting battle submission");

            // Nascondi eventuali dialog attivi prima di cambiare scena
            NotificationService.Instance.HideAllDialogs();

            ApiService.Instance.HandleBattleSubmission(
                onSuccess: (percentage) => {
                    Debug.Log($"[Home] Battle submission successful. Destruction: {percentage}%");

                    // Attiva il GameObject appropriato
                    if (percentage >= 50)
                    {
                        win.SetActive(true);
                        if (win.GetComponentInChildren<TextMeshProUGUI>() != null)
                        {
                            win.GetComponentInChildren<TextMeshProUGUI>().text = $"{percentage}%";
                        }
                    }
                    else
                    {
                        lose.SetActive(true);
                        if (lose.GetComponentInChildren<TextMeshProUGUI>() != null)
                        {
                            lose.GetComponentInChildren<TextMeshProUGUI>().text = $"{percentage}%";
                        }
                    }
                },
                onError: (error) => {
                    Debug.LogError($"[Home] Battle submission error: {error}");
                    ServicesManager.Instance.SceneTransitionService.ChangeSceneNoSync(1, () => {
                        StartCoroutine(ShowErrorNextFrame(error));
                    });
                }
            );
        }

        private IEnumerator ShowErrorNextFrame(string error)
        {
            yield return new WaitForEndOfFrame();
            NotificationService.Instance.ShowNotification($"Error sending battle information: {error}");
        }

        public void GoHome()
        {
            ServicesManager.Instance.SceneTransitionService.ChangeSceneNoSync(1, () => {
                
            });
        }
    

    }
}