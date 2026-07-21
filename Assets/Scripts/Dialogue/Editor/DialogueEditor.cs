using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;

namespace RPG.Dialogue.Editor
{
    /// <summary>
    /// 对话编辑器窗口 —— 一个可视化的节点编辑器，用于编辑对话树（Dialogue Tree）。
    /// 你可以在这个窗口中拖拽节点、创建/删除节点、用贝塞尔曲线连接节点来表示对话的流转。
    /// 打开方式：菜单 Window -> Dialogue Editor，或者双击 Project 窗口中的 Dialogue 资源。
    /// </summary>
    public class DialogueEditor : EditorWindow
    {
        // ==================== 状态字段 ====================

        /// <summary>当前正在编辑的对话资源（ScriptableObject）</summary>
        Dialogue selectedDialogue = null;

        // -- 节点样式（GUIStyle 不可序列化，所以标记 [NonSerialized]） --
        [NonSerialized]
        GUIStyle nodeStyle;                     // 普通 NPC 节点的样式（背景图、文字颜色、内边距等）
        [NonSerialized]
        GUIStyle playerNodeStyle;               // 玩家节点的样式（与 NPC 节点视觉上区分）

        // -- 拖拽相关 --
        [NonSerialized]
        DialogueNode draggingNode = null;       // 当前正在拖拽的节点（null 表示没有拖拽任何节点）
        [NonSerialized]
        Vector2 draggingOffset;                  // 鼠标点击位置相对于节点左上角的偏移（用于保持拖拽时节点不"跳动"）
        [NonSerialized]
        bool draggingCanvas = false;             // 是否正在拖拽整个画布（空白区域拖动）
        [NonSerialized]
        Vector2 draggingCanvasOffset;            // 画布拖拽时的初始偏移

        // -- 操作标记（延迟执行，避免在遍历节点集合时修改集合） --
        [NonSerialized]
        DialogueNode creatingNode = null;       // 待创建子节点的父节点（点击 "+" 按钮后设置，下一帧 OnGUI 中执行创建）
        [NonSerialized]
        DialogueNode deletingNode = null;       // 待删除的节点（点击 "x" 按钮后设置，下一帧 OnGUI 中执行删除）
        [NonSerialized]
        DialogueNode linkingParentNode = null;  // 当前正在进行链接操作的父节点（点击 "link" 后设置，进入链接模式）

        // -- 滚动视图 --
        Vector2 scrollPosition;                  // 画布的滚动偏移量（EditorGUILayout.BeginScrollView 使用）

        // ==================== 常量 ====================

        const float canvasSize = 4000;           // 画布的总大小（像素），足够放下很多节点
        const float backgroundSize = 50;         // 背景纹理的原始尺寸（用于计算 TexCoords 平铺）

        // ==================== 菜单入口 ====================

        /// <summary>
        /// 通过 Unity 菜单栏打开对话编辑器窗口。
        /// [MenuItem] 特性让这个方法出现在 Window -> Dialogue Editor 菜单中。
        /// </summary>
        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            // GetWindow 会查找已有的同名窗口，找不到就创建一个新的
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        // ==================== 主绘制循环 ====================

        /// <summary>
        /// Unity Editor 的主绘制方法。每帧调用一次（约 10fps 在编辑器空闲时，鼠标操作时会更高）。
        /// 整体流程：
        /// 1. ProcessEvents() — 处理鼠标输入（拖拽节点、拖动画布）
        /// 2. 绘制滚动区域 + 背景纹理
        /// 3. 绘制所有连接线（贝塞尔曲线）
        /// 4. 绘制所有节点（带按钮的 GUI 区域）
        /// 5. 执行延迟操作（创建/删除节点）
        /// </summary>
        private void OnGUI()
        {
            if (selectedDialogue != null)
            {
                // ---- 第 1 步：处理输入事件 ----
                ProcessEvents();

                // ---- 第 2 步：开始滚动视图，绘制背景 ----
                // BeginScrollView 返回的 Vector2 是用户滚动后的新位置
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                // 获取一块 4000x4000 的矩形区域作为画布
                Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
                Texture2D backgroundTex = Resources.Load("background") as Texture2D;

                // 计算纹理坐标，使背景图在 4000x4000 的画布上平铺（4000/50 = 80 次）
                Rect texCoords = new Rect(0, 0, canvasSize / backgroundSize, canvasSize / backgroundSize);
                GUI.DrawTextureWithTexCoords(canvas, backgroundTex, texCoords);

                // ---- 第 3 步：先绘制连接线（在节点下层，避免遮挡） ----
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }

                // ---- 第 4 步：再绘制节点（在上层） ----
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }

                // ---- 第 5 步：结束滚动视图 ----
                EditorGUILayout.EndScrollView();

                // ---- 第 6 步：执行延迟操作（避免在遍历节点集合时修改集合） ----
                if (creatingNode != null)
                {
                    // 为 creatingNode 创建一个新的子节点
                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }
                if (deletingNode != null)
                {
                    // 删除指定节点
                    selectedDialogue.DeleteNode(deletingNode);
                    deletingNode = null;
                }
            }
            else
            {
                // 没有选中对话资源时，显示提示文字
                EditorGUILayout.LabelField("No Dialogue Selected", EditorStyles.boldLabel);
            }
        }

        // ==================== 输入事件处理 ====================

        /// <summary>
        /// 处理当前帧的鼠标事件。Unity 的 Event.current 代表当前正在处理的事件。
        ///
        /// 使用 if-else if 链来判断事件类型，确保每种事件只被处理一次：
        /// - MouseDown：检测点击了哪个节点（开始拖拽节点）或点击了空白区域（开始拖动画布）
        /// - MouseDrag：更新节点位置或画布滚动位置
        /// - MouseUp：停止拖拽
        ///
        /// 注意：Event.current.mousePosition 是相对于编辑器窗口的坐标，
        /// 需要加上 scrollPosition 才能转换为画布坐标。
        /// </summary>
        private void ProcessEvents()
        {
            // ----- 鼠标按下 -----
            // 条件：事件类型是 MouseDown 且当前没有正在拖拽的节点
            if (Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                // 用画布坐标检测鼠标是否点中了某个节点
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);

                if (draggingNode != null)
                {
                    // 点中了节点 → 开始拖拽节点
                    // 记录偏移量 = 节点左上角坐标 - 鼠标位置，这样拖拽时节点不会"跳"到鼠标位置
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;

                    // 将节点设为 Unity 的当前选中对象，Inspector 面板会显示它的属性
                    Selection.activeObject = draggingNode;
                }
                else
                {
                    // 没点中任何节点 → 开始拖动画布
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    // 选中对话资源本身，Inspector 面板会显示它的属性
                    Selection.activeObject = selectedDialogue;
                }
            }
            // ----- 拖拽节点 -----
            else if (Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                // 注册 Undo，让拖拽操作可以被撤销（Ctrl+Z）
                Undo.RecordObject(selectedDialogue, "Move Dialogue Node");

                // 新位置 = 鼠标位置 + 偏移量
                draggingNode.SetPosition(Event.current.mousePosition + draggingOffset);

                // 标记 GUI 已改变，让 Unity 重新绘制
                GUI.changed = true;
            }
            // ----- 拖动画布 -----
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                // 画布拖拽：用初始偏移减去当前鼠标位置，得到滚动量
                // 直观理解：鼠标向右拖 → scrollPosition 变小 → 画布向左滚动
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;

                GUI.changed = true;
            }
            // ----- 松开鼠标（结束拖拽节点） -----
            else if (Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            // ----- 松开鼠标（结束拖动画布） -----
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }
        }

        // ==================== 节点查找 ====================

        /// <summary>
        /// 检测给定画布坐标点是否在某个节点矩形内。
        /// 返回最上层匹配的节点（遍历顺序决定了层级，后绘制的在上层，但由于没有重叠处理，找到最后一个匹配的即可）。
        /// </summary>
        /// <param name="point">画布坐标（已加上 scrollPosition 的坐标）</param>
        /// <returns>命中的节点，没有命中则返回 null</returns>
        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null;
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if (node.GetRect().Contains(point))
                {
                    foundNode = node;
                }
            }
            return foundNode;
        }

        // ==================== 节点绘制 ====================

        /// <summary>
        /// 绘制单个节点及其所有 UI 元素：
        /// - 文本输入框（对话内容）
        /// - "x" 删除按钮
        /// - link/child/cancel/unlink 连接按钮（状态机逻辑）
        /// - "+" 添加子节点按钮
        /// </summary>
        private void DrawNode(DialogueNode node)
        {
            // 根据节点说话者选择样式：玩家用 playerNodeStyle（绿色/蓝色背景），NPC 用 nodeStyle（灰色背景）
            GUIStyle style = nodeStyle;
            if (node.IsPlayerSpeaking())
            {
                style = playerNodeStyle;
            }

            // BeginArea 创建一个矩形 GUI 区域，后续的 GUI 控件都会相对于这个区域的左上角布局
            GUILayout.BeginArea(node.GetRect(), style);

            // 检测文本是否被修改（用于 Undo 支持）
            EditorGUI.BeginChangeCheck();

            // 可编辑的文本输入框，显示节点的对话内容
            node.SetText(EditorGUILayout.TextField(node.GetText()));

            // 水平布局：按钮排成一行
            GUILayout.BeginHorizontal();

            // "x" 按钮 — 删除当前节点
            if (GUILayout.Button("x"))
            {
                deletingNode = node;  // 不立即删除，等下一帧再执行（避免在遍历中修改集合）
            }

            // 链接按钮 — 状态机控制（详见 DrawLinkButtons 的注释）
            DrawLinkButtons(node);

            // "+" 按钮 — 为当前节点创建一个子节点
            if (GUILayout.Button("+"))
            {
                creatingNode = node;  // 不立即创建，等下一帧再执行
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // ==================== 连接线绘制 ====================

        /// <summary>
        /// 绘制从当前节点到其所有子节点的贝塞尔连接线。
        /// 起点：父节点的右边缘中点（xMax, center.y）
        /// 终点：子节点的左边缘中点（xMin, center.y）
        /// 控制点：在 X 轴方向偏移 80% 的水平距离，使曲线呈平滑的 S 形。
        /// </summary>
        private void DrawConnections(DialogueNode node)
        {
            // 起点：当前节点的右边缘中点
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);

            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                // 终点：子节点的左边缘中点
                Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);

                // 计算控制点偏移量：
                // - 只保留 X 分量（y=0），让曲线在水平方向上弯曲
                // - 乘以 0.8 控制弯曲程度（0.8 = 弯得比较多，1.0 = 更弯，0 = 直线）
                Vector3 controlPointOffset = endPosition - startPosition;
                controlPointOffset.y = 0;
                controlPointOffset.x *= 0.8f;

                // 使用 Unity Editor 的 Handles 绘制贝塞尔曲线（看起来比 GUI 线条更专业）
                Handles.DrawBezier(
                    startPosition,                           // 起点
                    endPosition,                             // 终点
                    startPosition + controlPointOffset,      // 起始控制点（从起点向右延伸）
                    endPosition - controlPointOffset,        // 结束控制点（从终点向左延伸）
                    Color.white,                             // 线条颜色
                    null,                                    // 不使用纹理
                    4f);                                     // 线宽 4 像素
            }
        }

        // ==================== 生命周期 ====================

        /// <summary>
        /// 编辑器窗口启用时调用（窗口创建或 Unity 重新编译后）。
        /// 在这里：
        /// 1. 订阅选择变化事件，以便在 Project 窗口中点击 Dialogue 资源时自动加载
        /// 2. 初始化节点样式（GUIStyle 不能在声明时初始化，因为那时 Unity 还没准备好加载资源）
        /// </summary>
        private void OnEnable()
        {
            // 订阅 Selection 变化事件：当用户在 Project/Inspector 中选择不同对象时触发
            Selection.selectionChanged += OnSelectionChange;

            // 初始化 NPC 节点样式
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;  // 灰色节点背景图
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);   // 内容到边框的内边距
            nodeStyle.border = new RectOffset(12, 12, 12, 12);    // 九宫格边框（拉伸时不失真的区域）

            // 初始化玩家节点样式（使用不同的背景图来区分）
            playerNodeStyle = new GUIStyle();
            playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;  // 绿色/蓝色节点背景图
            playerNodeStyle.normal.textColor = Color.white;
            playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
            playerNodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

        /// <summary>
        /// 当 Unity 的 Selection.activeObject 改变时调用。
        /// 如果用户选中的是一个 Dialogue 资源，就把它加载到编辑器中。
        /// </summary>
        private void OnSelectionChange()
        {
            // 尝试将当前选中的对象转换为 Dialogue 类型
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                selectedDialogue = newDialogue;
                // Repaint() 触发 OnGUI 重新绘制，让新对话的节点立即显示
                Repaint();
            }
        }

        // ==================== 双击资源打开编辑器 ====================

        /// <summary>
        /// [OnOpenAsset] 特性：当用户在 Project 窗口中双击一个资源时，Unity 会调用所有标记了此特性的方法。
        /// 参数 1 是回调顺序（数字越小越先调用）。
        ///
        /// 这个方法的作用：双击 Dialogue 资源时自动打开 Dialogue Editor 窗口，而不是进入默认的 Inspector 编辑模式。
        /// 返回 true 表示"我已处理，不需要默认行为"；返回 false 表示"我没处理，交给下一个处理器"。
        /// </summary>
        /// <param name="instanceID">被双击资源的实例 ID</param>
        /// <param name="line">行号（通常为 -1）</param>
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            // 通过实例 ID 获取对象，检查是否为 Dialogue 资源
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if (dialogue != null)
            {
                // 是 Dialogue 资源 → 打开编辑器窗口并阻止默认打开行为
                ShowEditorWindow();
                return true;
            }
            // 不是 Dialogue 资源 → 让其他处理器处理
            return false;
        }

        // ==================== 链接按钮 — 状态机 ====================

        /// <summary>
        /// 绘制链接按钮。这是一个 4 状态的状态机，用于管理节点之间的父子关系：
        ///
        /// 状态 1【初始状态】: linkingParentNode == null
        ///     → 显示 "link" 按钮。点击后进入状态 2/3/4，当前节点成为"正在链接的父节点"。
        ///
        /// 状态 2【取消】：linkingParentNode == 当前节点
        ///     → 显示 "cancel" 按钮。点击后回到状态 1（取消链接操作）。
        ///
        /// 状态 3【断开已有链接】：linkingParentNode 的 Children 中已包含当前节点
        ///     → 显示 "unlink" 按钮。点击后移除父子关系，回到状态 1。
        ///
        /// 状态 4【创建新链接】：linkingParentNode != null 且 != 当前节点 且 当前节点不在 Children 中
        ///     → 显示 "child" 按钮。点击后将当前节点添加为子节点，回到状态 1。
        ///
        /// 使用流程示例：
        /// 1. 点击节点 A 的 "link" → linkingParentNode = A（进入链接模式）
        /// 2. 点击节点 B 的 "child" → B 成为 A 的子节点，退出链接模式
        /// 3. 再次点击 A 的 "link"，再点击 B 的 "unlink" → 断开 A-B 关系
        /// </summary>
        private void DrawLinkButtons(DialogueNode node)
        {
            // ---- 状态 1：初始状态（没有进行中的链接操作） ----
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("link"))
                {
                    // 开始链接操作，记住"谁想成为父节点"
                    linkingParentNode = node;
                }
            }
            // ---- 状态 2：取消链接（再次点击同一个节点） ----
            else if (linkingParentNode == node)
            {
                if (GUILayout.Button("cancel"))
                {
                    // 取消链接操作，回到初始状态
                    linkingParentNode = null;
                }
            }
            // ---- 状态 3：断开已有链接（节点已经是父节点的子节点） ----
            else if (linkingParentNode.GetChildren().Contains(node.name))
            {
                if (GUILayout.Button("unlink"))
                {
                    // 移除父子关系
                    linkingParentNode.RemoveChild(node.name);
                    linkingParentNode = null;
                }
            }
            // ---- 状态 4：创建新链接（节点不是子节点，可以链接） ----
            else
            {
                if (GUILayout.Button("child"))
                {
                    // 将当前节点添加为父节点的子节点
                    linkingParentNode.AddChild(node.name);
                    linkingParentNode = null;
                }
            }
        }
    }
}
