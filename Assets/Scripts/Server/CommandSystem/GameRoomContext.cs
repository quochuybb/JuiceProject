using System.Collections.Generic;

public class GameRoomContext
{
    public ulong PlayerId;
    public ulong EnemyId;

    public int PlayerHP;
    public int PlayerShield;
    public int PlayerMana;

    public int EnemyHP;
    public int EnemyShield;
    public int EnemyMana;

    public List<CellData> PlayerBoard;
    public List<CellData> EnemyBoard;
}
