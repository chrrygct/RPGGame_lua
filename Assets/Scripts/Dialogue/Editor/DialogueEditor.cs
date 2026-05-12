using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
namespace RPG.Dialogue.Editor
{   
    public class DialogueEditor : EditorWindow
    {

        Dialogue selectedDialogue = null;
        [NonSerialized]
        GUIStyle nodeStyle;
        [NonSerialized]

        GUIStyle playerNodeStyle;
        [NonSerialized]
        DialogueNode draggingNode = null;
        [NonSerialized]
        Vector2 draggingOffset;
        [NonSerialized]
        DialogueNode creatingNode = null;//正在生孩子的父节点
        [NonSerialized]
        DialogueNode deletingNode = null;
        [NonSerialized]
        DialogueNode linkingParentNode = null;//正在连接的父节点
        [NonSerialized]
        bool draggingCanvas = false;
        [NonSerialized]
        Vector2 draggingCanvasOffset;
        const float canvasSize = 4000;
        const float backgroundSize = 50;




        Vector2 scrollPosition;

        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        private void OnGUI()
        {
            if (selectedDialogue != null)
            {
                ProcessEvents();

                //设置滚动区域，允许在编辑器窗口内拖动视图
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                //Debug.Log(scrollPosition);

                Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
                Texture2D backgroundTex = Resources.Load("background") as Texture2D;
                Rect texCoords = new Rect(0, 0, canvasSize / backgroundSize, canvasSize / backgroundSize);
                GUI.DrawTextureWithTexCoords(canvas, backgroundTex, texCoords);

                //先绘制连接线，再绘制节点，这样连接线会在节点下面，避免遮挡
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }

                //结束滚动区域
                EditorGUILayout.EndScrollView();

                //处理添加和删除节点的操作，放在最后以避免在迭代节点列表时修改它
                if (creatingNode != null) 
                {

                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }
                if (deletingNode != null)
                {

                    selectedDialogue.DeleteNode(deletingNode);
                    deletingNode = null;
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Dialogue Selected", EditorStyles.boldLabel);
            }
        }

        
        //处理鼠标事件，拖动节点
        private void ProcessEvents()
        {
            //鼠标按下事件，检查是否点击在某个节点上，如果是则开始拖动该节点，否则开始拖动画布
            if (Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if (draggingNode != null)
                {
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = draggingNode;//选中节点对象，显示在Inspector面板
                }
                else
                {
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    Selection.activeObject = selectedDialogue;//选中对话对象，显示在Inspector面板
                }
            }
            //鼠标拖动事件，更新节点的位置
            else if (Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                Undo.RecordObject(selectedDialogue, "Move Dialogue Node");
                draggingNode.SetPosition(Event.current.mousePosition + draggingOffset);

                GUI.changed = true;
            }
            //鼠标拖动事件，更新画布的位置
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;

                GUI.changed = true;
            }
            //鼠标松开事件，停止拖动节点
            else if (Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            //鼠标松开事件，停止拖动画布
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }

        }


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

        private void DrawNode(DialogueNode node)
        {
            //根据节点类型选择不同的样式
            GUIStyle style = nodeStyle;
            if (node.IsPlayerSpeaking())
            {
                style = playerNodeStyle;
            }
            GUILayout.BeginArea(node.GetRect(), style);


            EditorGUI.BeginChangeCheck();


            //EditorGUILayout.LabelField("Node:", EditorStyles.whiteLabel);
            node.SetText(EditorGUILayout.TextField(node.GetText()));

            GUILayout.BeginHorizontal();//水平布局，放置删除和添加子节点的按钮
            
            if (GUILayout.Button("x"))
            {
                deletingNode = node;
            }
            DrawLinkButtons(node);
            if (GUILayout.Button("+"))
            {
                creatingNode = node;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawConnections(DialogueNode node)
        {
            // 1. 计算起始位置（当前节点的右中点）
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);
            
            // 2. 遍历所有子节点
            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                // 3. 计算结束位置（子节点的左中点）
                Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
                
                // 4. 计算控制点偏移（贝塞尔曲线的控制点）
                Vector3 controlPointOffset = endPosition - startPosition;
                controlPointOffset.y = 0;           // 只在X轴上偏移
                controlPointOffset.x *= 0.8f;       // 偏移量为距离的80%
                
                // 5. 绘制贝塞尔曲线
                Handles.DrawBezier(
                    startPosition,                           // 起点
                    endPosition,                             // 终点
                    startPosition + controlPointOffset,      // 控制点1
                    endPosition - controlPointOffset,        // 控制点2
                    Color.white,                             // 白色线条
                    null,                                    // 没有纹理
                    4f);                                     // 线宽4像素
            }
        }

        //当编辑器窗口启用时，订阅Selection.selectionChanged事件，并初始化节点样式
        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChange;

            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            playerNodeStyle = new GUIStyle();
            playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            playerNodeStyle.normal.textColor = Color.white;
            playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
            playerNodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

        private void OnSelectionChange()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                selectedDialogue = newDialogue;
                Repaint();
            }
        }



        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID,int line)
        {
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if (dialogue != null)          
           {
                ShowEditorWindow();
                return true;
            }
            return false;
        }
        private void DrawLinkButtons(DialogueNode node)
        {
            //状态 1：初始状态（未选择父节点）
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("link"))
                {
                    linkingParentNode = node;
                }
            }
            //状态 2：取消链接（点击同一节点）
            else if (linkingParentNode == node)
            {
                if (GUILayout.Button("cancel"))
                {
                    linkingParentNode = null;
                }
            }
            //状态 3：取消已有链接（节点已是子节点）
            else if (linkingParentNode.GetChildren().Contains(node.name))
            {
                if (GUILayout.Button("unlink"))
                {
                    linkingParentNode.RemoveChild(node.name);
                    linkingParentNode = null;
                }
            }
            //状态 4：创建新链接
            else
            {
                if (GUILayout.Button("child"))
                {
                    linkingParentNode.AddChild(node.name);
                    linkingParentNode = null;
                }
            }
        }


    }
}