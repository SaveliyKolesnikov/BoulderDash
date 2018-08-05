using System;
using System.Windows.Media.Imaging;

namespace BoulderDashGUI.Model.GameObjects.Diamonds
{
    class CommonDiamond : Diamond
    {
        public CommonDiamond() => View.Source = new BitmapImage(new Uri("/Images/CommonDiamond.png", UriKind.Relative));

        public override int Value => 1;

        public override void Play()
        {
            throw new NotImplementedException();
        }

        
    }
}
