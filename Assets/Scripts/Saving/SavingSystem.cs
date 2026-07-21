using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using GameSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Saving
{
    public class SavingSystem : MonoBehaviour
    {
        public IEnumerator LoadLastScene(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (state.ContainsKey("lastSceneBuildIndex"))
            {
                buildIndex = (int)state["lastSceneBuildIndex"];
            }
            yield return SceneManager.LoadSceneAsync(buildIndex);
            RestoreState(state);
        }

        public void Save(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            CaptureState(state);
            SaveFile(saveFile, state);
        }

        public void Load(string saveFile)
        {
            RestoreState(LoadFile(saveFile));
        }

        public void Delete(string saveFile)
        {
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        // ===== 服务端存档（异步，走网络） =====

        /// <summary>
        /// 将当前场景状态保存到服务端（绑定账号）。
        /// 先从服务端拉取已有存档 → 合并当前场景状态 → 保存回服务端，避免覆盖其他场景的数据。
        /// </summary>
        public async Task<bool> SaveToServerAsync(string account)
        {
            try
            {
                // 1. 先从服务端加载已有存档
                Dictionary<string, object> state = null;
                LoadMsg loadMsg = new LoadMsg();
                loadMsg.account = account;
                BaseMsg reply = await NetAsyncMgr.Instance.SendAndWaitAsync(loadMsg, 1010);
                LoadResultMsg loadResult = reply as LoadResultMsg;
                if (loadResult != null && loadResult.result == 0 && loadResult.saveData != null && loadResult.saveData.Length > 0)
                    state = DeserializeState(loadResult.saveData);

                // 没有旧存档就新建空字典
                if (state == null)
                    state = new Dictionary<string, object>();

                // 2. 合并当前场景状态（不会覆盖其他场景的实体数据）
                CaptureState(state);

                // 3. 序列化并保存到服务端
                byte[] saveData = SerializeState(state);
                SaveMsg saveMsg = new SaveMsg();
                saveMsg.account = account;
                saveMsg.saveData = saveData;

                reply = await NetAsyncMgr.Instance.SendAndWaitAsync(saveMsg, 1008);
                SaveResultMsg result = reply as SaveResultMsg;

                if (result != null && result.result == 0)
                {
                    Debug.Log("服务端存档成功: " + result.info);
                    return true;
                }
                Debug.LogError("服务端存档失败: " + (result != null ? result.info : "无回复"));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("服务端存档异常: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 从服务端读取存档并恢复场景（绑定账号）
        /// </summary>
        public async Task<bool> LoadFromServerAsync(string account)
        {
            try
            {
                LoadMsg loadMsg = new LoadMsg();
                loadMsg.account = account;

                BaseMsg reply = await NetAsyncMgr.Instance.SendAndWaitAsync(loadMsg, 1010);
                LoadResultMsg result = reply as LoadResultMsg;

                if (result != null && result.result == 0 && result.saveData != null && result.saveData.Length > 0)
                {
                    Dictionary<string, object> state = DeserializeState(result.saveData);
                    int buildIndex = SceneManager.GetActiveScene().buildIndex;
                    if (state.ContainsKey("lastSceneBuildIndex"))
                        buildIndex = (int)state["lastSceneBuildIndex"];
                    await LoadSceneTaskAsync(buildIndex);
                    RestoreState(state);
                    Debug.Log("服务端读档成功: " + result.info);
                    return true;
                }
                Debug.Log("服务端读档: " + (result != null ? result.info : "无回复"));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("服务端读档异常: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 从服务端读取存档并恢复当前场景实体状态（不切场景）。
        /// Portal 跨场景时调用，只恢复刚加载的新场景的实体数据。
        /// </summary>
        public async Task<bool> RestoreFromServerAsync(string account)
        {
            try
            {
                LoadMsg loadMsg = new LoadMsg();
                loadMsg.account = account;

                BaseMsg reply = await NetAsyncMgr.Instance.SendAndWaitAsync(loadMsg, 1010);
                LoadResultMsg result = reply as LoadResultMsg;

                if (result != null && result.result == 0 && result.saveData != null && result.saveData.Length > 0)
                {
                    Dictionary<string, object> state = DeserializeState(result.saveData);
                    RestoreState(state);
                    Debug.Log("当前场景恢复成功");
                    return true;
                }
                Debug.Log("无存档，跳过恢复");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("当前场景恢复异常: " + e.Message);
                return false;
            }
        }

        private byte[] SerializeState(Dictionary<string, object> state)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
                return stream.ToArray();
            }
        }

        private Task LoadSceneTaskAsync(int buildIndex)
        {
            var tcs = new TaskCompletionSource<bool>();
            AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
            op.completed += (_) => tcs.SetResult(true);
            return tcs.Task;
        }

        private Dictionary<string, object> DeserializeState(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        // ===== 本地文件存档（保留，作为离线模式备选） =====

        private Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path))
            {
                return new Dictionary<string, object>();
            }
            FileInfo info = new FileInfo(path);
            if (info.Length == 0)
            {
                return new Dictionary<string, object>();
            }
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (Dictionary<string, object>)formatter.Deserialize(stream);
                }
                catch (Exception e)
                {
                    Debug.LogError($"存档文件损坏，已忽略并使用空状态：{path}\n{e}");
                    return new Dictionary<string, object>();
                }
            }
        }

        private void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Saving to " + path);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        private void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                state[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
            }

            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
        }
    }
}