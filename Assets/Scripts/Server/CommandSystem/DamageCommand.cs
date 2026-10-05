public class DamageCommand : ICommand
{
    private int amount;

    public DamageCommand(int damage)
    {
        amount = damage;
    }

    public void Execute(GameRoomContext context)
    {
        int remaining = amount;

        if (context.EnemyShield > 0)
        {
            if (context.EnemyShield >= remaining)
            {
                context.EnemyShield -= remaining;
                remaining = 0;
            }
            else
            {
                remaining -= context.EnemyShield;
                context.EnemyShield = 0;
                GameEventBus.TriggerShieldBroken(context.EnemyId);
            }
        }

        if (remaining > 0)
        {
            context.EnemyHP -= remaining;
            if (context.EnemyHP < 0) context.EnemyHP = 0;
            GameEventBus.TriggerDamageTaken(context.EnemyId, remaining);
        }
    }
}
