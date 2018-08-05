using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BoulderDashGUI.Model.GameObjects;
using BoulderDashGUI.Model.Interfaces;
using static System.String;

namespace BoulderDashGUI.Model
{
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }

    class Game
    {
        public int DiamondsToNextLvl => _field.DiamondsToNextLvl;
        // Key = isGameOver, Value = win/lose
        public KeyValuePair<bool, bool> IsOver;
        private Monster[] _monsters;
        private readonly GameField _field;
        private readonly Player _player;
        private readonly double _cellSize;
        private readonly MediaPlayer _levelCompletedSound;
        private string _gameOverMessage;

        private readonly DispatcherTimer _moveMonstersLoop;
        private readonly DispatcherTimer _renderLoop;
        private readonly DispatcherTimer _rockfallLoop;


        public bool IsScoreChanged
        {
            get => _player.IsScoreChanged;
            set => _player.IsScoreChanged = value;
        }
        public Game(Canvas gameFieldCanvas)
        {
            _field = new GameField(gameFieldCanvas);
            _field.LoadNexLvl();
            _monsters = GetAllMonsters();

            _player = Player.Instance;
            _cellSize = _player.CellSize;

            IsOver = new KeyValuePair<bool, bool>(false, false);
            _gameOverMessage = string.Empty;

            _levelCompletedSound = new MediaPlayer();
            _levelCompletedSound.Open(new Uri($"{Directory.GetCurrentDirectory()}/Data/Sounds/completeLvl.wav", UriKind.Absolute));
            _levelCompletedSound.Volume = 100;
            _levelCompletedSound.MediaFailed += (o, args) =>
            {
                Trace.WriteLine("AudioFailed");
                Trace.WriteLine(args.ErrorException);
            };

            _renderLoop = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };
            _renderLoop.Tick += Render;

            _moveMonstersLoop = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _moveMonstersLoop.Tick += MoveMonsters;

            _rockfallLoop = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _rockfallLoop.Tick += RockFall;
        }

        public void Start()
        {
            _renderLoop.Start();
            _rockfallLoop.Start();
            _moveMonstersLoop.Start();
        }

        private Monster[] GetAllMonsters()
        {
            var res = new List<Monster>();
            for (var i = 0; i < _field.RowsCount; i++)
                for (var j = 0; j < _field.ColsCount; j++)
                    if (_field[i, j] is Monster monster)
                    {
                        monster.GameField = _field;
                        monster.SetCoordinates(i, j);
                        monster.Reset();
                        res.Add(monster);
                    }

            return res.ToArray();
        }

        private Direction ConvertKeyToDirection(Key direction)
        {
            switch (direction)
            {
                case Key.Up:
                    return Direction.Up;
                case Key.Right:
                    return Direction.Right;
                case Key.Down:
                    return Direction.Down;
                case Key.Left:
                    return Direction.Left;
            }

            throw new ArgumentException();
        }

        public void MovePlayer(Key direction)
        {
            var isAllRedrawNeeded = false;
            const int renderDistance = 6;
            _player.SetImageUsingDirection(ConvertKeyToDirection(direction));
            
            switch (direction)
            {
                case Key.Right:
                case Key.Left:
                {
                    int nextCol, farCol;

                    if (direction == Key.Right)
                    {
                        nextCol = _player.Col + 1;
                        farCol = _player.Col + 2;


                        if (_player.Col == _field.ColsCount - 2 || _field[_player.Row, nextCol] is Wall)
                            return;

                        if (_player.Col + renderDistance > _field.ColsCount)
                        {
                            if (_field.StartViewCol != _field.ColsCount - _field.NumOfViewCols)
                            {
                                _field.StartViewCol = _field.ColsCount - _field.NumOfViewCols;
                                isAllRedrawNeeded = true;
                            }
                        }
                        else if (_player.Col + renderDistance > _field.NumOfViewCols + _field.StartViewCol)
                        {
                            _field.StartViewCol++;
                            isAllRedrawNeeded = true;
                        }
                    }
                    else
                    {

                        nextCol = _player.Col - 1;
                        farCol = _player.Col - 2;

                        if (_player.Col == 1 || _field[_player.Row, nextCol] is Wall)
                            return;
                        if (_player.Col - renderDistance < 0)
                        {
                            if (_field.StartViewCol != 0)
                            {
                                _field.StartViewCol = 0;
                                isAllRedrawNeeded = true;
                            }
                        }
                        else if (_player.Col - renderDistance < _field.StartViewCol)
                        {
                            _field.StartViewCol--;
                            isAllRedrawNeeded = true;
                        }
                    }

                    if (_field[_player.Row, nextCol] is Monster)
                    {
                        LoseGameByMonster();
                        return;
                    }


                    var diamondCell = _field[_player.Row, nextCol] as Diamond;
                    if (_field[_player.Row, nextCol] is Sand || !(diamondCell is null))
                    {
                        if (!(diamondCell is null))
                        {
                            _player.Score += diamondCell.Value;
                            //diamondCell.Play();
                        }
                        _field[_player.Row, _player.Col] = new Empty();
                        _player.Col = nextCol;
                        _field[_player.Row, _player.Col] = _player;
                    }
                    else if (_field[_player.Row, nextCol] is Empty)
                    {
                        // Swap empty and player cells
                        _field[_player.Row, _player.Col] = _field[_player.Row, nextCol];
                        _player.Col = nextCol;
                        _field[_player.Row, _player.Col] = _player;
                    }
                    else if (_field[_player.Row, nextCol] is Stone && farCol >= 0 && farCol < _field.ColsCount &&
                             _field[_player.Row, farCol] is Empty)
                    {
                        var emptyCell = _field[_player.Row, farCol];
                        var stoneCell = _field[_player.Row, nextCol];

                        _field[_player.Row, _player.Col] = emptyCell;
                        _player.Col = nextCol;
                        _field[_player.Row, _player.Col] = _player;
                        _field[_player.Row, farCol] = stoneCell;
                    }
                    else if (_field[_player.Row, nextCol] is Exit)
                    {
                        if (_player.Score >= _field.DiamondsToNextLvl)
                            WinGame();
                    }

                    break;
                }
                case Key.Up:
                case Key.Down:
                {
                    int nextRow;
                    if (direction == Key.Up)
                    {
                        nextRow = _player.Row - 1;

                        if (_player.Row == 1 || _field[nextRow, _player.Col] is Wall || _field[nextRow, _player.Col] is Stone)
                            return;

                        if (_player.Row - renderDistance < 0)
                        {
                            if (_field.StartViewRow != 0)
                            {
                                _field.StartViewRow = 0;
                                isAllRedrawNeeded = true;
                            }
                        }
                        else if (_player.Row - renderDistance <= _field.StartViewRow)
                        {
                            _field.StartViewRow--;
                            isAllRedrawNeeded = true;
                        }
                    }
                    else
                    {
                        nextRow = _player.Row + 1;

                        if (_player.Row == _field.RowsCount - 2 || _field[nextRow, _player.Col] is Wall || _field[nextRow, _player.Col] is Stone)
                            return;
                        if (_player.Row + renderDistance >= _field.RowsCount)
                        {
                            if (_field.StartViewRow != _field.RowsCount - _field.NumOfViewRows)
                            {
                                _field.StartViewRow = _field.RowsCount - _field.NumOfViewRows;
                                isAllRedrawNeeded = true;
                            }
                        }
                        else if (_player.Row + renderDistance >= _field.NumOfViewRows + _field.StartViewRow)
                        {
                            _field.StartViewRow++;
                            isAllRedrawNeeded = true;
                        }
                    }

                    if (_field[nextRow, _player.Col] is Monster)
                    {
                        LoseGameByMonster();
                        return;
                    }

                    var diamondCell = _field[nextRow, _player.Col] as Diamond;
                    if (_field[nextRow, _player.Col] is Sand || !(diamondCell is null))
                    {
                        if (!(diamondCell is null))
                        {
                            _player.Score += diamondCell.Value;
                            //diamondCell.Play();
                        }
                        _field[_player.Row, _player.Col] = new Empty();
                        _player.Row = nextRow;
                        _field[_player.Row, _player.Col] = _player;
                    }
                    else if (_field[nextRow, _player.Col] is Empty)
                    {
                        _field[_player.Row, _player.Col] = _field[nextRow, _player.Col];
                        _player.Row = nextRow;
                        _field[_player.Row, _player.Col] = _player;
                    }
                    else if (_field[nextRow, _player.Col] is Exit)
                    {
                        if (_player.Score >= _field.DiamondsToNextLvl)
                            WinGame();
                    }

                    break;
                }
            }
            if (isAllRedrawNeeded)
                _field.RedrawInRange(_field.StartViewRow, _field.StartViewRow + _field.NumOfViewRows, _field.StartViewCol, _field.StartViewCol + _field.NumOfViewCols);

            IsGameOver();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RockFall(object sender, EventArgs eventArgs)
        {
            for (var i = _field.RowsCount - 2; i >= 1; i--)
            {
                for (var j = 1; j < _field.ColsCount - 1; j++)
                {
                    if (_field[i, j] is Stone currentStone)
                    {
                        var isStoneFalled = false;
                        if (currentStone.IsFalling)
                        {
                            if (_field[i + 1, j] is Player)
                            {
                                _field.Draw();
                                LoseGameByRockfall();
                                return;
                            }

                            if (_field[i + 1, j] is Monster monster)
                            {
                                monster.Die();
                                continue;
                            }
                        }
                        if (_field[i + 1, j] is Empty || _field[i + 1, j] is Stone)
                        {
                            if (_field[i + 1, j] is Empty emptyCell)
                            {
                                _field[i, j] = emptyCell;
                                _field[i + 1, j] = currentStone;
                                isStoneFalled = true;
                            }
                            else if (_field[i + 1, j] is Stone)
                            {
                                try
                                {
                                    if (_field[i, j - 1] is Empty emptyCellLeft)
                                    {
                                        if (_field[i + 1, j - 1] is Empty ||
                                            _field[i + 1, j - 1] is IDie)
                                        {
                                            _field[i, j] = emptyCellLeft;
                                            _field[i, j - 1] = currentStone;
                                            isStoneFalled = true;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                if (!isStoneFalled)
                                {
                                    try
                                    {
                                        if (_field[i, j + 1] is Empty emptyCellRight)
                                        {
                                            if (_field[i + 1, j + 1] is Empty ||
                                                _field[i + 1, j + 1] is IDie)
                                            {
                                                _field[i, j] = emptyCellRight;
                                                _field[i, j + 1] = currentStone;
                                                isStoneFalled = true;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                            }
                        }

                        currentStone.IsFalling = isStoneFalled;
                    }
                }
            }
            _field.Draw();
        }

        private void IsGameOver()
        {
            var isPlayerCantMoveDown =
                _field[_player.Row + 1, _player.Col] is Stone || _field[_player.Row + 1, _player.Col] is Wall;
            if (!isPlayerCantMoveDown)
                return;

            var isPlayerCantMoveUp =
                _field[_player.Row - 1, _player.Col] is Stone || _field[_player.Row - 1, _player.Col] is Wall;
            if (!isPlayerCantMoveUp)
                return;

            var isPlayerCantMoveLeft =
                _player.Col > 2 && _field[_player.Row, _player.Col - 1] is Stone && !(_field[_player.Row, _player.Col - 2] is Empty) ||
                _field[_player.Row, _player.Col - 1] is Wall;
            if (!isPlayerCantMoveLeft)
                return;

            var isPlayerCantMoveRight =
                _player.Col < _field.ColsCount - 3 && _field[_player.Row, _player.Col + 1] is Stone && !(_field[_player.Row, _player.Col + 2] is Empty) ||
                _field[_player.Row, _player.Col + 1] is Wall;
            if (!isPlayerCantMoveRight)
                return;

            _gameOverMessage = "Ooops!\nYou lose!";
            IsOver = new KeyValuePair<bool, bool>(true, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoseGameByRockfall()
        {
            _gameOverMessage = "Ooops!\nYou died of a rockfall!";
            IsOver = new KeyValuePair<bool, bool>(true, false);
        }

        private void LoseGameByMonster()
        {
            _gameOverMessage = "Ooops!\nYou killed by a monster!";
            IsOver = new KeyValuePair<bool, bool>(true, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WinGame()
        {
            _gameOverMessage = "Congratulations!\nYou win!";
            IsOver = new KeyValuePair<bool, bool>(true, true);
        }



        public void WindowResize(object sender, EventArgs handler)
        {
            if (sender is Canvas gameFieldCanvas)
            {
                _field.NumOfViewCols = (int)(gameFieldCanvas.ActualWidth / _cellSize);
                _field.NumOfViewRows = (int)(gameFieldCanvas.ActualHeight / _cellSize);
                _field.Draw();
            }
        }

        private void MoveMonsters(object sender, EventArgs handler)
        {
            if (_monsters.Any(monster => monster.Move()))
            {
                LoseGameByMonster();
            }
        }

        private void Render(object sender, EventArgs handler)
        {
            if (IsOver.Key)
            {
                if (IsOver.Value)
                {
                    
                    _levelCompletedSound.Position = TimeSpan.Zero;
                    _levelCompletedSound.Play();
                    
                    if (_field.CurrentLvl >= Levels.Instances.Length)
                    {
                        MessageBox.Show(_gameOverMessage, "Win Game Message", MessageBoxButton.OK);
                        _renderLoop.Stop();
                        _moveMonstersLoop.Stop();
                        _rockfallLoop.Stop();
                    }
                    else
                    {
                        var decision = MessageBox.Show(_gameOverMessage + "\nStart next lvl?", "Win Game Message", MessageBoxButton.YesNo,
                            MessageBoxImage.Information,
                            MessageBoxResult.No);
                        if (decision == MessageBoxResult.Yes)
                        {
                            _field.LoadNexLvl();
                            _monsters = GetAllMonsters();
                        }
                        IsOver = new KeyValuePair<bool, bool>(false, false);
                    }

                    _levelCompletedSound.Stop();
                }
                else
                {
                    _player.Die();
                    _field.Draw();
                    _moveMonstersLoop.Stop();
                    var decision = MessageBox.Show(_gameOverMessage + "\nTry again?", "Game Over Message", MessageBoxButton.YesNo,
                        MessageBoxImage.Information,
                        MessageBoxResult.No);
                    if (decision == MessageBoxResult.Yes)
                    {
                        _field.ReloadLvl();
                        _monsters = GetAllMonsters();
                        IsOver = new KeyValuePair<bool, bool>(false, false);
                        _moveMonstersLoop.Start();
                    }
                    else
                    {

                        _moveMonstersLoop.Stop();
                        _renderLoop.Stop();
                        _rockfallLoop.Stop();
                        Application.Current.Shutdown();
                    }
                }
            }
            else if (_field.IsArrayNeedRedraw)
                _field.Draw();
        }
    }
}
