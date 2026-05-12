using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {

        // Update is called once per frame
        [SerializeField] GameObject targetToDestroy= null;
        void Update()
        {
            if (!GetComponent<ParticleSystem>().IsAlive())
            {
                if (targetToDestroy != null)
                {
                    Destroy(targetToDestroy);
                }
                else
                {
                    Destroy(gameObject);
                }
                
            }
        }
    }




}
