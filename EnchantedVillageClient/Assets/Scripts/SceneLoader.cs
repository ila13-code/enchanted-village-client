namespace Unical.Demacs.EnchantedVillage
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    {
        public void PlayGame()
        {
            SceneManager.LoadSceneAsync(1);
        }

        public void Battle()
        {
            SceneManager.LoadSceneAsync(2);
        }
        public void Home()
        {
            SceneManager.LoadSceneAsync(1);
        }
    }
}
