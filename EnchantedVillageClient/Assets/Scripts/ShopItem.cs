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

        public static Sprite getSprite(ItemType itemType)
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
                case ItemType.boath:
                    return Resources.Load<Sprite>("Sprites/boath");
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
    }
}
