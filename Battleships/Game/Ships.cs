namespace Battleships.Game;
public class ImpactEventArgs : EventArgs
{
    public Point Position { get; init; }
}

// Abstraction allows other developers to implement their concretion
public abstract class BaseShip
{
    public int Length { get; init; }
    public string Name { get; init; }
    public bool IsBuilt { get; protected set; }
    protected BaseShip(int len) => Parts = new List<Point>(len);

    protected IList<Point> Parts { get; set; }
    public virtual void EvaluateShot(Point point)
    {
        if (!Parts.Contains(point)) return;

        OnImpact(new ImpactEventArgs
        {
            Position = point
        });

        Parts.Remove(point);

        if (Parts.Count == 0)
            OnSubmerge(EventArgs.Empty);
    }

    protected abstract bool Validate(IEnumerable<Point> parts);

    public abstract void Built(IEnumerable<Point> parts);

    protected virtual void OnImpact(ImpactEventArgs e)
    {
        Impact?.Invoke(this, e);
    }

    protected virtual void OnSubmerge(EventArgs e)
    {
        Submerged?.Invoke(this, e);
    }

    public event EventHandler<ImpactEventArgs>? Impact;

    public event EventHandler? Submerged;
}

// approach here is to declare empty ships and "build" them later
public class Battleship : BaseShip
{
    public Battleship(int len, string name = "") : base(len)
    {
        Length = len;
        Name = name;
    }
    protected override bool Validate(IEnumerable<Point> parts)
    {
        return true; // Validation is Handled by client in the same project omitted for now
    }
    public override void Built(IEnumerable<Point> parts)
    {
        if (IsBuilt) return; // Ship Can't be build again

        var enumerable = parts.ToList();

        if (!Validate(enumerable)) throw new ArgumentException("Can't build ship with those parts..."); // in case some one wanna override just Validate

        Parts = new List<Point>(enumerable);
        IsBuilt = true;
    }
}
