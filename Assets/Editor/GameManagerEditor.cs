using UnityEditor;

[CustomEditor(typeof(GameManager))]
public sealed class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!EditorApplication.isPlaying)
            return;

        var manager = (GameManager)target;
        int currentLevel = EditorGUILayout.IntSlider("Nível atual (teste)", manager.CurrentPickaxeLevel, 1, 3);
        if (currentLevel != manager.CurrentPickaxeLevel)
            manager.SetCurrentPickaxeLevel(currentLevel);
    }

    public override bool RequiresConstantRepaint() => EditorApplication.isPlaying;
}
