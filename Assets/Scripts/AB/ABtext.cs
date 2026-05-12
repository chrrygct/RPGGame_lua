using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.AB;
public class ABtext : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GameObject obj = ABManager.Instance.LoadRes<GameObject>("character", "Character_Knights_Soldier_01");
        
        ABManager.GetInstance().LoadABAsync<GameObject>("character", "Character_Knights_Soldier_01", (obj) =>
        {
            obj.transform.position = new Vector3(0, 0, 0);
        });


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
