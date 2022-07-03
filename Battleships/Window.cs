namespace Battleships;

using Game;
using static ControlFactory;
public class Window : Form
{
    #region Declarations
    //game
    private GameSettings gameSettings;
    private BattleshipsManager gameManager;

    //controls with data...
    private ComboBox comboBoxRow;
    private ComboBox comboBoxCol;
    private ComboBox comboBoxDir;
    private ComboBox comboBoxShip;

    //dynamic data about game
    private Dictionary<Point, Color> previewMap1 = new();
    private Dictionary<Point, Color> previewMap2 = new();

    private Dictionary<Point, Color> battleMap1 = new();
    private Dictionary<Point, Color> battleMap2 = new();

    private Gameboard gameboard;
    private Gameboard boardPreview;

    private string PlayerNumber => $"{BattleshipsManager.IndexOf(gameManager.CurrentPlayer) + 1}";
    //some default values...
    private readonly Size formLarge = new Size(1280, 640);
    private readonly Size formSmall = new Size(720, 900);
    private readonly Font appFont = new Font(FontFamily.GenericMonospace, 16);
    private readonly Color cHit = Color.IndianRed;
    private readonly Color cInfo = Color.GreenYellow;
    private readonly Color cMiss = Color.CadetBlue;

    //some Const values...
    private const int BoardSize = 10;


    #endregion

    // Enable Double Buffering to stop flickering;
    // https://www.youtube.com/watch?v=HQ0UYIuzucI&ab_channel=C%23eLearning
    protected override CreateParams CreateParams
    {
        get
        {
            var baseHandler = base.CreateParams;
            baseHandler.ExStyle |= 0x02000000;
            return baseHandler;
        }
    }

    public Window()
    {
        InitGame();
        InitForm(formSmall);
        gameboard = InitGameBoard($"P{PlayerNumber}");
        RenderPlacingMenu(this);
        RenderGameboard(this, 180);
        gameboard.Foreach<Button>(btn => btn.Click += OnPreviewTileClick);
    }

    #region Common
    private void InitForm(Size size)
    {
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        AutoScaleDimensions = new SizeF(9F, 16F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = size;
        Name = "Window";
        Padding = new Padding(8);
        Text = "Battleships";
    }
    private Gameboard InitGameBoard(string cornerText = "")
    {
        var gameBoard = new Gameboard
        {
            RowCount = BoardSize,
            ColumnCount = BoardSize,
            AutoSize = true,

        };
        gameBoard.Built();
        gameBoard.CornerText = cornerText;
        return gameBoard;
    }

    private void InitGame()
    {
        gameSettings = new GameSettings();
        gameManager = new BattleshipsManager(gameSettings);
        gameManager.TurnEnd += OnPlayerPreparationEnd;
        gameManager.AttachToLostEvent(OnPlayerLose);
        gameManager.AttachImpactEventToShips(OnPlayerHit);
    }

    #endregion

    #region ShipPlacment
    private void RenderGameboard(Control parent, int offset)
    {
        var boardContainer = new GroupBox
        {
            Name = "preview",
            Text = "preview",
            Parent = parent,
            Width = parent.Width - 16,
            Height = parent.Width - 16,
            Left = 8
        };
        boardContainer.Top = offset;
        gameboard.Parent = boardContainer;
    }
    private void RenderPlacingMenu(Control Parent)
    {
        var mainLabel = new Label
        {
            Padding = new Padding(0, 16, 0, 0),
            Text = $"Player {BattleshipsManager.IndexOf(gameManager.CurrentPlayer) + 1}\nPrepare for battle!",
            Parent = Parent,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = appFont
        };
        mainLabel.Left = (mainLabel.Parent.Width - mainLabel.Width) / 2;

        var placeBoat = new GroupBox
        {
            Name = "placing_menu",
            Text = "Setup boat",
            Parent = Parent,
            Width = Parent.Width - 16,
            Left = 8
        };
        placeBoat.Top = mainLabel.Location.Y + mainLabel.Height;

        comboBoxRow = CreateLabeledComboBox("row", new Point(25, 25), Utils.EnumerateAlphabet(10).ToList(), placeBoat);

        comboBoxCol = CreateLabeledComboBox("col", new Point(80, 25), Enumerable.Range(1, 10).ToList().ToList(), placeBoat);

        comboBoxDir = CreateLabeledComboBox("dir", new Point(135, 25), new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West }, placeBoat, 80);

        comboBoxShip = CreateLabeledComboBox("ship", new Point(220, 25), gameManager.GetShips(false).Select(ship => new ComboboxItem<BaseShip>($"{ship.Name} ({ship.Length})", ship)).ToList(), placeBoat, 120);

        CreateButtonWithHandler("Place Boat", new Point(350, 47), placeBoat, OnShipPlace);
    }

    private void OnPreviewTileClick(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        var clicked = gameboard.GetCellPosition(btn);
        comboBoxRow.SelectedItem = comboBoxRow.Items[clicked.Row - 1];
        comboBoxCol.SelectedItem = comboBoxCol.Items[clicked.Column - 1];
    }
    private void OnShipPlace(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;

        var start = new Point(comboBoxCol.SelectedIndex + 1, comboBoxRow.SelectedIndex + 1);
        var direction = (Direction)comboBoxDir.SelectedItem;
        var boxedShip = (ComboboxItem<BaseShip>)comboBoxShip.SelectedItem;

        var highlight = Utils.GetPositions(start, direction, boxedShip.Value.Length);
        var backup = new List<Button>();

        foreach (var point in highlight)
        {
            var control = gameboard.GetControlFromPosition(point.X, point.Y);

            if (control is not Button btn) return;

            backup.Add(btn);

            btn.BackColor = cInfo;
        }

        var result = MessageBox.Show("Do you wanna place your ship here?", "Are You Sure?", MessageBoxButtons.YesNo);

        if (result == DialogResult.Yes)
        {
            gameManager.PlaceShip(boxedShip.Value.Name, highlight);
            comboBoxShip.DataSource = gameManager.GetShips(false)
                .Select(ship => new ComboboxItem<BaseShip>($"{ship.Name} ({ship.Length})", ship)).ToList();
        }
        else
        {
            foreach (var btn in backup)
            {
                btn.BackColor = default;
                btn.UseVisualStyleBackColor = true;
            }
        }
    }

    private void OnPlayerPreparationEnd(object? sender, TurnEndEventArgs e)
    {
        if (e.PlayerBefore)
        {
            previewMap2 = gameboard.GetMap();
        }
        else
        {
            previewMap1 = gameboard.GetMap();
        }

        Utils.RemoveAllControls(this);

        MessageBox.Show($"End of Player{BattleshipsManager.IndexOf(e.PlayerBefore) + 1} turn\nPlayer{PlayerNumber} are you ready?");


        if (e.PlayerNow)
        {
            gameboard = InitGameBoard($"P{PlayerNumber}");
            RenderPlacingMenu(this);
            RenderGameboard(this, 180);
            gameboard.Foreach<Button>(btn => btn.Click += OnPreviewTileClick);
        }

        if (e.PlayerNow) return;

        gameManager.TurnEnd -= OnPlayerPreparationEnd;
        gameManager.TurnEnd += OnPlayerMoveEnd;

        Size = formLarge;

        BuildBattleMode();
    }


    #endregion

    #region BattleMode

    private void BuildBattleMode()
    {
        ClientSize = formLarge;

        var splitter = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Parent = this,
            SplitterDistance = Width / 2,
            IsSplitterFixed = true,
        };

        gameboard = InitGameBoard("P2");
        boardPreview = InitGameBoard("P1");

        boardPreview.UseMap(previewMap1);
        boardPreview.Foreach<Button>(btn => btn.Enabled = false);


        gameboard.Parent = splitter.Panel1;
        boardPreview.Parent = splitter.Panel2;

        gameboard.Foreach<Button>(btn => btn.Click += OnPlayerMove);

    }

    private void OnPlayerMoveEnd(object? sender, TurnEndEventArgs e)
    {
        if (e.PlayerNow)
        {
            previewMap1 = boardPreview.GetMap();
            battleMap1 = gameboard.GetMap();
        }
        else
        {
            previewMap2 = boardPreview.GetMap();
            battleMap2 = gameboard.GetMap();
        }

        gameboard.Clear();
        boardPreview.Clear(false);
        gameboard.SwitchCornerText(boardPreview);

        MessageBox.Show(
            $"End of Player{BattleshipsManager.IndexOf(e.PlayerBefore) + 1} turn\nPlayer{PlayerNumber} are you ready?");

        if (e.PlayerNow)
        {
            boardPreview.UseMap(previewMap2);
            gameboard.UseMap(battleMap2);
        }
        else
        {
            boardPreview.UseMap(previewMap1);
            gameboard.UseMap(battleMap1);
        }

    }

    private void OnPlayerMove(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        var btnPosition = gameboard.GetCellPosition(btn);

        var target = new Point(btnPosition.Column, btnPosition.Row);

        btn.Enabled = false;
        btn.BackColor = cMiss;

        if (gameManager.CurrentPlayer)
        {
            previewMap1[target] = cMiss;
        }
        else
        {
            previewMap2[target] = cMiss;
        }
        gameManager.TakeShot(target);

    }

    private void OnPlayerHit(object? sender, ImpactEventArgs e)
    {
        var control = gameboard.GetControlFromPosition(e.Position.X, e.Position.Y);

        if (gameManager.CurrentPlayer)
        {
            previewMap1[e.Position] = cHit;
        }
        else
        {
            previewMap2[e.Position] = cHit;
        }

        control.BackColor = cHit;

    }

    private void OnPlayerLose(object? sender, EventArgs e)
    {
        var result = MessageBox.Show($"Congratulations Player{PlayerNumber}\n\tYou Won!", "Game over", MessageBoxButtons.RetryCancel);

        if (result == DialogResult.Cancel) Application.Exit();
        else
        {
            Utils.RemoveAllControls(this);

            gameManager.TurnEnd -= OnPlayerMoveEnd;


            //reset some values
            previewMap1 = new Dictionary<Point, Color>();
            previewMap2 = new Dictionary<Point, Color>();

            battleMap1 = new Dictionary<Point, Color>();
            battleMap2 = new Dictionary<Point, Color>();

            InitGame();
            ClientSize = formSmall;
            gameboard = InitGameBoard($"P{PlayerNumber}");
            RenderPlacingMenu(this);
            RenderGameboard(this, 180);
            gameboard.Foreach<Button>(btn => btn.Click += OnPreviewTileClick);
        }

    }

    #endregion

}

