namespace Battleships;
public sealed class Gameboard : TableLayoutPanel
{
    public bool Enumerated { get; init; } = true;

    public string CornerText
    {
        get => GetControlFromPosition(0, 0).Text;
        set => GetControlFromPosition(0, 0).Text = value;
    }

    public void Built()
    {
        ColumnStyles.Clear();
        RowStyles.Clear();
        Dock = DockStyle.Fill;
        InitializeBoard();
        FillBoard();
        CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
    }

    private void InitializeBoard()
    {
        // add one more row & col for enumeration
        if (Enumerated)
        {
            RowCount++;
            ColumnCount++;
        }

        for (var column = 0; column < ColumnCount; column++)
        {
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / ColumnCount));
        }
        for (var row = 0; row < RowCount; row++)
        {
            RowStyles.Add(new RowStyle(SizeType.Percent, 100f / RowCount));
        }

    }

    private void FillBoard()
    {
        this.SuspendLayout();
        for (var column = 0; column < ColumnCount; column++)
        {
            for (var row = 0; row < RowCount; row++)
            {
                if (Enumerated && (column == 0 || row == 0))
                {
                    var label = new Label
                    {
                        Name = $"{this.Name}_{column}_{row}",
                        Text = row == 0 ? $"{column}" : $"{Convert.ToChar('A' + row - 1)}",
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    if (Enumerated && (column == 0 && row == 0)) label.Text = "";
                    Controls.Add(label, column, row);
                    label.Dock = DockStyle.Fill;
                    continue;
                }

                var button = new Button
                {
                    Name = $"{this.Name}_{column}_{row}",
                    Margin = new Padding(0)
                };
                Controls.Add(button, column, row);
                button.Dock = DockStyle.Fill;
            }
        }
        this.ResumeLayout();
    }

    public void Foreach<TControl>(Action<TControl> action)
        where TControl : Control, new()
    {
        foreach (Control control in Controls)
        {
            if (control is TControl tControl)
            {
                action?.Invoke(tControl);
            }
        }
    }
    public void Clear(bool enabled = true)
    {
        Foreach<Button>(btn =>
        {
            btn.Enabled = enabled;
            btn.BackColor = default;
            btn.UseVisualStyleBackColor = true;
        });
    }

    public void UseMap(Dictionary<Point, Color> map)
    {
        foreach (var pixel in map)
        {
            var btn = (Button)GetControlFromPosition(pixel.Key.X, pixel.Key.Y);
            btn.BackColor = pixel.Value;
            btn.Enabled = true;
        }
    }

    public Dictionary<Point, Color> GetMap()
    {
        var map = new Dictionary<Point, Color>();
        Foreach<Button>(btn =>
        {
            if (btn.BackColor == default) return;

            var position = GetPositionFromControl(btn);

            map.Add(new Point(position.Column, position.Row), btn.BackColor);
        });

        return map;
    }

    public void SwitchCornerText(Gameboard other) => (CornerText, other.CornerText) = (other.CornerText, CornerText);

}