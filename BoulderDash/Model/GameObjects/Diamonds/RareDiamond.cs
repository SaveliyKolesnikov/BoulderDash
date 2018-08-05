using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects.Diamonds
{
    class RareDiamond : Diamond
    {
        public RareDiamond() => View.Source = new BitmapImage(new Uri("/Images/RareDiamond.png", UriKind.Relative));

        public override int Value => 5;

        public override void Play()
        {
            throw new NotImplementedException();
        }

    }
}
