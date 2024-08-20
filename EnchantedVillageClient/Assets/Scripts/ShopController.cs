using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unical.Demacs.EnchantedVillage.ShopItem;

namespace Unical.Demacs.EnchantedVillage
{
    public class ShopController : MonoBehaviour
    {
        private static ShopController instance = null;
        private Transform shopItemTemplate;
       
        private Boolean isConstructionOpen = true;
        private Boolean isProductionOpen = false;
        private Boolean isDefenseOpen = false;
        private Boolean isDecorationOpen = false;

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
            shopItemTemplate = transform.Find("ShopItemTemplate");
            
        }

        private void CreateItemButtons(ItemCategory category)
        {
            Transform shopItemTransform=Instantiate(shopItemTemplate);
            RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
            ItemType[] items = getByCategory(category);
            for(int i = 0; i < items.Length; i++)
            {
                ItemType item = items[i];
                shopItemRectTransform.Find("itemName").GetComponent<Text>().text = item.ToString();
                shopItemRectTransform.Find("itemCost").GetComponent<Text>().text = GetCost(item).ToString();
                shopItemRectTransform.Find("itemImage").GetComponent<Image>().sprite = GetSprite(item);
                //shopItemRectTransform.Find("itemButton").GetComponent<Button>().onClick.AddListener(() => OnShopItemButtonClick(item));
                shopItemTransform = Instantiate(shopItemTemplate);
                shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
            }
        }

        public void setConstructionOpen()
        {
            isConstructionOpen = true;
            isProductionOpen = false;
            isDefenseOpen = false;
            isDecorationOpen = false;
        }

        public void setProductionOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = true;
            isDefenseOpen = false;
            isDecorationOpen = false;
        }

        public void setDefenseOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = false;
            isDefenseOpen = true;
            isDecorationOpen = false;
        }

        public void setDecorationOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = false;
            isDefenseOpen = false;
            isDecorationOpen = true;
        }
    }
}