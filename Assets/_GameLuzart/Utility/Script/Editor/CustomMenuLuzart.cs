#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomMenuLuzart : EditorWindow
{
    [MenuItem("Luzart/LuzartTool/Remove Missing Scripts")]
    public static void Remove()
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = objs.Sum(GameObjectUtility.RemoveMonoBehavioursWithMissingScript);
        foreach (var obj in objs)
        {
            EditorUtility.SetDirty(obj);
        }
        Debug.Log($"Removed {count} missing scripts");
    }

    //[MenuItem("Luzart/Game")]
    //public static void Game()
    //{
    //    // Tên của scene bạn muốn chuyển đến
    //    string sceneName = "Game";

    //    // Kiểm tra xem scene có tồn tại trong Build Settings hay không
    //    if (IsSceneInBuildSettings(sceneName))
    //    {
    //        // Chuyển scene
    //        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(GetBuildIndex(sceneName)));
    //    }
    //    else
    //    {
    //        AddSceneToBuildSettings(sceneName);
    //        // Chuyển scene
    //        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(GetBuildIndex(sceneName)));
    //    }

    //}
    //[MenuItem("Luzart/Gameplay")]
    //public static void Gameplay()
    //{
    //    // Tên của scene bạn muốn chuyển đến
    //    string sceneName = "GamePlay";

    //    // Kiểm tra xem scene có tồn tại trong Build Settings hay không
    //    if (IsSceneInBuildSettings(sceneName))
    //    {
    //        // Chuyển scene
    //        EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(GetBuildIndex(sceneName)));
    //    }

    //}
    // Kiểm tra xem scene có tồn tại trong Build Settings hay không
    static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneFileName == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    static int GetBuildIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneFileName == sceneName)
            {
                return i;
            }
        }
        return -1;
    }

    // Thêm scene vào Build Settings
    static void AddSceneToBuildSettings(string sceneName)
    {
        // Lấy đường dẫn của scene
        string scenePath = "Assets/_GameLuzart/Scenes/" + sceneName + ".unity"; // Đường dẫn của scene trong thư mục Assets

        // Tạo một danh sách mới với tất cả các scene hiện tại trong Build Settings
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Tạo một scene mới và đặt nó là enabled
        EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scenePath, true);
        scenes.Insert(0, newScene);

        // Cập nhật Build Settings với danh sách mới
        EditorBuildSettings.scenes = scenes.ToArray();

        Debug.Log("Scene " + sceneName + " đã được thêm vào Build Settings.");
    }

    [MenuItem("Luzart/Play")]
    public static void Play()
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = true;
    }
}
public static class DynamicMenuGenerator
{
    private const string MenuScriptPath = "Assets/_GameLuzart/Script/Utility/Editor/GeneratedDynamicMenu.cs";

    [MenuItem("Luzart/LuzartTool/Generate Scene Menus")]
    public static void GenerateSceneMenus()
    {
        var scenes = EditorBuildSettings.scenes;

        // Kiểm tra nếu không có scene nào trong Build Settings
        if (scenes.Length == 0)
        {
            Debug.LogWarning("Không có scene nào trong Build Settings.");
            return;
        }

        // Bắt đầu tạo nội dung script
        string scriptContent = "using UnityEditor;\nusing UnityEditor.SceneManagement;\n\n";
        scriptContent += "namespace Luzart\n{\n";
        scriptContent += "    public static class GeneratedDynamicMenu\n    {\n";

        for (int i = 0; i < scenes.Length; i++)
        {
            var scene = scenes[i];
            if (!scene.enabled) continue; // Bỏ qua scene không được tick

            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            scriptContent += $@"
        [MenuItem(""Luzart/_Scenes/{sceneName}"")]
        public static void OpenScene_{i}()
        {{
            EditorSceneManager.OpenScene(@""{scene.path.Replace("\\", "/")}"");
        }}
";
        }

        scriptContent += "    }\n}\n";

        if (File.Exists(MenuScriptPath))
        {
            // Đọc nội dung file cũ và kiểm tra xem có cần thêm hoặc cập nhật gì không
            string existingContent = File.ReadAllText(MenuScriptPath);
            if (existingContent != scriptContent)
            {
                // Ghi lại nội dung mới nếu có sự thay đổi
                File.WriteAllText(MenuScriptPath, scriptContent);
                Debug.Log("File script đã được cập nhật.");
            }
            else
            {
                Debug.Log("File script đã có nội dung giống nhau, không cần cập nhật.");
            }
        }
        else
        {
            // Nếu chưa có file thì tạo mới
            Directory.CreateDirectory(Path.GetDirectoryName(MenuScriptPath) ?? string.Empty);
            File.WriteAllText(MenuScriptPath, scriptContent);
            Debug.Log("File script đã được tạo mới.");
        }

        // Import lại file script vừa tạo hoặc cập nhật
        AssetDatabase.Refresh();
    }
}



public class ScaleToOneAndKeepSize : Editor
{
    [MenuItem("Luzart/Reset Parent Scale & Keep Child Size")]
    public static void ResetScaleForSelected()
    {
        Transform[] selectedTransforms = Selection.transforms;

        if (selectedTransforms.Length == 0)
        {
            Debug.LogWarning("Please select at least one RectTransform in the hierarchy.");
            return;
        }

        foreach (Transform selected in selectedTransforms)
        {
            if (selected is RectTransform parentTransform)
            {
                ResetScaleForParent(parentTransform);
            }
        }
    }

    private static void ResetScaleForParent(RectTransform parentTransform)
    {
        Vector2 originalParentSize = parentTransform.rect.size;
        Vector3 originalParentScale = parentTransform.localScale;
        Vector3 originalParentAnchors = parentTransform.anchoredPosition3D;

        RectTransform[] childTransforms = parentTransform.GetComponentsInChildren<RectTransform>(true);
        Vector3[] originalChildSizeDeltas = new Vector3[childTransforms.Length];
        Vector2[] originalChildPositions = new Vector2[childTransforms.Length];
        TMP_Text[] txts = parentTransform.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < childTransforms.Length; i++)
        {
            var child = childTransforms[i];
            if (child != null)
            {
                originalChildSizeDeltas[i] = child.sizeDelta;
                originalChildPositions[i] = child.anchoredPosition;
            }
        }

        parentTransform.localScale = Vector3.one;

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] != null)
            {
                var child = childTransforms[i];

                child.sizeDelta = new Vector2(
                    originalChildSizeDeltas[i].x * originalParentScale.x,
                    originalChildSizeDeltas[i].y * originalParentScale.y
                );

                child.anchoredPosition = new Vector2(
                    originalChildPositions[i].x * originalParentScale.x,
                    originalChildPositions[i].y * originalParentScale.y
                );

                EditorUtility.SetDirty(childTransforms[i]);
            }
        }

        for (int i = 0; i < txts.Length; i++)
        {
            float size = txts[i].fontSize;
            size = size * originalParentScale.x;
            txts[i].fontSize = size;
        }

        parentTransform.anchoredPosition3D = originalParentAnchors;

        EditorUtility.SetDirty(parentTransform);
    }
}

public class NamespaceAdder : EditorWindow
{
    private string namespaceName = "Luzart";
    private string folderPath = "";

    [MenuItem("Luzart/LuzartTool/Add Namespace to Scripts")]
    public static void ShowWindow()
    {
        GetWindow<NamespaceAdder>("Namespace Adder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add Namespace to Scripts", EditorStyles.boldLabel);
        namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);

        GUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        if (GUILayout.Button("Browse"))
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                folderPath = selectedFolder;
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Add Namespace"))
        {
            AddNamespaceToScripts();
        }
    }

    private void AddNamespaceToScripts()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Folder path is empty. Please select a folder.");
            return;
        }

        string[] scriptFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

        foreach (var scriptPath in scriptFiles)
        {
            string[] lines = File.ReadAllLines(scriptPath);

            // Kiểm tra xem file đã có namespace hay chưa
            if (HasNamespace(lines))
            {
                Debug.Log($"Skipped: {scriptPath} (Namespace already exists)");
                continue;
            }

            // Thêm namespace nếu chưa có
            using (StreamWriter writer = new StreamWriter(scriptPath))
            {
                writer.WriteLine($"namespace {namespaceName}");
                writer.WriteLine("{");

                foreach (var line in lines)
                {
                    writer.WriteLine($"    {line}");
                }

                writer.WriteLine("}");
            }

            Debug.Log($"Namespace added to: {scriptPath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("Namespace addition complete.");
    }

    private bool HasNamespace(string[] lines)
    {
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("namespace"))
            {
                return true; // Đã có namespace
            }
        }
        return false;
    }
}
public class MissingScriptFinder : EditorWindow
{
    private List<string> missingScriptObjects = new List<string>();

    [MenuItem("Luzart/LuzartTool/Find Missing Scripts in Project")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptFinder>("Missing Script Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts"))
        {
            FindMissingScripts();
        }
        if (GUILayout.Button("Find Missing Scripts In Current"))
        {
            FindMissingScriptInCurrent();
        }

        GUILayout.Label("GameObjects with Missing Scripts:", EditorStyles.boldLabel);
        foreach (var obj in missingScriptObjects)
        {
            GUILayout.Label(obj);
        }
    }

    private void FindMissingScripts()
    {
        missingScriptObjects.Clear();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        FindMissing(allObjects);
        Debug.Log("Missing script search complete.");
    }
    private void FindMissingScriptInCurrent()
    {
        missingScriptObjects.Clear();
        // Kiểm tra có đang chỉnh sửa Prefab không
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            GetInPrefabs();

            return;
        }

        GetInScene();

        void GetInPrefabs()
        {
            GameObject root = prefabStage.prefabContentsRoot;
            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
            var allObjects = allTransforms.Select(x => x.gameObject).ToArray();
            FindMissing(allObjects);
            Debug.Log($"🟢 Bạn đang làm việc trong Prefabs: {prefabStage.name}");
            Debug.Log("Missing script search complete.");
        }

        void GetInScene()
        {
            // Nếu không trong Prefab Mode, kiểm tra Scene đang mở
            var activeScene = EditorSceneManager.GetActiveScene();
            GameObject[] allObjects = activeScene.GetRootGameObjects();
            FindMissing(allObjects);
            Debug.Log($"🟢 Bạn đang làm việc trong Scene: {activeScene.name}");
            Debug.Log("Missing script search complete.");

        }
    }

    private bool HasMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
            {
                return true;
            }
        }
        return false;
    }

    private string GetFullPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
    private void FindMissing(GameObject[] allObjects)
    {
        foreach (var obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
                continue;

            if (HasMissingScripts(obj))
            {
                missingScriptObjects.Add(GetFullPath(obj));
            }
        }
    }
}
#endif
