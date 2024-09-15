using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unical.Demacs.EnchantedVillage
{
    public class ShopItem : MonoBehaviour
    {
        public enum ItemType
        {
            cannon,
            tower,
            wall,
            barrack,
            trainingBase,
            boat,
            camp,
            flag,
            tree,
            elisirStorage,
            elisirCollector,
            goldStorage,
            goldCollector
        }

        public enum ItemCategory
        {
            construction,
            production,
            defense,
            decoration,
            nullCategory
        }

        public static ItemCategory GetCategory(ItemType itemType)
        {
            if(itemType.Equals(ItemType.cannon) || itemType.Equals(ItemType.tower) || itemType.Equals(ItemType.wall))
            {
                return ItemCategory.defense;
            }
            else if(itemType.Equals(ItemType.elisirStorage) || itemType.Equals(ItemType.elisirCollector) || itemType.Equals(ItemType.goldStorage) || itemType.Equals(ItemType.goldCollector))
            {
                return ItemCategory.production;
            }
            else if(itemType.Equals(ItemType.barrack) || itemType.Equals(ItemType.trainingBase)  || itemType.Equals(ItemType.camp))
            {
                return ItemCategory.construction;
            }
            else if(itemType.Equals(ItemType.flag) || itemType.Equals(ItemType.tree) || itemType.Equals(ItemType.boat))
            {
                 return ItemCategory.decoration;
            }
            else
            {
                return ItemCategory.nullCategory;
            }
        }

        public static ItemType[] getByCategory(ItemCategory category)
        {
            switch(category)
            {
                case ItemCategory.construction:
                    return new ItemType[] { ItemType.barrack, ItemType.trainingBase, ItemType.camp };
                case ItemCategory.production:
                    return new ItemType[] { ItemType.elisirStorage, ItemType.elisirCollector, ItemType.goldStorage, ItemType.goldCollector };
                case ItemCategory.defense:
                    return new ItemType[] { ItemType.cannon, ItemType.tower, ItemType.wall };
                case ItemCategory.decoration:
                    return new ItemType[] { ItemType.flag, ItemType.tree, ItemType.boat };
                default:
                    return new ItemType[] { };
            }
        }

        public static int GetCost(ItemType item)
        {
            switch (item)
            {
                case ItemType.cannon:
                    return 100;
                case ItemType.tower:
                    return 200;
                case ItemType.wall:
                    return 20;
                case ItemType.barrack:
                    return 150;
                case ItemType.trainingBase:
                   return 200;
                case ItemType.boat:
                    return 100;
                case ItemType.camp:
                    return 100;
                case ItemType.flag:
                    return 50;
                case ItemType.tree:
                    return 20;
                case ItemType.elisirStorage:
                    return 100;
                case ItemType.elisirCollector:
                    return 100;
                case ItemType.goldStorage:
                    return 100;
                case ItemType.goldCollector:
                    return 100;
                default:
                    return 0;

            }
        }

        public static Sprite GetSprite(ItemType itemType)
        {
            switch(itemType)
            {
                case ItemType.cannon:
                    return Resources.Load<Sprite>("Sprites/cannon");
                case ItemType.tower:
                    return Resources.Load<Sprite>("Sprites/tower");
                case ItemType.wall:
                    return Resources.Load<Sprite>("Sprites/wall");
                case ItemType.barrack:
                    return Resources.Load<Sprite>("Sprites/barrack");
                case ItemType.trainingBase:
                    return Resources.Load<Sprite>("Sprites/trainingBase");
                case ItemType.boat:
                    return Resources.Load<Sprite>("Sprites/boat");
                case ItemType.camp:
                    return Resources.Load<Sprite>("Sprites/camp");
                case ItemType.flag:
                    return Resources.Load<Sprite>("Sprites/flag");
                case ItemType.tree:
                    return Resources.Load<Sprite>("Sprites/tree1");
                case ItemType.elisirStorage:
                    return Resources.Load<Sprite>("Sprites/elisirStorage");
                case ItemType.elisirCollector:
                    return Resources.Load<Sprite>("Sprites/elisirCollector");
                case ItemType.goldStorage:
                    return Resources.Load<Sprite>("Sprites/goldStorage");
                case ItemType.goldCollector:
                    return Resources.Load<Sprite>("Sprites/goldCollector");
                default:
                    return null;
            }   
        }

        public static int GetIndex(ItemType itemType)
        {
            switch(itemType)
            {
                case ItemType.cannon:
                    return 0;
                case ItemType.tower:
                    return 1;
                case ItemType.wall:
                    return 2;
                case ItemType.barrack:
                    return 3;
                case ItemType.trainingBase:
                    return 4;
                case ItemType.boat:
                    return 5;
                case ItemType.camp:
                    return 6;
                case ItemType.flag:
                    return 7;
                case ItemType.tree:
                    return 8;
                case ItemType.elisirStorage:
                    return 9;
                case ItemType.elisirCollector:
                    return 10;
                case ItemType.goldStorage:
                    return 11;
                case ItemType.goldCollector:
                    return 12;
                default:
                    return 0;
            }
        }

        public static ItemType GetItemType(int index)
        {
            switch (index)
            {
                case 0:
                    return ItemType.cannon;
                case 1:
                    return ItemType.tower;
                case 2:
                    return ItemType.wall;
                case 3:
                    return ItemType.barrack;
                case 4:
                    return ItemType.trainingBase;
                case 5:
                    return ItemType.boat;
                case 6:
                    return ItemType.camp;
                case 7:
                    return ItemType.flag;
                case 8:
                    return ItemType.tree;
                case 9:
                    return ItemType.elisirStorage;
                case 10:
                    return ItemType.elisirCollector;
                case 11:
                    return ItemType.goldStorage;
                case 12:
                    return ItemType.goldCollector;
                default:
                    return ItemType.cannon;
            }
        }

        public static int GetCostFromIndex(int index)
        {
            return GetCost(GetItemType(index));
        }

        public static int GetExperiencePoints(int index)
        {
            switch (index)
            {
                case 0:
                    return 6; 
                case 1:
                    return 15; 
                case 2:
                    return 10; 
                case 3:
                    return 20; 
                case 4:
                    return 10; 
                case 5:
                    return 12; 
                case 6:
                    return 18; 
                case 7:
                    return 17; 
                case 8:
                    return 9;  
                case 9:
                    return 14;
                case 10:
                    return 15; 
                case 11:
                    return 16; 
                case 12:
                    return 17; 
                default:
                    return 0;  
            }

        }
    }
}
