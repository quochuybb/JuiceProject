public class TrueDamageCommand : ICommand
{
    private int amount;

    public TrueDamageCommand(int damage)
    {
        amount = damage;
    }

    public void Execute(GameRoomContext context)
    {
        context.EnemyHP -= amount;
        if (context.EnemyHP < 0) context.EnemyHP = 0;
        GameEventBus.TriggerDamageTaken(context.EnemyId, amount);
    }
}
