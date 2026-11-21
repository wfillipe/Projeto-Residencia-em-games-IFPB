using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class GameMenus : MonoBehaviour
{
    private static readonly int Dead = Animator.StringToHash("Dead");

    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject pauseSelection;
    [SerializeField] private GameObject deathSelection;

    [Header("Desfecho da arena")]
    [SerializeField] private Health bossHealth;
    [SerializeField] private CanvasGroup endingPanel;
    [SerializeField] private Text endingMessage;
    [SerializeField] private Text deathTitle;
    [SerializeField, Min(0f)] private float endingFadeDuration = 0.9f;
    [SerializeField, Min(0f)] private float endingHoldDuration = 2f;

    private Animator playerAnimator;
    private bool ending;
    private bool dying;
    private bool changingScene;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (playerHealth != null)
            playerAnimator = playerHealth.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.Damaged += CheckDeath;
        if (bossHealth != null)
            bossHealth.Damaged += CheckBossDefeat;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Damaged -= CheckDeath;
        if (bossHealth != null)
            bossHealth.Damaged -= CheckBossDefeat;
    }

    private void Update()
    {
        if (changingScene || ending || SceneFade.Busy)
            return;

        if (dying)
        {
            // Deixa a animação terminar antes de congelar a partida.
            if (!deathPanel.activeSelf && DeathAnimationFinished())
            {
                if (endingPanel != null)
                    StartCoroutine(ShowArenaEnding(false));
                else
                    ShowDeathPanel();
            }
            return;
        }

        if (pausePanel != null && (Keyboard.current?.escapeKey.wasPressedThisFrame == true ||
                                   Gamepad.current?.startButton.wasPressedThisFrame == true))
        {
            if (pausePanel.activeSelf)
                Resume();
            else
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f;
                Select(pauseSelection);
            }
        }
    }

    private void CheckDeath()
    {
        if (playerHealth.IsAlive || dying || ending)
            return;

        dying = true;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
        if (playerAnimator != null)
            playerAnimator.SetBool(Dead, true);
    }

    private void CheckBossDefeat()
    {
        if (!bossHealth.IsAlive && !dying && !ending && playerHealth.IsAlive)
            StartCoroutine(ShowArenaEnding(true));
    }

    private IEnumerator ShowArenaEnding(bool survived)
    {
        ending = true;
        if (survived)
            yield return new WaitForSeconds(0.6f);

        Time.timeScale = 0f;
        endingMessage.text = survived ? "Você sobreviveu, por enquanto..." : "Continua...";
        endingPanel.alpha = 0f;
        endingPanel.gameObject.SetActive(true);
        for (float elapsed = 0f; elapsed < endingFadeDuration; elapsed += Time.unscaledDeltaTime)
        {
            endingPanel.alpha = elapsed / endingFadeDuration;
            yield return null;
        }

        endingPanel.alpha = 1f;
        yield return new WaitForSecondsRealtime(endingHoldDuration);
        endingPanel.gameObject.SetActive(false);
        if (survived && deathTitle != null)
            deathTitle.text = "Você sobreviveu";
        ShowDeathPanel();
    }

    private void ShowDeathPanel()
    {
        Time.timeScale = 0f;
        deathPanel.SetActive(true);
        Select(deathSelection);
    }

    private bool DeathAnimationFinished()
    {
        if (playerAnimator == null || !playerAnimator.isActiveAndEnabled)
            return true;

        AnimatorStateInfo state = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return state.IsTag("Death") && state.normalizedTime >= 1f;
    }

    public void Resume()
    {
        if (dying || ending || changingScene || pausePanel == null)
            return;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Select(null);
    }

    public void Play() => Load("Assets/Scenes/Primeira cena.unity");
    public void Restart() => Load(SceneManager.GetActiveScene().path, resetProgress: bossHealth == null);
    public void MainMenu() => Load("Assets/Scenes/MainMenu.unity");

    private void Load(string scenePath, bool resetProgress = true)
    {
        if (changingScene)
            return;

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        if (sceneIndex < 0)
        {
            Debug.LogError($"A cena {scenePath} precisa estar na lista de cenas do build.", this);
            return;
        }

        changingScene = true;
        if (resetProgress)
            OreInventory.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    private static void Select(GameObject button)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button);
    }
}
