using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects
{
    class Exit : GameObject
    {
        public Exit() => View.Source = new BitmapImage(new Uri("/Images/Exit.png", UriKind.Relative));
    }
}
