namespace Battleships.Game;

public class Player
{
    public List<BaseShip> Ships { get; init; }

    public Player(IEnumerable<BaseShip> ships)
    {
        Ships = ships.ToList();
        ObserveSubmerging();
    }

    private void ObserveSubmerging()
    {
        foreach (var ship in Ships)
        {
            ship.Submerged += OnShipSubmerge;
        }
    }

    public bool PlaceShip(string name, IEnumerable<Point> parts)
    {
        var ship = Ships.SingleOrDefault(ship => ship.Name == name);

        if (ship == null || ship.IsBuilt) return false;

        ship.Built(parts);

        return true;
    }
    public bool ShipsReady => Ships.All(s => s.IsBuilt);

    private void OnShipSubmerge(object? sender, EventArgs e)
    {
        if (sender is BaseShip ship)
            Ships.Remove(ship);
        if (Ships.Count == 0)
            Lost?.Invoke(this, e);
    }

    public event EventHandler? Lost;
}