using Gwen.Control;
using System.Drawing;

namespace Gwen.DragDrop
{
    public class Package
    {
        public string Name;
        public object UserData;
        public bool IsDraggable;
        public Base DrawControl;
        public Point HoldOffset;
    }
}
