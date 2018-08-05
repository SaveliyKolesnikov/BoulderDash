using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using BoulderDashGUI.Model.GameObjects;

namespace BoulderDashGUI.Model
{
    class GameField
    {
        public bool IsArrayNeedRedraw;

        private int _currentLvlDiamondsRequired;
        public int DiamondsToNextLvl => 
            _currentLvlDiamondsRequired - _player.Score > 0 ? _currentLvlDiamondsRequired - _player.Score : 0;
        public int RowsCount { get; private set; }
        public int ColsCount { get; private set; }

        public double CanvasWidth => _canvasField.ActualWidth;
        public double CanvasHeight => _canvasField.ActualHeight;
        public int CurrentLvl { get; private set; }
        public GameObject[,] Field { get; private set; }


        private readonly Canvas _canvasField;
        private readonly Player _player;
        private bool[,] _isRedrawNeed;
        private readonly double _cellSize;


        public int NumOfViewCols;
        public int NumOfViewRows;

        private int _startViewCol;

        public GameField(Canvas canvasField, int startLvl = 0)
        {
            _canvasField = canvasField;
            _player = Player.Instance;
            CurrentLvl = startLvl;
            _cellSize = _player.CellSize;
        }

        public void LoadNexLvl()
        {
            Field = Levels.Instances[CurrentLvl].Key.Clone() as GameObject[,];
            _currentLvlDiamondsRequired = Levels.Instances[CurrentLvl].Value;
            _player.Score = 0;
            _player.ResetDirection();
            if (!(Field is null))
            {
                RowsCount = Field.GetLength(0);
                ColsCount = Field.GetLength(1);
                _player.SetPlayerCoord(Field);
            }

            _canvasField.Children.Clear();

            var startViewCol = _player.Col - 3;
            StartViewCol = startViewCol > 0 ? startViewCol : 0;

            var startViewRow = _player.Row - 3;
            StartViewRow = startViewRow > 0 ? startViewRow : 0;

            CurrentLvl++;

            _isRedrawNeed = new bool[RowsCount, ColsCount];

            for (var i = 0; i < RowsCount; i++)
                for (var j = 0; j < ColsCount; j++)
                    _isRedrawNeed[i, j] = true;

            IsArrayNeedRedraw = true;

            Draw();
        }

        public void ReloadLvl()
        {
            CurrentLvl--;
            LoadNexLvl();
        }

        public int StartViewCol
        {
            get => _startViewCol;
            set
            {
                if (value < 0)
                    _startViewCol = 0;
                else if (value >= ColsCount)
                    _startViewCol = ColsCount - NumOfViewCols;
                else
                    _startViewCol = value;

            }
        }

        private int _startViewRow;

        public int StartViewRow
        {
            get => _startViewRow;
            set
            {
                if (value < 0)
                    _startViewRow = 0;
                else if (value >= RowsCount)
                    _startViewRow = RowsCount - NumOfViewRows;
                else
                    _startViewRow = value;

            }
        }




        public GameObject this[int i, int j]
        {
            get => Field[i, j];
            set
            {
                Field[i, j] = value;
                _isRedrawNeed[i, j] = true;
                if (!IsArrayNeedRedraw)
                    IsArrayNeedRedraw = true;
            }
        }

        public void RedrawInRange(int startRow, int endRow, int startCol, int endCol)
        {
            for (var i = startRow; i < RowsCount && i < endRow; i++)
                for (var j = startCol; j < ColsCount && j < endCol; j++)
                    _isRedrawNeed[i, j] = true;
        }

        public void RedrawAll()
        {
            for (var i = 0; i < RowsCount; i++)
                for (var j = 0; j < ColsCount; j++)
                    _isRedrawNeed[i, j] = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Draw()
        {

            for (int i = StartViewRow, top = 0; i < RowsCount && i < NumOfViewRows + StartViewRow; i++, top++)
            {
                var topMargin = top * _cellSize;
                for (int j = StartViewCol, left = 0; j < ColsCount && j < NumOfViewCols + StartViewCol; j++, left++)
                {
                    if (_isRedrawNeed[i, j])
                    {
                        var margin = new Thickness(left * _cellSize, topMargin, 0, 0);
                        Field[i, j].Print(_canvasField, margin);
                        _isRedrawNeed[i, j] = false;
                    }
                }
            }
            
            IsArrayNeedRedraw = false;
        }
    }
}