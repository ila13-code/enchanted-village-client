namespace Unical.Demacs.EnchantedVillage
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using System;

    /// <summary>
    /// Controlla l'interfaccia utente del gioco.
    /// Implementa il pattern Singleton per garantire un'unica istanza.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        private static UIController instance = null;

        /// <summary>
        /// Testo che mostra la quantità di elisir.
        /// </summary>
        [SerializeField] private TextMeshProUGUI _elisirAmount;

        /// <summary>
        /// Testo che mostra la quantità di oro.
        /// </summary>
        [SerializeField] private TextMeshProUGUI _goldAmount;

        /// <summary>
        /// Testo che mostra il livello corrente.
        /// </summary>
        [SerializeField] private TextMeshProUGUI _level;

        /// <summary>
        /// Pulsante per accedere allo shop.
        /// </summary>
        [SerializeField] private Button _shop;

        /// <summary>
        /// Pulsante per iniziare una battaglia.
        /// </summary>
        [SerializeField] private Button _battle;

        /// <summary>
        /// Proprietà per accedere all'istanza singleton di UIController.
        /// </summary>
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
        /// <summary>
        /// Inizializza il controller UI.
        /// </summary>
        private void Start()
        {
            _shop.onClick.AddListener(OnClickShop);
        }

        /// <summary>
        /// Gestisce il click sul pulsante dello shop.
        /// </summary>
        private void OnClickShop() { }

        /// <summary>
        /// Ottiene la quantità corrente di elisir.
        /// </summary>
        /// <returns>La quantità di elisir come intero.</returns>
        public int GetElisirAmount()
        {
            return int.Parse(_elisirAmount.text);
        }

        /// <summary>
        /// Ottiene la quantità corrente di oro.
        /// </summary>
        /// <returns>La quantità di oro come intero.</returns>
        public int GetGoldAmount()
        {
            return int.Parse(_goldAmount.text);
        }

        /// <summary>
        /// Ottiene il livello corrente.
        /// </summary>
        /// <returns>Il livello come intero.</returns>
        public int GetLevel()
        {
            return int.Parse(_level.text);
        }

        /// <summary>
        /// Metodo chiamato ad ogni frame. Registra il livello corrente.
        /// </summary>
        private void Update()
        {
            Debug.Log("Current Level: " + GetLevel());
        }
    }
}