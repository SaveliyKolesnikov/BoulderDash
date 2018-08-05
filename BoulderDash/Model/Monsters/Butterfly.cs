using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BoulderDashGUI.Model.GameObjects;
using BoulderDashGUI.Model.GameObjects.Diamonds;

namespace BoulderDashGUI.Model.Monsters
{
    class Butterfly : Monster
    {
        private readonly Dictionary<Direction, Func<KeyValuePair<bool, bool>>> _moveInDirection;
        private readonly Direction[] _directionsOrder;

        public Butterfly(int row, int col)
        {
            View.Source = new BitmapImage(new Uri("/Images/Butterfly.png", UriKind.Relative));
            //_dieSound.Open(new Uri("",UriKind.Relative));
            Row = row;
            Col = col;
            DefaultDirectionIdx = 0;

            _moveInDirection = new Dictionary<Direction, Func<KeyValuePair<bool, bool>>>()
            {
                [Direction.Left] = MoveLeft,
                [Direction.Down] = MoveDown,
                [Direction.Right] = MoveRight,
                [Direction.Up] = MoveUp
            };

            _directionsOrder = new[]
            {
                Direction.Left,
                Direction.Down,
                Direction.Right,
                Direction.Up
            };

            CurrentDirectionIndex = 0;

            DieSound.Open(new Uri($"{Directory.GetCurrentDirectory()}/Data/Sounds/butterflyDieSound.wav", UriKind.Absolute));
        }


        public override bool Move()
        {
            if (GameField is null)
                throw new NotImplementedException($"Set gamefield for this monster({Row},{Col}).");

            if (IsDead) return false;

            var isPlayerAround = FindPlayerAround();
            int tryToChangeDirectionIndex;

            if (!(isPlayerAround is null))
            {
                tryToChangeDirectionIndex = GetDirectionIndex((Direction)isPlayerAround);
            }
            else
            { 
                tryToChangeDirectionIndex =
                ((_directionsOrder.Length - 1) + CurrentDirectionIndex) % _directionsOrder.Length;
            }

            var move = _moveInDirection[_directionsOrder[tryToChangeDirectionIndex]].Invoke();
            if (move.Key)
            {
                CurrentDirectionIndex = tryToChangeDirectionIndex;
                return move.Value;
            }

            for (var i = 0; i < _directionsOrder.Length - 1; i++)
            {
                move = _moveInDirection[_directionsOrder[CurrentDirectionIndex]].Invoke();
                if (move.Key)
                    return move.Value;

                CurrentDirectionIndex = (CurrentDirectionIndex + 1) % _directionsOrder.Length;
            }

            return false;
        }

        private Direction? FindPlayerAround()
        {
            try
            {
                if (GameField[Row - 1, Col] is Player)
                    return Direction.Up;
                if (GameField[Row, Col + 1] is Player)
                    return Direction.Right;
                if (GameField[Row + 1, Col] is Player)
                    return Direction.Down;
                if (GameField[Row, Col - 1] is Player)
                    return Direction.Left;
            }
            catch (IndexOutOfRangeException e)
            {
                Trace.WriteLine(e);
            }

            return null;
        }

        private int GetDirectionIndex(Direction direction)
        {
            for (var i = 0; i < _directionsOrder.Length; i++)
                if (direction == _directionsOrder[i])
                    return i;

            return -1;
        }

        // 1st bool isMoved, 2d bool is player has been died.
        private KeyValuePair<bool, bool> MoveLeft()
        {
            if (Col - 1 < 0)
                return new KeyValuePair<bool, bool>(false, false);

            switch (GameField[Row, Col - 1])
            {
                case Player _:
                    return new KeyValuePair<bool, bool>(true, true);
                case Empty emptyLeftCell:
                    GameField[Row, Col] = emptyLeftCell;
                    Col--;
                    GameField[Row, Col] = this;
                    return new KeyValuePair<bool, bool>(true, false);
            }


            return new KeyValuePair<bool, bool>(false, false);
        }

        private KeyValuePair<bool, bool> MoveDown()
        {
            if (Row + 1 >= RowsCount)
                return new KeyValuePair<bool, bool>(false, false);

            switch (GameField[Row + 1, Col])
            {
                case Player _:
                    return new KeyValuePair<bool, bool>(true, true);
                case Empty emptyCellBelow:
                    GameField[Row, Col] = emptyCellBelow;
                    Row++;
                    GameField[Row, Col] = this;
                    return new KeyValuePair<bool, bool>(true, false);
            }


            return new KeyValuePair<bool, bool>(false, false);
        }

        private KeyValuePair<bool, bool> MoveRight()
        {
            if (Col + 1 >= ColsCount)
                return new KeyValuePair<bool, bool>(false, false);

            switch (GameField[Row, Col + 1])
            {
                case Player _:
                    return new KeyValuePair<bool, bool>(true, true);
                case Empty emptyRightCell:
                    GameField[Row, Col] = emptyRightCell;
                    Col++;
                    GameField[Row, Col] = this;
                    return new KeyValuePair<bool, bool>(true, false);
            }


            return new KeyValuePair<bool, bool>(false, false);
        }

        private KeyValuePair<bool, bool> MoveUp()
        {
            if (Row - 1 < 0)
                return new KeyValuePair<bool, bool>(false, false);

            switch (GameField[Row - 1, Col])
            {
                case Player _:
                    return new KeyValuePair<bool, bool>(true, true);
                case Empty emptyCellAbove:
                    GameField[Row, Col] = emptyCellAbove;
                    Row--;
                    GameField[Row, Col] = this;
                    return new KeyValuePair<bool, bool>(true, false);
            }


            return new KeyValuePair<bool, bool>(false, false);
        }

        public override void Die()
        {
            DieSound.Position = TimeSpan.Zero;
            DieSound.Play();

            try
            {
                IsDead = true;
                var rand = new Random(DateTime.Now.Millisecond);
                for (var i = Row - 1; i <= Row + 1; i++)
                {
                    for (var j = Col - 1; j <= Col + 1; j++)
                    {
                        var currentCell = GameField[i, j];
                        if (currentCell is Wall || currentCell is Player) continue;

                        var randomNum = rand.Next(0, 5);
                        switch (randomNum)
                        {
                            case 0:
                                GameField[i, j] = new RareDiamond();
                                break;
                            case 1:
                                GameField[i, j] = new CommonDiamond();
                                break;
                            default:
                                GameField[i, j] = new Empty();
                                break;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Trace.WriteLine(e);
            }
            
        }
    }
}
