using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
namespace Unical.Demacs.EnchantedVillage
{
    public class ShopController : MonoBehaviour
    {

        private static ShopController instance = null;
        public Boolean isActive = false;
        private InputControls _inputs = null;
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


        
   
        public void OnClickShop()
        {
            Debug.Log("cjdjdc");
            isActive = true;
        }


    }
}
