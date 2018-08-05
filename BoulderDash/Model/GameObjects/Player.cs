using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BoulderDashGUI.Model.Interfaces;

namespace BoulderDashGUI.Model.GameObjects
{

    class Player : GameObject, IDie
    {
        private int _score = 0;
        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                IsScoreChanged = true;
            }
        }
        public bool IsScoreChanged = true;
        public int Col { get; set; }
        public int Row { get; set; }

        private readonly MediaPlayer _dieSound;
        private readonly Direction _defaultDirection;
        private readonly Dictionary<Direction, BitmapImage> _imagesByDirection;


        public static Player Instance { get; }

        static Player() => Instance = new Player();

        private Player()
        {
            _defaultDirection = Direction.Down;
            _imagesByDirection = new Dictionary<Direction, BitmapImage>()
            {
                [Direction.Up] = new BitmapImage(new Uri("/Images/Player_up.png", UriKind.Relative)),
                [Direction.Right] = new BitmapImage(new Uri("/Images/Player_right.png", UriKind.Relative)),
                [Direction.Down] = new BitmapImage(new Uri("/Images/Player_down.png", UriKind.Relative)),
                [Direction.Left] = new BitmapImage(new Uri("/Images/Player_left.png", UriKind.Relative))
            };
            SetImageUsingDirection(_defaultDirection);
            _dieSound = new MediaPlayer();
            _dieSound.Open(new Uri($"{Directory.GetCurrentDirectory()}/Data/Sounds/playerDie.mp3", UriKind.Absolute));
            _dieSound.Volume = 100;
            _dieSound.MediaFailed += (o, args) =>
            {
                Trace.WriteLine("AudioFailed");
                Trace.WriteLine(args.ErrorException);
            };
        }

        public void SetImageUsingDirection(Direction direction) => View.Source = _imagesByDirection[direction];

        public void ResetDirection() => SetImageUsingDirection(_defaultDirection);



        public void SetPlayerCoord(GameObject[,] field)
        {
            var rows = field.GetLength(0);
            var cols = field.GetLength(1);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    if (field[i, j] is Player)
                    {
                        this.Row = i;
                        this.Col = j;
                        return;
                    }
                }
            }

            throw new ArgumentException("Player is absent in levels array");
        }


        public void Die()
        {
            _dieSound.Position = TimeSpan.Zero;
            _dieSound.Play();
        }
    }
}
