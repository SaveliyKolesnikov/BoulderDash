using BoulderDashGUI.Model.Interfaces;

namespace BoulderDashGUI.Model.GameObjects
{
    abstract class Diamond : GameObject, ISound
    {
        public abstract int Value { get; }
        public abstract void Play();
    }
}
