using System;
using GameLogin;
using RPG.Base;

// 注册/登录的客户端入口与结果分发中心。
// Handler 收到服务器响应后触发这里的事件，UI 面板订阅事件刷新界面，使网络层不依赖任何具体 UI。
// result 约定：0 = 成功，非 0 = 失败（失败原因直接用服务器回传的 info 文字显示）。
public class LoginManager : BaseManager<LoginManager>
{
    public string CurrentAccount { get; private set; }
    public event Action<int, string> onLoginResult;
    public event Action<int, string> onRegisterResult;

    public void Connect(string ip, int port)
    {
        NetAsyncMgr.Instance.Connect(ip, port);
    }

    public void Login(string account, string password)
    {
        CurrentAccount = account;
        LoginMsg msg = new LoginMsg();
        msg.account = account;
        msg.password = password;
        NetAsyncMgr.Instance.Send(msg);
    }

    public void Register(string account, string password)
    {
        RegisterMsg msg = new RegisterMsg();
        msg.account = account;
        msg.password = password;
        NetAsyncMgr.Instance.Send(msg);
    }

    // 由 LoginResultMsgHandler 调用（event 只能在声明类内部 Invoke）。
    public void RaiseLoginResult(int result, string info)
    {
        onLoginResult?.Invoke(result, info);
    }

    // 由 RegisterResultMsgHandler 调用。
    public void RaiseRegisterResult(int result, string info)
    {
        onRegisterResult?.Invoke(result, info);
    }
}
