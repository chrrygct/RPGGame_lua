using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Base;
using XLua;
using RPG.AB;
using XLua.LuaDLL;

/// Lua管理器
/// 保证唯一性，负责Lua环境的创建、执行和销毁
public class LuaManager:BaseManager<LuaManager>
{
    private LuaEnv luaEnv;

    public LuaTable Global 
    {
        get
        {
            if (luaEnv == null)
            {
                Debug.LogError("LuaEnv is not initialized. Call Init() first.");
                return null;
            }
            return luaEnv.Global;
        }
    }
    //初始化Lua环境
    public void Init()
    {
        if (luaEnv == null)
        {
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(MyLoader);
            luaEnv.AddLoader(MyABLoader);
        }
    }
    //自动加载Lua脚本的加载器，默认从Assets/LuaScripts目录下加载
    private byte[] MyLoader(ref string filepath)
    {
        string path = Application.dataPath + "/LuaScripts/" + filepath + ".lua";
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllBytes(path);
        }
        else
        {
            Debug.Log("Lua file not found: " + path);
        }
        return null;
    }

    //重定向加载AB包中的Lua脚本
    private byte[] MyABLoader(ref string filepath)
    {
        //通过管理器加载AB包
        TextAsset luaTextAsset = ABManager.GetInstance().LoadRes<TextAsset>("lua", filepath+".lua");
        if (luaTextAsset != null)
        {
            return luaTextAsset.bytes;
        }
        else
        {
            Debug.Log("Lua file not found in AB: " + filepath);
            return null;
        }
    }



    //执行Lua脚本
    public void DoString(string luaScript)
    {
        if (luaEnv == null)
        {
            Debug.LogError("LuaEnv is not initialized. Call Init() first.");
            return;
        }
        
        luaEnv.DoString(luaScript);
    }

    //释放Lua垃圾
    public void Tick()
    {
        if (luaEnv == null)
        {
            Debug.LogError("LuaEnv is not initialized. Call Init() first.");
            return;
        }

        luaEnv.Tick();    
    }
    
    //销毁Lua环境，释放资源
    public void Dispose()
    {
        if (luaEnv == null)
        {
            Debug.LogError("LuaEnv is not initialized. Call Init() first.");
            return;
        }

        luaEnv.Dispose();
        luaEnv = null;
    }


}
