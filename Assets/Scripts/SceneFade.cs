using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public sealed class SceneFade : MonoBehaviour
{
    [SerializeField, Min(0f)] private float duration = 0.45f;

    private static SceneFade current;
    private CanvasGroup overlay;
    private bool busy;
    private string reportedMissingScene;

    public static bool Busy => current != null && current.busy;

    private void Awake()
    {
        current = this;
        overlay = GetComponent<CanvasGroup>();
        overlay.alpha = 1f;
        busy = true;
    }

    private IEnumerator Start()
    {
        yield return FadeTo(0f);
        busy = false;
    }

    public void GoTo(string scenePath)
    {
        if (busy)
            return;

        int index = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (index >= 0)
        {
            busy = true;
            StartCoroutine(LoadAfterFade(() => SceneManager.LoadSceneAsync(index)));
            return;
        }

#if UNITY_EDITOR
        // O Play Mode pode manter a lista de cenas de antes da última alteração.
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath) != null)
        {
            busy = true;
            StartCoroutine(LoadAfterFade(() =>
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    scenePath, new LoadSceneParameters(LoadSceneMode.Single))));
            return;
        }
#endif

        if (reportedMissingScene == scenePath)
            return;

        reportedMissingScene = scenePath;
        Debug.LogError($"Cena fora da lista do build: {scenePath}", this);
    }

    private IEnumerator LoadAfterFade(System.Func<AsyncOperation> loadScene)
    {
        // O fade usa tempo real mesmo com o jogo congelado.
        Time.timeScale = 0f;
        yield return FadeTo(1f);
        yield return loadScene();
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float initial = overlay.alpha;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            overlay.alpha = Mathf.Lerp(initial, targetAlpha, elapsed / duration);
            yield return null;
        }
        overlay.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (current == this)
            current = null;
    }
}
