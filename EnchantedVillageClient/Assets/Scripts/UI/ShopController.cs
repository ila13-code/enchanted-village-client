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
        public GameObject shopItemTemplatePrefab;
        [SerializeField] private Transform container;

        private bool isConstructionOpen = true;
        private bool isProductionOpen = false;
        private bool isDefenseOpen = false;
        private bool isDecorationOpen = false;

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

        private void CreateItemButtons(ItemCategory category)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
            ItemType[] items = getByCategory(category);
   

            int itemsPerRow = 4;
            float itemWidth = 200f;
            float itemHeight = 200f;
            float spacing = 20f;

            int numRows = Mathf.CeilToInt((float)items.Length / itemsPerRow);
            float totalHeight = (numRows * itemHeight) + ((numRows - 1) * spacing);
            float totalWidth = (itemsPerRow * itemWidth) + ((itemsPerRow - 1) * spacing);
            float containerCenterY = container.GetComponent<RectTransform>().rect.center.y+((itemHeight/2)-20);
            float containerCenterX = container.GetComponent<RectTransform>().rect.center.x + ((itemWidth / 2) - 20);

            for (int i = 0; i < items.Length; i++)
            {
                if (items.Length == 4)
                {
                    spacing = -3f;
                    containerCenterX = container.GetComponent<RectTransform>().rect.center.x + ((itemWidth / 2) -55);
                }
                
                
                ItemType item = items[i];
                GameObject shopItemGameObject = Instantiate(shopItemTemplatePrefab, container);
                RectTransform shopItemRectTransform = shopItemGameObject.GetComponent<RectTransform>();

                shopItemGameObject.GetComponent<UIBuilding>().setPrefabIndex(GetIndex(item));

                int row = i / itemsPerRow;
                int col = i % itemsPerRow;
                float xPosition = (col * (itemWidth + spacing)) - (totalWidth / 2f) + (itemWidth / 2f) + (col * spacing) + containerCenterX;
                float yPosition = -(row * (itemHeight + spacing)) + containerCenterY - (totalHeight / 2f);
                shopItemRectTransform.anchoredPosition = new Vector2(xPosition, yPosition);

                shopItemGameObject.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = item.ToString().ToUpper();
                shopItemGameObject.transform.Find("Cost").GetComponent<TMPro.TextMeshProUGUI>().text = GetCost(item).ToString();
                shopItemGameObject.transform.Find("Image").GetComponent<Image>().sprite = GetSprite(item);
            }
        }
        private void Start()
        {
            setProductionOpen();
            CreateItemButtons(ItemCategory.production);
        }

        public void setConstructionOpen()
        {
            isConstructionOpen = true;
            isProductionOpen = false;
            isDefenseOpen = false;
            isDecorationOpen = false;
            CreateItemButtons(ItemCategory.construction);
        }

        public void setProductionOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = true;
            isDefenseOpen = false;
            isDecorationOpen = false;
            CreateItemButtons(ItemCategory.production);
        }

        public void setDefenseOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = false;
            isDefenseOpen = true;
            isDecorationOpen = false;
            CreateItemButtons(ItemCategory.defense);
        }

        public void setDecorationOpen()
        {
            isConstructionOpen = false;
            isProductionOpen = false;
            isDefenseOpen = false;
            isDecorationOpen = true;
            CreateItemButtons(ItemCategory.decoration);
        }

        public void disableButtonPressed(Button button)
        {
            button.interactable = false;
        }

    }
}