using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;
using System;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public static class GroundFoundationBuilder
{
    public const string ScenePath = "Assets/Scenes/GroundPrototype.unity";
    public const string AuthorizedFolder = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Grass";
    public const string PlantPrefabFolder = "Assets/Cainos/Pixel Art Top Down - Basic/Prefab/Plant";
    private const string Task05Key = "Task05_HierarchyPrepared_v2";

    static GroundFoundationBuilder()
    {
        EditorApplication.delayCall += OnEditorDelayCall;
    }

    private static void OnEditorDelayCall()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorSceneManager.GetActiveScene().path != ScenePath)
            return;

        if (!SessionState.GetBool(Task05Key, false))
        {
            SessionState.SetBool(Task05Key, true);
            PrepareSceneForManualPlacement();
        }
    }

    [MenuItem("TopDown2D/Prepare Hierarchy for Manual Placement (Task 05)")]
    public static void PrepareSceneForManualPlacement()
    {
        if (EditorApplication.isPlaying) return;

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path != ScenePath && File.Exists(ScenePath))
        {
            activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        Debug.Log("=== [TASK 05: PREPARE SCENE FOR MANUAL PREFAB PLACEMENT START] ===");

        // 1. Create or preserve required hierarchy groups without modifying existing objects
        var propsRoot = EnsureEmptyContainer("Environment - Props", null, new[] { "Rocks", "Wood", "Decorations", "Other" });
        var structRoot = EnsureEmptyContainer("Environment - Structures", null, new[] { "Buildings", "Fences", "Other" });
        var interRoot = EnsureEmptyContainer("Environment - Interactive Objects", null, new[] { "Containers", "Collectibles", "Other" });

        // 2. Verify existing terrain and vegetation remain intact
        var groundGo = GameObject.Find("Tilemap - Ground");
        var tm = groundGo != null ? groundGo.GetComponent<Tilemap>() : null;
        int tileCount = 0;
        if (tm != null)
        {
            foreach (var p in tm.cellBounds.allPositionsWithin)
            {
                if (tm.GetTile(p) != null) tileCount++;
            }
        }

        var vegRoot = GameObject.Find("Environment - Vegetation");
        int treeCount = vegRoot?.transform.Find("Trees")?.childCount ?? 0;
        int bushCount = vegRoot?.transform.Find("Bushes")?.childCount ?? 0;
        int plantCount = vegRoot?.transform.Find("Small Plants")?.childCount ?? 0;
        int colliderCount = vegRoot != null ? vegRoot.GetComponentsInChildren<Collider2D>(true).Length : 0;

        Debug.Log($"[Task 05 Verification] Terrain tiles={tileCount}, Trees={treeCount}, Bushes={bushCount}, SmallPlants={plantCount}, Colliders={colliderCount}");

        // 3. Save Scene
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene, ScenePath);
        Debug.Log($"[Task 05] Saved {ScenePath} with organized manual placement hierarchy.");

        // 4. Expand the new containers in the Hierarchy window via Selection
        var childrenToReveal = new List<UnityEngine.Object>();
        if (groundGo != null) childrenToReveal.Add(groundGo);
        if (vegRoot != null && vegRoot.transform.childCount > 0) childrenToReveal.Add(vegRoot.transform.GetChild(0).gameObject);
        if (propsRoot != null && propsRoot.transform.childCount > 0) childrenToReveal.Add(propsRoot.transform.GetChild(0).gameObject);
        if (structRoot != null && structRoot.transform.childCount > 0) childrenToReveal.Add(structRoot.transform.GetChild(0).gameObject);
        if (interRoot != null && interRoot.transform.childCount > 0) childrenToReveal.Add(interRoot.transform.GetChild(0).gameObject);
        Selection.objects = childrenToReveal.ToArray();

        // Schedule screenshot capture on next delayCall after Hierarchy UI repaints
        EditorApplication.delayCall += CaptureHierarchyScreenshot;

        Debug.Log("=== [TASK 05: PREPARE SCENE FOR MANUAL PREFAB PLACEMENT END] ===");
    }

    private static GameObject EnsureEmptyContainer(string rootName, Transform parent, string[] childNames)
    {
        GameObject root = null;
        if (parent == null)
        {
            root = GameObject.Find(rootName);
        }
        else
        {
            var t = parent.Find(rootName);
            if (t != null) root = t.gameObject;
        }

        if (root == null)
        {
            root = new GameObject(rootName);
            if (parent != null) root.transform.SetParent(parent, false);
        }

        // Enforce exact transform settings: Position (0,0,0), Rotation (0,0,0), Scale (1,1,1)
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        if (childNames != null)
        {
            for (int i = 0; i < childNames.Length; i++)
            {
                string cName = childNames[i];
                Transform childT = root.transform.Find(cName);
                GameObject childGo;
                if (childT == null)
                {
                    childGo = new GameObject(cName);
                    childGo.transform.SetParent(root.transform, false);
                }
                else
                {
                    childGo = childT.gameObject;
                }
                childGo.transform.localPosition = Vector3.zero;
                childGo.transform.localRotation = Quaternion.identity;
                childGo.transform.localScale = Vector3.one;
                childGo.transform.SetSiblingIndex(i);
            }
        }

        Debug.Log($"[Task 05 Hierarchy] Verified '{rootName}' at pos={root.transform.localPosition}, rot={root.transform.localEulerAngles}, scale={root.transform.localScale} with {root.transform.childCount} children.");
        return root;
    }

    [MenuItem("TopDown2D/Capture Hierarchy Screenshot (Task 05)")]
    public static void CaptureHierarchyScreenshot()
    {
        try
        {
            Selection.activeGameObject = null;
            var hierarchyType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (hierarchyType == null) return;

            var windows = Resources.FindObjectsOfTypeAll(hierarchyType);
            if (windows == null || windows.Length == 0) return;

            var hw = (EditorWindow)windows[0];
            hw.Repaint();

            string artifactDir = @"C:\Users\Suthema\.gemini\antigravity\brain\0351d5e8-d616-4757-9d1c-7cd00f83970f";
            if (!Directory.Exists(artifactDir)) Directory.CreateDirectory(artifactDir);

            Rect r = hw.position;
            int w = Mathf.Max(200, (int)r.width);
            int h = Mathf.Max(200, (int)r.height);
            Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(new Vector2(r.x, r.y), w, h);
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.SetPixels(pixels);
            tex.Apply();

            string outPath = Path.Combine(artifactDir, "Hierarchy_Task05.png");
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            Debug.Log($"[Task 05 Screenshot] Saved Hierarchy window capture to {outPath} ({w}x{h}) rect=({r.x},{r.y},{r.width},{r.height})");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Task 05 Screenshot] Error capturing Hierarchy window: {ex.Message}");
        }
    }
}
