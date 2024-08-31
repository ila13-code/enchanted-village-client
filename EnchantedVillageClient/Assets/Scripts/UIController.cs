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
        [SerializeField] private Building[] _buildings;
        [SerializeField] public GameObject _elements = null;
        [SerializeField] public BuildGrid _buildGrid = null;
        [SerializeField] public GameObject _dialogs = null;
        private bool _active = true;


        public static UIController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIController>();
                    if (instance == null)
                    {
                        instance = new UIController();
                        GameObject go = new GameObject("UIController");
                        instance = go.AddComponent<UIController>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
            _elements.SetActive(true);
        }

        private void Start()
        {
            PlayerPrefsController.Instance.OnLevelChanged += UpdateLevel;
            PlayerPrefsController.Instance.OnExpChanged += UpdateExp;
            PlayerPrefsController.Instance.OnElixirChanged += UpdateElixir;
            PlayerPrefsController.Instance.OnGoldChanged += UpdateGold;

            UpdateLevel(PlayerPrefsController.Instance.Level);
            UpdateExp(PlayerPrefsController.Instance.Exp);
            UpdateElixir(PlayerPrefsController.Instance.Elixir);
            UpdateGold(PlayerPrefsController.Instance.Gold);
        }

        private void OnDestroy()
        {
            PlayerPrefsController.Instance.OnLevelChanged -= UpdateLevel;
            PlayerPrefsController.Instance.OnExpChanged -= UpdateExp;
            PlayerPrefsController.Instance.OnElixirChanged -= UpdateElixir;
            PlayerPrefsController.Instance.OnGoldChanged -= UpdateGold;
        }


        private void UpdateLevel(int newLevel)
        {
            _level.text = newLevel.ToString();
        }

        private void UpdateExp(int newExp)
        {
            int currentLevel = PlayerPrefsController.Instance.Level;
            int expForNextLevel = PlayerPrefsController.Instance.ExperienceForNextLevel(currentLevel);
            _levelSlider.maxValue = expForNextLevel;
            _levelSlider.value = newExp;
        }

        private void UpdateElixir(int newElixir)
        {
            _elisirAmount.text = newElixir.ToString();
        }

        private void UpdateGold(int newGold)
        {
            _goldAmount.text = newGold.ToString();
        }


        public bool isActive
        {
            get { return _active; }
        }

        public Building[] Buildings
        {
            get { return _buildings; }
        }
    }
}
