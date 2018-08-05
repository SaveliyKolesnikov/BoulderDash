using System.Diagnostics;
using System.Windows.Media;
using BoulderDashGUI.Model.Interfaces;

namespace BoulderDashGUI.Model.GameObjects
{
    abstract class Monster : GameObject, IDie, IMove
    {
        private GameField _gameField;

        public GameField GameField
        {
            protected get => _gameField;
            set
            {
                _gameField = value;

                if (!(_gameField is null))
                {
                    RowsCount = _gameField.RowsCount;
                    ColsCount = _gameField.ColsCount;
                }
            }
        }

        protected int RowsCount;
        protected int ColsCount;
        protected int Row;
        protected int Col;
        protected int DefaultDirectionIdx;
        protected int CurrentDirectionIndex;
        protected bool IsDead;

        public void SetCoordinates(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void Reset()
        {
            CurrentDirectionIndex = DefaultDirectionIdx;
            IsDead = false;
        }


        protected Monster()
        {
            GameField = null;
            IsDead = false;
            DieSound = new MediaPlayer();
            DieSound.Volume = 100;
            DieSound.MediaFailed += (o, args) =>
            {
                Trace.WriteLine("AudioFailed");
                Trace.WriteLine(args.ErrorException);
            };
        }

        protected MediaPlayer DieSound;

        public abstract bool Move();

        public abstract void Die();
    }
}