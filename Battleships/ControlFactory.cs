using Battleships.Game;

namespace Battleships;

public static class ControlFactory
{
    public static ComboBox CreateLabeledComboBox(string label, Point location, object? data, Control parent, int width = 50)
    {
        var lb = new Label
        {
            AutoSize = false,
            Parent = parent,
            Name = $"ln_{label}",
            Text = label,
            Location = location,
            Width = width

        };

        return new ComboBox
        {
            Parent = parent,
            Name = $"cb_{label}",
            Top = location.Y + lb.Height,
            Left = location.X,
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList,
            MaxDropDownItems = 10,
            DataSource = data
        };
    }

    public static Button CreateButtonWithHandler(string text, Point location, Control parent,
        Action<object?, EventArgs> action)
    {
        var btn = new Button
        {
            Name = $"{parent}_{text.Replace(" ", "_")}_btn",
            Parent = parent,
            Text = text,
            Location = location
        };

        btn.Click += new EventHandler(action);

        return btn;
    }

    public static Button CreateBoatButton(Gameboard gameboard, Control parent, ComboBox col, ComboBox row, ComboBox dir, ComboBox ship)
    {
        var previewButton = new Button
        {
            Name = $"{parent}_preview_btn",
            Parent = parent,
            Text = "Place Boat",
            Location = new Point(350, 47)
        };

        previewButton.Click += (s, e) =>
        {
            if (s is not Button) return;

            var start = new Point(col.SelectedIndex + 1, row.SelectedIndex + 1);
            var direction = (Direction)dir.SelectedItem;
            var boxedShip = (ComboboxItem<BaseShip>)ship.SelectedItem;

            var highlight = Utils.GetPositions(start, direction, boxedShip.Value.Length);

            foreach (var point in highlight)
            {
                var control = gameboard.GetControlFromPosition(point.X, point.Y);

                if (control is not Button btn) return;

                btn.BackColor = Color.GreenYellow;
            }

            var result = MessageBox.Show("Do you wanna place your ship here?", "Are You Sure", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {

            }
        };

        return previewButton;
    }
}