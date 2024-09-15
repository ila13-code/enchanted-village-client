using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{

    public class SceneCharacterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Transform spawnBase;
        [SerializeField] private float characterSpacing = 1f;
        [SerializeField] private int maxCharacters = 5;

        private List<GameObject> spawnedCharacters = new List<GameObject>();

        public void SpawnCharacter()
        {
            if (spawnedCharacters.Count >= maxCharacters)
            {
                Debug.Log("Numero massimo di personaggi raggiunto.");
                return;
            }

            Vector3 spawnPosition = CalculateSpawnPosition();
            GameObject newCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
            newCharacter.transform.SetParent(spawnBase);
            spawnedCharacters.Add(newCharacter);
        }

        private Vector3 CalculateSpawnPosition()
        {
            int characterIndex = spawnedCharacters.Count;
            float xOffset = characterIndex * characterSpacing;

            Vector3 basePosition = spawnBase.position;
            basePosition.x += xOffset;

            Collider2D baseCollider = spawnBase.GetComponent<Collider2D>();
            if (baseCollider != null)
            {
                basePosition.y = baseCollider.bounds.max.y;
            }
            else
            {
                // Se non c'è un collider, usiamo semplicemente la posizione Y della base
                basePosition.y = spawnBase.position.y;
            }

            return basePosition;
        }
    }
}
