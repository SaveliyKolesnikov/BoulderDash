using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects
{
    class Empty : GameObject
    {
        public Empty() => View.Source = new BitmapImage(new Uri("/Images/Empty.png", UriKind.Relative));
    }
}
