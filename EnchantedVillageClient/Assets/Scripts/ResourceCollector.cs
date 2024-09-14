using System.Collections;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class ResourceCollector : MonoBehaviour
    {
        private static ResourceCollector instance = null;
        [SerializeField] private int type = 0; // 0 per elisir, 1 per oro
        [SerializeField] private GameObject particles;

        public static ResourceCollector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ResourceCollector>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ResourceCollector");
                        instance = go.AddComponent<ResourceCollector>();
                        instance.Initialize();
                    }
                }
                return instance;
            }
        }

        public int resources = 100;

        private void Initialize()
        {
            StartCoroutine(IncreaseResources());
            if (particles != null)
            {
                particles.SetActive(false); 
            }
        }

        private IEnumerator IncreaseResources()
        {
            while (true)
            {
                yield return new WaitForSeconds(180);

                IncreaseResources(50);

                // Attiva le particelle
                if (particles != null)
                {
                    particles.SetActive(true); 
                }

                yield return new WaitForSeconds(3); 

                if (particles != null)
                {
                    particles.SetActive(false);
                }
            }
        }

        private void IncreaseResources(int amount)
        {
            resources += amount;
        }
    }
}
