namespace Battleships.Game;

public class TurnEndEventArgs : EventArgs
{
    public bool PlayerBefore { get; set; }
    public bool PlayerNow { get; set; }
}
public class BattleshipsManager
{
    private readonly Player[] _players;

    private bool _canShoot = true;
    private GameSettings Settings { get; }
    public bool CurrentPlayer { get; set; }

    public BattleshipsManager(GameSettings settings)
    {
        Settings = settings;
        _players = new[]
        {
            new Player(Settings.ShipsConfiguration),
            new Player(Settings.ShipsConfiguration)
        };
        AttachImpactEventToShips((s, e) => _canShoot = true);
    }

    public void PlaceShip(string name, IEnumerable<Point> parts)
    {
        var player = _players[IndexOf(CurrentPlayer)];
        player.PlaceShip(name, parts);
        if (player.ShipsReady) EndTurn();
    }
    public void TakeShot(Point target)
    {
        _canShoot = false; // not anymore
        foreach (var ship in _players[IndexOf(NextPlayer)].Ships.ToList())
        {
            ship.EvaluateShot(target); // if something was hit than _canShoot = true
        }
        if (!_canShoot) EndTurn();
    }


    public void AttachImpactEventToShips(Action<object?, ImpactEventArgs> action)
    {
        foreach (var player in _players)
        {
            foreach (var ship in player.Ships)
            {
                ship.Impact += new EventHandler<ImpactEventArgs>(action);
            }
        }
    }

    public void AttachToLostEvent(Action<object?, EventArgs> action)
    {
        foreach (var player in _players)
        {
            player.Lost += new EventHandler(action);
        }
    }

    // This could be omitted but than client would need to keep track of used ships. 
    public IEnumerable<BaseShip> GetShips(bool? mode = null)
    {
        return mode switch
        {
            false => _players[IndexOf(CurrentPlayer)].Ships.Where(ship => ship.IsBuilt == false),
            true => _players[IndexOf(CurrentPlayer)].Ships.Where(ship => ship.IsBuilt == true),
            null => _players[IndexOf(CurrentPlayer)].Ships,
        };
    }

    private bool NextPlayer => !CurrentPlayer;

    public static int IndexOf(bool player) => player ? 1 : 0;
    private void EndTurn()
    {
        CurrentPlayer = NextPlayer;
        TurnEnd?.Invoke(this, new TurnEndEventArgs
        {
            PlayerBefore = NextPlayer,
            PlayerNow = CurrentPlayer
        });
        _canShoot = true;
    }

    public event EventHandler<TurnEndEventArgs>? TurnEnd;
}