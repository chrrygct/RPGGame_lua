using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Core
{   
    public class PeristenObjectSpawner : MonoBehaviour
    {

        [SerializeField] GameObject peristenObjectPrefab;
        static bool hasSpawned = false;
        // Start is called before the first frame update
        void Awake()
        {
            if (hasSpawned) return;
            SpawnPeristenObject();
            hasSpawned = true;
        }

        private void SpawnPeristenObject()
        {
            GameObject peristenObject = Instantiate(peristenObjectPrefab);
            DontDestroyOnLoad(peristenObject);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
