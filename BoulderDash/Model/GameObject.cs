using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BoulderDashGUI.Model
{
    abstract class GameObject : ICloneable
    {
        protected Image View { get; }
        private readonly Rectangle _backgroundRectangle;
        public double CellSize => 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Print(Canvas gameField, Thickness margin)
        {
            if (_backgroundRectangle.Stroke is null)
            {
                var bgColor = (SolidColorBrush)gameField.Background;

                _backgroundRectangle.Stroke = _backgroundRectangle.Fill = new SolidColorBrush(bgColor?.Color ?? Brushes.White.Color);
            }

            try
            {
                gameField.Children.Remove(_backgroundRectangle);
                gameField.Children.Remove(View);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            View.Margin = margin;
            _backgroundRectangle.Margin = margin;
            gameField.Children.Add(_backgroundRectangle);
            gameField.Children.Add(View);
        }

        protected GameObject()
        {
            View = new Image();
            View.Width = View.Height = CellSize;


            _backgroundRectangle = new Rectangle();
            _backgroundRectangle.Stroke = _backgroundRectangle.Fill = null;
            _backgroundRectangle.Width = _backgroundRectangle.Height = CellSize;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
