using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects
{
    class Stone : GameObject
    {
        public bool IsFalling { get; set; }

        public Stone()
        {
            View.Source = new BitmapImage(new Uri("/Images/Stone.png", UriKind.Relative));
            IsFalling = false;
        }
    }
}
