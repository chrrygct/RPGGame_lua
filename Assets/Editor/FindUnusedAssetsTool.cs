using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 扫描 Asset Packs 下未被任何场景或 Resources 引用的资源。
/// 菜单：Tools / Find Unused Assets In "Asset Packs"
/// </summary>
public class FindUnusedAssetsTool : EditorWindow
{
    // 只扫描这个根目录下的资源
    const string ScanRoot = "Assets/Asset Packs";

    // 输出报告路径（项目根目录下）
    const string ReportPath = "unused_assets_report.txt";

    // 在结果里默认排除的路径关键字（防止误删）
    static readonly string[] ExcludePathContains = new[]
    {
        "/Editor/",
        "/Editor.",
        "/Resources/",
        "/StreamingAssets/",
        "/Plugins/",
        "/Gizmos/",
        "/Documentation",
        "AssetBundles-Browser-master",
    };

    // 在结果里默认排除的扩展名（脚本/原生插件/shader include 等不能按资源删）
    static readonly HashSet<string> ExcludeExtensions = new HashSet<string>
    {
        ".cs", ".asmdef", ".asmref",
        ".dll", ".so", ".dylib", ".a", ".aar", ".jar", ".bundle",
        ".cginc", ".hlsl", ".compute",
        ".unitypackage",
        ".md", ".txt", ".pdf",
    };

    Vector2 _scroll;
    List<string> _unused = new List<string>();
    long _totalBytes;

    [MenuItem("Tools/Find Unused Assets In \"Asset Packs\"")]
    static void Open()
    {
        GetWindow<FindUnusedAssetsTool>("Unused Asset Finder");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("扫描根目录", ScanRoot);
        EditorGUILayout.LabelField("根入口", "BuildSettings 场景 + 所有 Resources/ 下资源");
        EditorGUILayout.Space();

        if (GUILayout.Button("Scan", GUILayout.Height(28)))
        {
            Scan();
        }

        if (_unused.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"未引用资源: {_unused.Count} 个   合计 {FormatBytes(_totalBytes)}");

        if (GUILayout.Button($"Export Report  →  {ReportPath}"))
        {
            ExportReport();
        }

        EditorGUILayout.HelpBox(
            "下面列表点击可在 Project 中定位。删除前请用 Right-Click → Find References In Project 再次确认。",
            MessageType.Info);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var p in _unused)
        {
            if (GUILayout.Button(p, EditorStyles.linkLabel))
            {
                var obj = AssetDatabase.LoadMainAssetAtPath(p);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void Scan()
    {
        EditorUtility.DisplayProgressBar("Scanning", "Collecting roots...", 0f);
        try
        {
            // 1) 收集根入口
            var roots = new List<string>();

            // 1a) BuildSettings 中已勾选的场景
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled && !string.IsNullOrEmpty(s.path))
                    roots.Add(s.path);
            }

            // 1b) 所有 Resources/ 文件夹下的资源（运行时通过字符串加载）
            var resourceAssets = AssetDatabase.FindAssets("", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.Contains("/Resources/"))
                .Distinct()
                .ToList();
            roots.AddRange(resourceAssets);

            EditorUtility.DisplayProgressBar("Scanning", "Resolving dependencies...", 0.2f);

            // 2) 递归收集所有依赖
            var used = new HashSet<string>(
                AssetDatabase.GetDependencies(roots.ToArray(), recursive: true),
                System.StringComparer.OrdinalIgnoreCase);

            EditorUtility.DisplayProgressBar("Scanning", "Listing Asset Packs files...", 0.6f);

            // 3) 列出 Asset Packs 下所有非目录资源
            if (!AssetDatabase.IsValidFolder(ScanRoot))
            {
                EditorUtility.DisplayDialog("Error", $"目录不存在: {ScanRoot}", "OK");
                return;
            }

            var allInPack = AssetDatabase.FindAssets("", new[] { ScanRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !AssetDatabase.IsValidFolder(p))
                .Distinct()
                .ToList();

            // 4) 过滤
            _unused.Clear();
            _totalBytes = 0;

            for (int i = 0; i < allInPack.Count; i++)
            {
                var path = allInPack[i];
                if (i % 200 == 0)
                {
                    EditorUtility.DisplayProgressBar(
                        "Scanning",
                        $"Filtering {i}/{allInPack.Count}",
                        0.6f + 0.4f * i / Mathf.Max(1, allInPack.Count));
                }

                if (used.Contains(path)) continue;

                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ExcludeExtensions.Contains(ext)) continue;

                var pathLower = path.Replace('\\', '/');
                bool excluded = false;
                foreach (var key in ExcludePathContains)
                {
                    if (pathLower.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        excluded = true;
                        break;
                    }
                }
                if (excluded) continue;

                _unused.Add(path);

                var fi = new FileInfo(path);
                if (fi.Exists) _totalBytes += fi.Length;
            }

            _unused.Sort();
            Debug.Log($"[FindUnusedAssets] 未引用资源 {_unused.Count} 个，合计 {FormatBytes(_totalBytes)}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    void ExportReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Unused assets report");
        sb.AppendLine($"# Scan root : {ScanRoot}");
        sb.AppendLine($"# Total     : {_unused.Count} files, {FormatBytes(_totalBytes)}");
        sb.AppendLine($"# Generated : {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // 按一级子目录分组，便于按 pack 整体决策
        var groups = _unused
            .GroupBy(p =>
            {
                var rel = p.Substring(ScanRoot.Length).TrimStart('/');
                var idx = rel.IndexOf('/');
                return idx > 0 ? rel.Substring(0, idx) : "(root)";
            })
            .OrderByDescending(g => g.Sum(f => SafeSize(f)));

        foreach (var g in groups)
        {
            long bytes = g.Sum(f => SafeSize(f));
            sb.AppendLine($"## [{FormatBytes(bytes),9}] {g.Key}   ({g.Count()} files)");
            foreach (var f in g.OrderBy(x => x))
                sb.AppendLine($"  {f}");
            sb.AppendLine();
        }

        File.WriteAllText(ReportPath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[FindUnusedAssets] 报告已写入: {Path.GetFullPath(ReportPath)}");
        EditorUtility.RevealInFinder(Path.GetFullPath(ReportPath));
    }

    static long SafeSize(string path)
    {
        try { return new FileInfo(path).Length; }
        catch { return 0; }
    }

    static string FormatBytes(long b)
    {
        if (b < 1024) return $"{b} B";
        if (b < 1024 * 1024) return $"{b / 1024f:F1} KB";
        if (b < 1024L * 1024 * 1024) return $"{b / 1024f / 1024f:F1} MB";
        return $"{b / 1024f / 1024f / 1024f:F2} GB";
    }
}
