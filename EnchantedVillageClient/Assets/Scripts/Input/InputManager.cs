using UnityEngine;
using UnityEngine.EventSystems;

namespace Unical.Demacs.EnchantedVillage
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InputManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InputManager");
                        _instance = go.AddComponent<InputManager>();
                    }
                }
                return _instance;
            }
        }

        public InputControls Controls { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            Controls = new InputControls();
            Controls.Enable();
        }

        private void OnDestroy()
        {
            if (Controls != null)
            {
                Controls.Disable();
            }
        }

        public bool IsPointerOverUIElement()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}