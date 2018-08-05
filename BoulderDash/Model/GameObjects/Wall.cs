using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects
{
    class Wall : GameObject
    {
        public Wall() => View.Source = new BitmapImage(new Uri("/Images/Brick.png", UriKind.Relative));
    }
}
