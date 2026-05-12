using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Base
{
//泛型的单例模式，适用于不需要挂载在游戏对象上的管理类
public class SingletoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T GetInstance()
    {
            return instance;
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }


}
}