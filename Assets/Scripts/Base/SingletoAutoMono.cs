using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Base
{
//泛型的单例模式，适用于不需要挂载在游戏对象上的管理类
public class SingletoAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T GetInstance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject();
            //设置对象的名字为类名，方便在层级视图中找到
            obj.name = typeof(T).ToString();
            instance = obj.AddComponent<T>();
            //设置对象在场景切换时不被销毁
            DontDestroyOnLoad(obj);

        }
        return instance;
    }




}
}