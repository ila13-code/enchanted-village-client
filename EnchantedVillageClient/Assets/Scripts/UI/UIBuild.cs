using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class UIBuild : MonoBehaviour
    {
        private static UIBuild instance = null;
        public static UIBuild Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIBuild>();
                    if (instance == null)
                    {
                        instance = new UIBuild();
                        GameObject go = new GameObject("UIBuild");
                        instance = go.AddComponent<UIBuild>();
                    }
                }
                return instance;
            }
        }

        public void Confirm()
        {
            
        }

        public void Cancel()
        {

        }
    }
}
