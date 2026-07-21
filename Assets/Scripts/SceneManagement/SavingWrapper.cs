using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RPG.Saving;
namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;
        void Awake()
        {
            string account = LoginManager.GetInstance().CurrentAccount;
            if (!string.IsNullOrEmpty(account))
            {
                // 已登录 → 从服务端加载存档
                LoadFromServer();
            }
            else
            {
                // 未登录 → 本地加载（开发调试用，保留）
                //StartCoroutine(LoadLastScene());
            }
        }

        IEnumerator LoadLastScene()
        {
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            
            yield return fader.FadeIn(fadeInTime);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();             // 服务端存档
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                LoadFromServer();   // 从服务端读档
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();           // 删除本地存档
            }
        }

        public void Save()
        {
            //GetComponent<SavingSystem>().Save(defaultSaveFile);  // 本地存档（已注释）
            SaveToServer();                                        // 服务端存档
        }

        public void Load()
        {
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(defaultSaveFile);
        }

        /// <summary>
        /// Portal 跨场景时调用：从服务端恢复当前场景实体状态（不切场景）
        /// </summary>
        public IEnumerator RestoreCurrentSceneFromServer()
        {
            string account = LoginManager.GetInstance().CurrentAccount;
            if (string.IsNullOrEmpty(account))
            {
                Debug.LogWarning("未登录，无法从服务端恢复");
                yield break;
            }
            var task = GetComponent<SavingSystem>().RestoreFromServerAsync(account);
            yield return new WaitUntil(() => task.IsCompleted);
        }

        // ===== 服务端存档 =====

        /// <summary>
        /// 保存到服务端（绑定当前登录账号），由 Save 按钮或 UI 调用
        /// </summary>
        public async void SaveToServer()
        {
            string account = LoginManager.GetInstance().CurrentAccount;
            if (string.IsNullOrEmpty(account))
            {
                Debug.LogWarning("未登录，无法保存到服务端");
                return;
            }
            bool ok = await GetComponent<SavingSystem>().SaveToServerAsync(account);
            Debug.Log(ok ? "服务端存档成功" : "服务端存档失败");
        }

        /// <summary>
        /// 从服务端读取存档（绑定当前登录账号），登录成功后调用
        /// </summary>
        public async void LoadFromServer()
        {
            string account = LoginManager.GetInstance().CurrentAccount;
            if (string.IsNullOrEmpty(account))
            {
                Debug.LogWarning("未登录，无法从服务端读取存档");
                return;
            }
            Fader fader = FindObjectOfType<Fader>();
            if (fader != null)
                fader.FadeOutImmediate();

            bool ok = await GetComponent<SavingSystem>().LoadFromServerAsync(account);
            if (ok && fader != null)
                StartCoroutine(fader.FadeIn(fadeInTime));
        }
    }


}


