using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class UserInput
    {
        static public void MouseClick(MouseButton Button)
        {
            switch (Button)
            {
                case MouseButton.Middle:
                    WinApi.MouseEvent(WinApi.MouseEventFlags.MiddleDown);
                    WinApi.MouseEvent(WinApi.MouseEventFlags.MiddleUp);
                    break;
                case MouseButton.Left:
                    WinApi.MouseEvent(WinApi.MouseEventFlags.LeftDown);
                    WinApi.MouseEvent(WinApi.MouseEventFlags.LeftUp);
                    break;
                case MouseButton.Right:
                    WinApi.MouseEvent(WinApi.MouseEventFlags.RightDown);
                    WinApi.MouseEvent(WinApi.MouseEventFlags.RightUp);
                    break;
            }
        }

        static public void MouseButtonEvent(MouseButton Button, ButtonEvents Event = ButtonEvents.None)
        {
            switch (Event)
            {
                case ButtonEvents.None:
                    break;
                case ButtonEvents.Up:
                    switch (Button)
                    {
                        case MouseButton.Middle:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.MiddleUp);
                            break;
                        case MouseButton.Left:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.LeftUp);
                            break;
                        case MouseButton.Right:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.RightUp);
                            break;
                    }
                    break;
                case ButtonEvents.Down:
                    switch (Button)
                    {
                        case MouseButton.Middle:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.MiddleDown);
                            break;
                        case MouseButton.Left:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.LeftDown);
                            break;
                        case MouseButton.Right:
                            WinApi.MouseEvent(WinApi.MouseEventFlags.RightDown);
                            break;
                    }
                    break;
            }
            
        }

        static public void SetMouse(int X, int Y)
        {
            WinApi.SetCursorPos(X, Y);
        }

        static public void MouseMove(int X, int Y)
        {
            var Old = WinApi.GetCursorPosition();
            WinApi.SetCursorPos(Old.X + X, Old.Y + Y);
        }

        static public void KeyboardClick(WinApi.Vk Button)
        {
            WinApi.KeyBDEvent(Button);
            WinApi.KeyBDEvent(Button,WinApi.KeyBDdwFlags.KEYEVENTF_KEYUP);
        }

        static public void ButtonEvent(WinApi.Vk Button, ButtonEvents Event = ButtonEvents.None)
        {
            switch (Event)
            {
                case ButtonEvents.None:
                    break;
                case ButtonEvents.Up:
                    WinApi.KeyBDEvent(Button, WinApi.KeyBDdwFlags.KEYEVENTF_KEYUP);
                    break;
                case ButtonEvents.Down:
                    WinApi.KeyBDEvent(Button);
                    break;
            }
        }

        static public Color GetScreenColorAt(int X,int Y)
        {
            Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = WinApi.BitBlt(hDC, 0, 0, 1, 1, hSrcDC, X, Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        public enum ButtonEvents : byte
        {
            None = 0,
            Up = 1,
            Down = 2
        }

        public enum MouseButton : byte
        {
            Middle = 0,
            Left = 1,
            Right = 2
        }
    }
}
