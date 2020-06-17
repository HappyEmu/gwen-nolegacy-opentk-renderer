using Gwen.Control;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;

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
        private Key TranslateKeyCode(OpenToolkit.Windowing.Common.Input.Key key)
        {
            switch (key)
            {
                case OpenToolkit.Windowing.Common.Input.Key.BackSpace: return Key.Backspace;
                case OpenToolkit.Windowing.Common.Input.Key.Enter: return Key.Return;
                case OpenToolkit.Windowing.Common.Input.Key.Escape: return Key.Escape;
                case OpenToolkit.Windowing.Common.Input.Key.Tab: return Key.Tab;
                case OpenToolkit.Windowing.Common.Input.Key.Space: return Key.Space;
                case OpenToolkit.Windowing.Common.Input.Key.Up: return Key.Up;
                case OpenToolkit.Windowing.Common.Input.Key.Down: return Key.Down;
                case OpenToolkit.Windowing.Common.Input.Key.Left: return Key.Left;
                case OpenToolkit.Windowing.Common.Input.Key.Right: return Key.Right;
                case OpenToolkit.Windowing.Common.Input.Key.Home: return Key.Home;
                case OpenToolkit.Windowing.Common.Input.Key.End: return Key.End;
                case OpenToolkit.Windowing.Common.Input.Key.Delete: return Key.Delete;
                case OpenToolkit.Windowing.Common.Input.Key.LControl:
                    m_AltGr = true;
                    return Key.Control;
                case OpenToolkit.Windowing.Common.Input.Key.LAlt: return Key.Alt;
                case OpenToolkit.Windowing.Common.Input.Key.LShift: return Key.Shift;
                case OpenToolkit.Windowing.Common.Input.Key.RControl: return Key.Control;
                case OpenToolkit.Windowing.Common.Input.Key.RAlt:
                    if (m_AltGr)
                    {
                        m_Canvas.Input_Key(Key.Control, false);
                    }
                    return Key.Alt;
                case OpenToolkit.Windowing.Common.Input.Key.RShift: return Key.Shift;

            }
            return Key.Invalid;
        }

        /// <summary>
        /// Translates alphanumeric OpenTK key code to character value.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>Translated character.</returns>
        private static char TranslateChar(OpenToolkit.Windowing.Common.Input.Key key)
        {
            if (key >= OpenToolkit.Windowing.Common.Input.Key.A && key <= OpenToolkit.Windowing.Common.Input.Key.Z)
            {
                return (char)('a' + ((int)key - (int)OpenToolkit.Windowing.Common.Input.Key.A));
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
