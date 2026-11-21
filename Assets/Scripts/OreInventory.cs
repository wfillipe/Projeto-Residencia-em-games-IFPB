using System;
using System.Collections.Generic;
using UnityEngine;

public static class OreInventory
{
    private static readonly int[] oresByTier = new int[3];
    private static readonly HashSet<string> minedDeposits = new();

    public static int Count { get; private set; }
    public static int PickaxeLevel { get; private set; } = 1;
    public static event Action<int> Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Changed = null;
        Reset();
    }

    public static int CountForTier(int tier) => oresByTier[tier - 1];

    public static bool WasMined(string depositId) =>
        !string.IsNullOrEmpty(depositId) && minedDeposits.Contains(depositId);

    public static void Collect(string depositId, int tier, int amount)
    {
        if (!string.IsNullOrEmpty(depositId) && !minedDeposits.Add(depositId))
            return;

        oresByTier[tier - 1] += amount;
        Count += amount;
        Changed?.Invoke(Count);
    }

    public static void Upgrade()
    {
        if (PickaxeLevel >= oresByTier.Length)
            return;

        SetPickaxeLevel(PickaxeLevel + 1);
    }

    internal static void SetPickaxeLevel(int level)
    {
        level = Mathf.Clamp(level, 1, oresByTier.Length);
        if (level == PickaxeLevel)
            return;

        PickaxeLevel = level;
        Changed?.Invoke(Count);
    }

    public static void Reset()
    {
        Array.Clear(oresByTier, 0, oresByTier.Length);
        minedDeposits.Clear();
        Count = 0;
        PickaxeLevel = GameManager.Settings?.StartingPickaxeLevel ?? 1;
        Changed?.Invoke(Count);
    }
}
