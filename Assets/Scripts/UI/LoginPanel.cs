using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace RPG.UI
{
    // 登录/注册面板。账号密码输入后点按钮发送请求，结果通过 LoginManager 的事件回调刷新提示文字。
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] TMP_InputField accountInput = null;
        [SerializeField] TMP_InputField passwordInput = null;
        [SerializeField] Button loginButton = null;
        [SerializeField] Button registerButton = null;
        [SerializeField] TextMeshProUGUI tipText = null;

        [SerializeField] string serverIp = "127.0.0.1";
        [SerializeField] int serverPort = 8080;
        // 登录成功后要加载的第一个场景。用场景名而非 build index，避免调整 Build Settings 顺序后失效。
        [SerializeField] string firstSceneName = "Scene01";

        private void Start()
        {
            LoginManager.GetInstance().onLoginResult += OnLoginResult;
            LoginManager.GetInstance().onRegisterResult += OnRegisterResult;

            loginButton.onClick.AddListener(OnLoginClick);
            registerButton.onClick.AddListener(OnRegisterClick);

            LoginManager.GetInstance().Connect(serverIp, serverPort);
            SetTip("");
        }

        private void OnDestroy()
        {
            LoginManager manager = LoginManager.GetInstance();
            if (manager != null)
            {
                manager.onLoginResult -= OnLoginResult;
                manager.onRegisterResult -= OnRegisterResult;
            }
        }

        private bool TryGetInput(out string account, out string password)
        {
            account = accountInput.text;
            password = passwordInput.text;
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                SetTip("Account and password cannot be empty");
                return false;
            }
            return true;
        }

        private void OnLoginClick()
        {
            if (!TryGetInput(out string account, out string password)) return;
            SetTip("Logging in...");
            LoginManager.GetInstance().Login(account, password);
        }

        private void OnRegisterClick()
        {
            if (!TryGetInput(out string account, out string password)) return;
            SetTip("Registering...");
            LoginManager.GetInstance().Register(account, password);
        }

        private void OnLoginResult(int result, string info)
        {
            SetTip(info);
            if (result == 0)
            {
                // 登录成功，跳转到第一个场景。
                // 回调由 NetAsyncMgr.Update 在主线程触发，这里可以安全调用 Unity API。
                SceneManager.LoadScene(firstSceneName);
            }
        }

        private void OnRegisterResult(int result, string info)
        {
            SetTip(info);
        }

        private void SetTip(string text)
        {
            if (tipText != null) tipText.text = text;
        }
    }
}
