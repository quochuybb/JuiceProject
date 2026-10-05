public class HealCommand : ICommand
{
    private int amount;

    public HealCommand(int heal)
    {
        amount = heal;
    }

    public void Execute(GameRoomContext context)
    {
        context.PlayerHP += amount;
    }
}
