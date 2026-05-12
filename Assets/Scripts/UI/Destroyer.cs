using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] GameObject targetToDestory=null;
    public void DestroyTarget()
    {
        if (targetToDestory != null)
        {
            Destroy(targetToDestory);
        }
    }

}
