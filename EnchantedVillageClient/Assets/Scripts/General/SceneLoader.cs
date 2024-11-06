namespace Unical.Demacs.EnchantedVillage
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    {
        public void PlayGame()
        {
            SceneManager.LoadSceneAsync(1);
        }

        public void BattleFriend(string email)
        {
            ServicesManager.Instance.SceneTransitionService.ChangeScene(3, () => {
                Player.Instance.SaveLocalGame();
                PlayerPrefsController.Instance.EnemyEmail = email;
            });
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