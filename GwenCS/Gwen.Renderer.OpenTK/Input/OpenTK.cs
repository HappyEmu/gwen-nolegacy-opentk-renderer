using Gwen.Control;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Gwen.Input
{
    public class OpenTK
    {

        #region Properties

        private Canvas m_Canvas = null;

        private float m_MouseX = 0.0f;
        private float m_MouseY = 0.0f;
        private bool m_AltGr = false;

        #endregion

        #region Constructors
        public OpenTK()
        {
        }

        #endregion

        #region Methods
        public void Initialize(Canvas c)
        {
            m_Canvas = c;
        }

        /// <summary>
        /// Translates control key's OpenTK key code to GWEN's code.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>GWEN key code.</returns>
        private Key TranslateKeyCode(Keys key)
        {
            switch (key)
            {
                case Keys.Backspace: return Key.Backspace;
                case Keys.Enter: return Key.Return;
                case Keys.Escape: return Key.Escape;
                case Keys.Tab: return Key.Tab;
                case Keys.Space: return Key.Space;
                case Keys.Up: return Key.Up;
                case Keys.Down: return Key.Down;
                case Keys.Left: return Key.Left;
                case Keys.Right: return Key.Right;
                case Keys.Home: return Key.Home;
                case Keys.End: return Key.End;
                case Keys.Delete: return Key.Delete;
                case Keys.LeftControl:
                    m_AltGr = true;
                    return Key.Control;
                case Keys.LeftAlt: return Key.Alt;
                case Keys.LeftShift: return Key.Shift;
                case Keys.RightControl: return Key.Control;
                case Keys.RightAlt:
                    if (m_AltGr)
                    {
                        m_Canvas.Input_Key(Key.Control, false);
                    }
                    return Key.Alt;
                case Keys.RightShift: return Key.Shift;
            }
            return Key.Invalid;
        }

        /// <summary>
        /// Translates alphanumeric OpenTK key code to character value.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>Translated character.</returns>
        private static char TranslateChar(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {
                return (char)('a' + ((int)key - (int)Keys.A));
            }

            return ' ';
        }

        public bool ProcessMouseMessage(MouseMoveEventArgs args)
        {
            if (null == m_Canvas)
            {
                return false;
            }

            int dx = (int)(args.X - m_MouseX);
            int dy = (int)(args.Y - m_MouseY);

            m_MouseX = args.X;
            m_MouseY = args.Y;

            return m_Canvas.Input_MouseMoved((int)m_MouseX, (int)m_MouseY, dx, dy);
        }

        public bool ProcessMouseMessage(MouseButtonEventArgs args)
        {
            if (null == m_Canvas)
            {
                return false;
            }

            /* We can not simply cast ev.Button to an int, as 1 is middle click, not right click. */
            int ButtonID = -1; //Do not trigger event.

            if (args.Button == MouseButton.Left)
            {
                ButtonID = 0;
            }
            else if (args.Button == MouseButton.Right)
            {
                ButtonID = 1;
            }

            if (ButtonID != -1) //We only care about left and right click for now
            {
                return m_Canvas.Input_MouseButton(ButtonID, args.IsPressed);
            }

            return false;
        }

        public bool ProcessMouseMessage(MouseWheelEventArgs args)
        {
            if (null == m_Canvas)
            {
                return false;
            }

            return m_Canvas.Input_MouseWheel((int)(args.OffsetX + args.OffsetY) * 60);
        }


        public bool ProcessKeyDown(KeyboardKeyEventArgs ev)
        {
            char ch = TranslateChar(ev.Key);

            if (InputHandler.DoSpecialKeys(m_Canvas, ch))
            {
                return false;
            }
            /*
if (ch != ' ')
{
   m_Canvas.Input_Character(ch);
}
*/
            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, true);
        }

        public bool ProcessKeyUp(KeyboardKeyEventArgs ev)
        {
            char ch = TranslateChar(ev.Key);

            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, false);
        }

        public void KeyPress(TextInputEventArgs e)
        {
            m_Canvas.Input_Character((char)e.Unicode);
        }

        #endregion
    }
}
