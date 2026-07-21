using TeachTcpServerExercises2;

/// <summary>
/// 消息处理器基类。服务端与客户端 BaseHandler 签名一致，
/// 额外持有 client 字段以便 Handler 向对应客户端发回响应。
/// </summary>
public abstract class BaseHandler
{
    // 反序列化后的消息对象
    public BaseMsg message;

    // 收到该消息的客户端连接，Handler 可通过此字段发回响应
    public ClientSocket client;

    // 处理消息的业务逻辑（由子类实现）
    public abstract void MsgHandle();
}
