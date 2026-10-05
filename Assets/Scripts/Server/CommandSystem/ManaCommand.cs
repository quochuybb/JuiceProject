public class ManaCommand : ICommand
{
    private int amount;

    public ManaCommand(int mana)
    {
        amount = mana;
    }

    public void Execute(GameRoomContext context)
    {
        context.PlayerMana += amount;
        GameEventBus.TriggerManaFull(context.PlayerId);
    }
}
