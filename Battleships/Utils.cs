namespace Battleships;
using Game;
public static class Utils
{
    public static IEnumerable<char> EnumerateAlphabet(int stop)
    {
        var stopSymbol = Convert.ToChar('A' + stop - 1);
        for (char c = 'A'; c <= stopSymbol; c++)
        {
            yield return c;
        }
    }

    public static IEnumerable<string> EnumerateShips(GameSettings settings)
    {
        return settings.ShipsConfiguration.Select(ships => ships.Name);
    }

    public static void RemoveAllControls(Control control)
    {
        control.Controls.Clear();
    }

    public static IEnumerable<Point> GetPositions(Point start, Direction direction, int length)
    {
        var result = new List<Point>();
        switch (direction)
        {
            case Direction.North:
                for (var i = start.Y; i > start.Y - length; i--)
                {
                    if (i <= 0) break;
                    result.Add(new Point(start.X, i));
                }
                break;
            case Direction.East:
                for (var i = start.X; i < start.X + length; i++)
                {
                    if (i > 10) break;
                    result.Add(new Point(i, start.Y));
                }
                break;
            case Direction.South:
                for (var i = start.Y; i < start.Y + length; i++)
                {
                    if (i > 10) break;
                    result.Add(new Point(start.X, i));
                }
                break;
            case Direction.West:
                for (var i = start.X; i > start.X - length; i--)
                {
                    if (i <= 0) break;
                    result.Add(new Point(i, start.Y));
                }
                break;
        }
        return result;
    }

}

public class ComboboxItem<T>
{
    public ComboboxItem(string text, T value)
    {
        Text = text;
        Value = value;
    }

    public string Text { get; set; }
    public T Value { get; set; }

    public override string ToString()
    {
        return Text;
    }

}

public enum Direction
{
    North,
    East,
    South,
    West
}