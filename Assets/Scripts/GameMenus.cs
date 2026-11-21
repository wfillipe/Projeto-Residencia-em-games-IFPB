using UnityEngine;
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

    private Animator playerAnimator;
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
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Damaged -= CheckDeath;
    }

    private void Update()
    {
        if (changingScene || SceneFade.Busy)
            return;

        if (dying)
        {
            // Deixa a animação terminar antes de congelar a partida.
            if (!deathPanel.activeSelf && DeathAnimationFinished())
            {
                deathPanel.SetActive(true);
                Time.timeScale = 0f;
                Select(deathSelection);
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
        if (playerHealth.IsAlive || dying)
            return;

        dying = true;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
        if (playerAnimator != null)
            playerAnimator.SetBool(Dead, true);
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
        if (dying || changingScene || pausePanel == null)
            return;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Select(null);
    }

    public void Play() => Load("Assets/Scenes/Primeira cena.unity");
    public void Restart() => Load(SceneManager.GetActiveScene().path);
    public void MainMenu() => Load("Assets/Scenes/MainMenu.unity");

    private void Load(string scenePath)
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
