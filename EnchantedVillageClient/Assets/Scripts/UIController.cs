namespace Unical.Demacs.EnchantedVillage
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using System;

    /// @brief Controlla l'interfaccia utente del gioco.
    /// @details Implementa il pattern Singleton per garantire un'unica istanza.
    public class UIController : MonoBehaviour
    {
        private static UIController instance = null;

        /// @brief Testo che mostra la quantità di elisir.
        [SerializeField] private TextMeshProUGUI _elisirAmount;

        /// @brief Testo che mostra la quantità di oro.
        [SerializeField] private TextMeshProUGUI _goldAmount;

        /// @brief Testo che mostra il livello corrente.
        [SerializeField] private TextMeshProUGUI _level;

        /// @brief Pulsante per accedere allo shop.
        [SerializeField] private Button _shop;

        /// @brief Pulsante per iniziare una battaglia.
        [SerializeField] private Button _battle;

        /// @brief Ottiene l'istanza singleton di UIController.
        /// @return L'istanza di UIController.
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
        /// @brief Inizializza il controller UI.
        private void Start()
        {
            _shop.onClick.AddListener(OnClickShop);
        }

        /// @brief Gestisce il click sul pulsante dello shop.
        private void OnClickShop() { }

        /// @brief Ottiene la quantità corrente di elisir.
        /// @return La quantità di elisir come intero.
        public int GetElisirAmount()
        {
            return int.Parse(_elisirAmount.text);
        }

        /// @brief Ottiene la quantità corrente di oro.
        /// @return La quantità di oro come intero.
        public int GetGoldAmount()
        {
            return int.Parse(_goldAmount.text);
        }

        /// @brief Ottiene il livello corrente.
        /// @return Il livello come intero.
        public int GetLevel()
        {
            return int.Parse(_level.text);
        }

        /// @brief Metodo chiamato ad ogni frame. Registra il livello corrente.
        private void Update()
        {
            Debug.Log("Current Level: " + GetLevel());
        }
    }
}