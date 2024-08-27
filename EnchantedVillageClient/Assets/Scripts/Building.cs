using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unical.Demacs.EnchantedVillage
{
    public class Building : MonoBehaviour
    {
        [System.Serializable] public class Level
        {
            public int level;
            public Sprite icon;
            public GameObject mesh;
        }

        private BuildGrid _buildGrid = null;
        [SerializeField] private int _rows = 1;
        [SerializeField] private int _columns = 1;
        private int _currentX = 0;
        private int _currentY = 0;
        [SerializeField] private MeshRenderer _base = null;
        [SerializeField] private Level[] levels = null;
    }
    
}
