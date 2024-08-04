using UnityEngine;
using UnityEngine.EventSystems;

namespace Unical.Demacs.EnchantedVillage
{
    public class ShopController : MonoBehaviour
    {
        private static ShopController instance = null;

        public static ShopController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ShopController>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ShopController");
                        instance = go.AddComponent<ShopController>();
                    }
                }
                return instance;
            }
        }
        private void Awake()
        {
            instance = this;
        }
    }
}