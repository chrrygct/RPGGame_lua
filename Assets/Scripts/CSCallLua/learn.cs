using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using XLua.LuaDLL;


public class learn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LuaManager.GetInstance().Init();

        LuaManager.GetInstance().DoString("require 'test'");

        //使用Global属性
        int i =LuaManager.GetInstance().Global.Get<int>("testnumber");
        Debug.Log("testnumber:" + i);

        Action action = LuaManager.GetInstance().Global.Get<Action>("testfunc");
        action();

        

        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
