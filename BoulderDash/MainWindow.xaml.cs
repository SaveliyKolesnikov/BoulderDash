using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BoulderDashGUI.Model;
using BoulderDashGUI.Model.GameObjects;
using Window = System.Windows.Window;

namespace BoulderDashGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly Game _game;
        public string DiamondsToNextLvl => _game.DiamondsToNextLvl.ToString();
        public string PlayerScore => Player.Instance.Score.ToString();

        public MainWindow()
        {
            InitializeComponent();

            _game = new Game(GameFieldCanvas);
            _game.Start();

            GameFieldCanvas.SizeChanged += _game.WindowResize;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_game.IsOver.Key) return;

            switch (e.Key)
            {

                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    _game.MovePlayer(e.Key);
                    if (_game.IsScoreChanged)
                    {
                        OnPropertyChanged($"PlayerScore");
                        OnPropertyChanged($"DiamondsToNextLvl");
                        _game.IsScoreChanged = false;
                    }
                    break;
            }
        }
    }
}
