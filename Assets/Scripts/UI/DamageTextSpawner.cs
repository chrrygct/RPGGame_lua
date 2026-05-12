using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.UI
{
    public class DamageTextSpawner : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] DamageText damageTextPrefab=null;

        void Start()
        {
            //Spawn(10f);
        }
        public void Spawn(float damageAmount)
        {
            DamageText damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
            damageText.SetDamageAmount(damageAmount);
        }
    }
}