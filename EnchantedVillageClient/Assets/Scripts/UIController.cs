using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

namespace Unical.Demacs.EnchantedVillage
{
    public class UIController : MonoBehaviour
    {
        private static UIController instance = null;

        [SerializeField] private TextMeshProUGUI _elisirAmount;
        [SerializeField] private TextMeshProUGUI _goldAmount;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Slider _levelSlider;
        [SerializeField] private Slider _elisirSlider;
        [SerializeField] private Slider _goldSlider;
        [SerializeField] private Button _shop;
        [SerializeField] private Button _battle;

        public static UIController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIController>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("UIController");
                        instance = go.AddComponent<UIController>();
                    }
                }
                return instance;
            }
        }

        private void Start()
        {
            _shop.onClick.AddListener(OnClickShop);

            PlayerPrefsController.Instance.OnLevelChanged += UpdateLevel;
            PlayerPrefsController.Instance.OnElixirChanged += UpdateElixir;
            PlayerPrefsController.Instance.OnGoldChanged += UpdateGold;

            UpdateLevel(PlayerPrefsController.Instance.Level);
            UpdateElixir(PlayerPrefsController.Instance.Elixir);
            UpdateGold(PlayerPrefsController.Instance.Gold);
        }

        private void OnDestroy()
        {
            PlayerPrefsController.Instance.OnLevelChanged -= UpdateLevel;
            PlayerPrefsController.Instance.OnElixirChanged -= UpdateElixir;
            PlayerPrefsController.Instance.OnGoldChanged -= UpdateGold;
        }

        private void OnClickShop() { }

        private void UpdateLevel(int newLevel)
        {
            double experience = newLevel / 100;
            int level =Convert.ToInt32(experience);
            _level.text = level.ToString();
            if(experience % 1 != 0)
            {
                double pointsToNextLevel = Mathf.Abs((float) (level - experience));
                _levelSlider.value = Convert.ToInt32(pointsToNextLevel * 100);
                Debug.Log(level.ToString());
                Debug.Log(pointsToNextLevel.ToString());
                Debug.Log(experience.ToString());
            }

        }

        private void UpdateElixir(int newElixir)
        {
            _elisirAmount.text = newElixir.ToString();
        }

        private void UpdateGold(int newGold)
        {
            _goldAmount.text = newGold.ToString();
        }
    }
}
