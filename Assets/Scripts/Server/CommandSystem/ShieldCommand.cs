public class ShieldCommand : ICommand
{
    private int amount;

    public ShieldCommand(int shield)
    {
        amount = shield;
    }

    public void Execute(GameRoomContext context)
    {
        context.PlayerShield += amount;
    }
}
