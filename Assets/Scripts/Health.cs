using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Health : MonoBehaviour, IPickaxeDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 5;

    [SerializeField] private bool useGameManagerEnemyHealth;

    public int Current { get; private set; }
    public int Max => Mathf.Max(1, maxHealth);
    public bool IsAlive => Current > 0;
    public event Action Damaged;
    public event Action<Vector2?> Hit;

    private void Awake()
    {
        if (useGameManagerEnemyHealth && GameManager.Settings is GameManager manager)
            maxHealth = manager.EnemyHealth(OreInventory.PickaxeLevel);

        Current = Max;
    }

    public void TakeDamage(int amount, Vector2? source = null)
    {
        if (amount <= 0 || !IsAlive)
            return;

        Current = Mathf.Max(0, Current - amount);
        Hit?.Invoke(source);
        Damaged?.Invoke();
    }

    void IPickaxeDamageable.TakePickaxeDamage(int damage) => TakeDamage(damage);
}
