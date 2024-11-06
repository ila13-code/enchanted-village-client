namespace Unical.Demacs.EnchantedVillage
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    {
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
                        ServicesManager.Instance.SceneTransitionService.ChangeScene(3, () => {
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
            //todo battle info
            ServicesManager.Instance.SceneTransitionService.ChangeScene(1, () => {
                PlayerPrefsController.Instance.Elixir += AttackManager.Instance.Elixir;
                PlayerPrefsController.Instance.Gold += AttackManager.Instance.Gold;
                PlayerPrefsController.Instance.Exp += AttackManager.Instance.Exp;
                //Player.Instance.SaveLocalGame();
            });
        }
    }
}