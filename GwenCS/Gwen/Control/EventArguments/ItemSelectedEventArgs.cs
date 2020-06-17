using System;

namespace Gwen.Control
{
    public class ItemSelectedEventArgs : EventArgs
    {
        public Base SelectedItem { get; private set; }

        internal ItemSelectedEventArgs(Base selecteditem)
        {
            SelectedItem = selecteditem;
        }
    }
}
