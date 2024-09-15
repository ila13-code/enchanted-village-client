using System.Collections;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    //classe che gestisce la raccolta di risorse
    //ogni 3 minuti aumenta le risorse di 50 e attiva le particelle
    //usa una coroutine per aumentare le risorse
    public class ResourceCollector : MonoBehaviour
    {
        [SerializeField] private int type = 0; // 0 per elisir, 1 per oro
        [SerializeField] private GameObject particles;
        private int resources = 50;


        public int Resources
        {
            get
            {
                return resources;
            }
            set
            {
                resources = value;
            }
        }

        private void Start()
        {
            StartCoroutine(IncreaseResources());
            if (particles != null)
            {
                particles.SetActive(false);
            }
        }

        //coroutine che aumenta le risorse ogni 3 minuti
        private IEnumerator IncreaseResources()
        {
            while (true)
            {
                yield return new WaitForSeconds(180);
                IncreaseResources(50);

                if (particles != null)
                {
                    particles.SetActive(true);
                }
            }
        }

        private void IncreaseResources(int amount)
        {
            resources += amount;
        }
    }
}
