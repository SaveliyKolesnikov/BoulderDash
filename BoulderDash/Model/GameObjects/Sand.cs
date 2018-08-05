using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects
{
    class Sand : GameObject
    {
        public Sand() =>  View.Source = new BitmapImage(new Uri("/Images/Sand.png", UriKind.Relative));
    }
}
