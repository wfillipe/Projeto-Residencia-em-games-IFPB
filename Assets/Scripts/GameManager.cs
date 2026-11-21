using UnityEngine;

[CreateAssetMenu(fileName = "GameManager", menuName = "Milthrill/Game Manager")]
public sealed class GameManager : ScriptableObject
{
    [Header("Picareta ao iniciar ou reiniciar")]
    [SerializeField, Range(1, 3)] private int startingPickaxeLevel = 1;

    [Header("Minérios para desbloquear")]
    [SerializeField, Min(0)] private int oresForLevelTwo = 3;
    [SerializeField, Min(0)] private int oresForLevelThree = 3;

    [Header("Vida dos inimigos por nível da picareta")]
    [SerializeField, Min(1)] private int enemyHealthLevelOne = 3;
    [SerializeField, Min(1)] private int enemyHealthLevelTwo = 3;
    [SerializeField, Min(1)] private int enemyHealthLevelThree = 3;

    public static GameManager Settings => Resources.Load<GameManager>("GameManager");
    public int StartingPickaxeLevel => Mathf.Clamp(startingPickaxeLevel, 1, 3);
    public int CurrentPickaxeLevel => OreInventory.PickaxeLevel;

    public int OresForUpgrade(int currentLevel) =>
        Mathf.Max(0, currentLevel == 1 ? oresForLevelTwo : oresForLevelThree);

    public int EnemyHealth(int currentLevel) => Mathf.Max(1, currentLevel switch
    {
        1 => enemyHealthLevelOne,
        2 => enemyHealthLevelTwo,
        _ => enemyHealthLevelThree
    });

    public void SetCurrentPickaxeLevel(int level) => OreInventory.SetPickaxeLevel(level);
}
