namespace Core.BuffSystem
{
    public enum EffectType
    {
        Damage,
        TrueDamage,
        Heal,
        Shield,
        Mana,
        LockCell,
        ClearJunk
    }

    public enum EffectTarget
    {
        SelfPlayer,
        EnemyPlayer,
        SelfGrid,
        EnemyGrid
    }

    public enum ItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum CellState
    {
        Normal,
        Locked,
        Trapped,
        Frozen
    }
}
