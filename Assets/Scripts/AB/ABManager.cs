using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Base;

namespace RPG.AB
{
    //AB包管理器，负责加载和卸载AB包
    //单例模式，保证全局只有一个ABManager实例
    public class ABManager : SingletoAutoMono<ABManager>
    {
        //目的：管理AB包的加载和卸载，提供接口供外部调用

        //AB包不能重复加载，使用字典来存储已经加载的AB包，key为AB包名称，value为AB包对象
        private Dictionary<string, AssetBundle> abDict = new Dictionary<string, AssetBundle>();

        //主包和依赖包的配置文件
        private AssetBundle mainAB=null;
        private AssetBundleManifest manifest=null;

        //这里是AB包的路径
        private string PathUrl 
        {
            get
            {
                return Application.streamingAssetsPath + "/";
            }
        }

        private string MainABName
        {
            get
            {
    #if UNITY_IOS
                return "IOS";

    #elif UNITY_ANDROID
                return "Android";
    #elif UNITY_STANDALONE_WIN
                return "StandaloneWindows";
    #endif
            }
        }

        //同步加载AB包,不指定类型
        public Object LoadRes(string abName,string resName)
        {
            LoadAB(abName);
            //加载资源
            Object obj = abDict[abName].LoadAsset(resName);
            if (obj is GameObject)
            {
                return Instantiate(obj);
            }
            else
            {
                return obj;
            }
        }
        //同步加载AB包,指定类型
        public Object LoadRes(string abName,string resName,System.Type type)
        {
            LoadAB(abName);
            //加载资源
            Object obj = abDict[abName].LoadAsset(resName, type);
            if (obj is GameObject)
            {
                return Instantiate(obj);
            }
            else
            {
                return obj;
            }
        }
        //同步加载AB包,指定类型，泛型
        public T LoadRes<T>(string abName,string resName) where T : Object
        {
            LoadAB(abName);
            //加载资源
            T obj = abDict[abName].LoadAsset<T>(resName);
            if (obj is GameObject)
            {
                return Instantiate(obj);
            }
            else
            {
                return obj;
            }
        }
        private void LoadAB(string abName)
        {
            //加载主包
            if (mainAB == null)
            {
                mainAB = AssetBundle.LoadFromFile(PathUrl + MainABName);
                manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            //加载主包中的依赖包信息
            string[] dependences = manifest.GetAllDependencies(abName);
            for (int i = 0; i < dependences.Length; i++)
            {
                //加载依赖包
                if (!abDict.ContainsKey(dependences[i]))
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + dependences[i]);
                    abDict.Add(dependences[i], ab);
                }
            }

            //加载包
            if (!abDict.ContainsKey(abName))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(PathUrl + abName);
                abDict.Add(abName, ab);
            }
        }





        //异步加载AB包，返回AB包对象
        public void LoadABAsync(string abName,string resName,System.Action<Object> callback)
        {
            StartCoroutine(LoadABAsyncCoroutine(abName, resName, callback));
        }
        IEnumerator LoadABAsyncCoroutine(string abName, string resName, System.Action<Object> callback)
        {
            //加载主包
            LoadAB(abName);
            //加载资源        
            AssetBundleRequest request = abDict[abName].LoadAssetAsync(resName);
            yield return request;

            if (request.asset is GameObject)
            {
                callback(Instantiate(request.asset));
            }
            else
            {
                callback(request.asset);
            }
        }


        //异步加载AB包，返回AB包对象,指定类型
        public void LoadABAsync(string abName,string resName,System.Type type,System.Action<Object> callback)
        {
            StartCoroutine(LoadABAsyncCoroutine(abName, resName, type, callback));
        }
        IEnumerator LoadABAsyncCoroutine(string abName, string resName, System.Type type, System.Action<Object> callback)
        {
            //加载主包
            LoadAB(abName);
            //加载资源        
            AssetBundleRequest request = abDict[abName].LoadAssetAsync(resName, type);
            yield return request;

            if (request.asset is GameObject)
            {
                callback(Instantiate(request.asset));
            }
            else
            {
                callback(request.asset);
            }
        }


        //异步加载AB包，返回AB包对象,指定类型，泛型
        public void LoadABAsync <T>(string abName,string resName,System.Action<T> callback) where T : Object
        {
            StartCoroutine(LoadABAsyncCoroutine<T>(abName, resName, callback));
        }
        IEnumerator LoadABAsyncCoroutine<T>(string abName, string resName, System.Action<T> callback) where T : Object
        {
            //加载主包
            LoadAB(abName);
            //加载资源        
            AssetBundleRequest request = abDict[abName].LoadAssetAsync<T>(resName);
            yield return request;

            if (request.asset is GameObject)
            {
                callback(Instantiate(request.asset) as T);
            }
            else
            {
                callback(request.asset as T);
            }
        }









        //卸载单个AB包，释放内存
        public void UnloadAB(string abName)
        {
            if (abDict.ContainsKey(abName))
            {
                abDict[abName].Unload(false);
                abDict.Remove(abName);
            }
        }

        //卸载所有AB包，释放内存
        public void UnloadAllAB()
        {
            AssetBundle.UnloadAllAssetBundles(false);
            abDict.Clear();
            mainAB = null;
            manifest = null;
        }
    }
}