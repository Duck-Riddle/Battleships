namespace Battleships.Game;

public class GameSettings
{
    public IList<BaseShip> ShipsConfiguration => new List<BaseShip>
    {
        new Battleship(5, "Carrier"),
        new Battleship(4, "Battleship"),
        new Battleship(3, "Cruiser"),
        new Battleship(3, "Submarine"),
        new Battleship(2, "Destroyer")
    };
}