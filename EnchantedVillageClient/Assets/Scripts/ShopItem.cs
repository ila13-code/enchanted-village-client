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
            boath,
            camp,
            flag,
            tree,
            elisirStorage,
            elisirCollector,
            goldStorage,
            goldCollector
        }
        
        public static int getCost(ItemType item)
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
                case ItemType.boath:
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

    }
}
