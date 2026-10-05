using System;
using Core.BuffSystem;

public static class GameEventBus
{
    public static event Action<ulong, int, int, MicroBuffData> OnMatchSuccess;
    public static event Action<ulong, ItemData> OnItemUsed;
    public static event Action<ulong, int> OnDamageTaken;
    public static event Action<ulong> OnShieldBroken;
    public static event Action<ulong> OnManaFull;
    public static event Action<ulong, ulong> OnPlayerDied;

    public static void TriggerMatchSuccess(ulong playerId, int cellValue1, int cellValue2, MicroBuffData recipe)
    {
        OnMatchSuccess?.Invoke(playerId, cellValue1, cellValue2, recipe);
    }

    public static void TriggerItemUsed(ulong playerId, ItemData item)
    {
        OnItemUsed?.Invoke(playerId, item);
    }

    public static void TriggerDamageTaken(ulong playerId, int actualDamage)
    {
        OnDamageTaken?.Invoke(playerId, actualDamage);
    }

    public static void TriggerShieldBroken(ulong playerId)
    {
        OnShieldBroken?.Invoke(playerId);
    }

    public static void TriggerManaFull(ulong playerId)
    {
        OnManaFull?.Invoke(playerId);
    }

    public static void TriggerPlayerDied(ulong deadPlayerId, ulong killerPlayerId)
    {
        OnPlayerDied?.Invoke(deadPlayerId, killerPlayerId);
    }

    public static void ClearAll()
    {
        OnMatchSuccess = null;
        OnItemUsed = null;
        OnDamageTaken = null;
        OnShieldBroken = null;
        OnManaFull = null;
        OnPlayerDied = null;
    }
}
