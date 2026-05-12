using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
namespace RPG.SceneManagement
{   

    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
        A,B,C,D,E
        }   
        [SerializeField] int sceneToLoad = 1;    
        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] float fadeWaitTime = 0.5f;
        private void OnTriggerEnter(Collider other) {
            if(other.tag=="Player")
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            
            DontDestroyOnLoad(gameObject);

            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            //Save current level
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            //Load current level
            savingWrapper.Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            savingWrapper.Save();//跨场景自动保存

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
            
            Destroy(gameObject);
            
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");
            NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(otherPortal.spawnPoint.position);
                player.transform.rotation = otherPortal.spawnPoint.rotation; // 如果需要旋转
            }


            // GameObject player = GameObject.FindWithTag("Player");
            // player.GetComponent<NavMeshAgent>().enabled = false; // 禁用 NavMeshAgent
            // player.transform.position = otherPortal.spawnPoint.position;
            // player.transform.rotation = otherPortal.spawnPoint.rotation; 
            // player.GetComponent<NavMeshAgent>().enabled = true; // 重新启用 NavMeshAgent
        }

        private Portal GetOtherPortal()
        {
            foreach(Portal portal in FindObjectsOfType<Portal>())
            {
                if(portal==this) continue;
                if (portal.destination != destination) continue;
                return portal;
            }
            return null;
        }





















        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
