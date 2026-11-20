using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TopDownSetup
{
    private const string ScenePath = "Assets/Scenes/TopDown.unity";
    private const string PlayerPath = "Assets/Player/Player.prefab";

    [MenuItem("Milthrill/Criar cena Top Down")]
    public static void CreateStarterScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
            return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
        if (prefab == null)
        {
            Debug.LogError("Prefab do player não encontrado em " + PlayerPath);
            return;
        }

        var previousScene = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0, 0.45f, -10);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(45, 64, 65, 255);
            cameraObject.AddComponent<AudioListener>();
            var follow = new SerializedObject(cameraObject.AddComponent<PlayerCameraFollow>());
            follow.FindProperty("target").objectReferenceValue = player.transform;
            follow.ApplyModifiedPropertiesWithoutUndo();

            var obstacle = new GameObject("Barreira de teste");
            obstacle.transform.position = new Vector3(2, 0, 0);
            obstacle.transform.localScale = new Vector3(0.5f, 2f, 1f);
            var marker = obstacle.AddComponent<SpriteRenderer>();
            marker.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            marker.drawMode = SpriteDrawMode.Sliced;
            marker.size = Vector2.one;
            marker.color = new Color32(124, 151, 140, 255);
            marker.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            obstacle.AddComponent<BoxCollider2D>().size = Vector2.one;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Não foi possível salvar " + ScenePath);
        }
        finally
        {
            if (previousScene.IsValid())
                SceneManager.SetActiveScene(previousScene);
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
