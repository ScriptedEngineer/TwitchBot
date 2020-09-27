using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;

namespace TwitchBot
{
    class WinApi
    {
#pragma warning disable IDE1006 // Стили именования
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr HWnd, GetWindow_Cmd cmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(int hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);
        public static MousePoint GetCursorPosition()
        {
            var gotPoint = GetCursorPos(out MousePoint currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(Int32 i);
        public static bool GetKeyState(int i)
        {
            return GetAsyncKeyState(i) != 0;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId([In] IntPtr hWnd,[Out, Optional] IntPtr lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort GetKeyboardLayout([In] int idThread);
        public static ushort GetKeyboardLayout()
        {
            return GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage lpMsg, IntPtr window, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk,byte bScan,int dwFlags,int dwExtraInfo);
        public static void KeyBDEvent(Vk button,KeyBDdwFlags value = 0)
        {
            keybd_event((byte)button, 0, (int)value, 0);
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        public static void MouseEvent(MouseEventFlags value,int dwData = 0)
        {
            MousePoint position = GetCursorPosition();
            mouse_event((int)value,position.X,position.Y, dwData, 0);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        public static void SetEnableScreen(bool enable)
        {
            if (enable)
            {
                SendMessage(0xffff, 0x0112, 0xF170, -1);
                MouseEvent(MouseEventFlags.Move);
            }
            else
            {
                SendMessage(0xffff, 0x0112, 0xF170, 2);
            }
        }
        public static class MouseHook

        {
            public static event EventHandler<MouseEventArgs> MouseAction = delegate { };

            public static void Start()
            {
                _hookID = SetHook(_proc);


            }
            public static void stop()
            {
                UnhookWindowsHookEx(_hookID);
            }

            private static LowLevelMouseProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;

            private static IntPtr SetHook(LowLevelMouseProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc,
                      GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

            private static IntPtr HookCallback(
              int nCode, IntPtr wParam, IntPtr lParam)
            {
                MouseMessages MSGTP = (MouseMessages)wParam;
                if (nCode >= 0 && (MSGTP == MouseMessages.WM_MOUSEWHEEL || MSGTP == MouseMessages.WM_MOUSEMOVE))
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    MouseAction(null, new MouseEventArgs(NativeMethods.GET_WHEEL_DELTA_WPARAM(hookStruct.mouseData), hookStruct.pt.x, hookStruct.pt.y));
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            private const int WH_MOUSE_LL = 14;

            private enum MouseMessages
            {
                WM_LBUTTONDOWN = 0x0201,
                WM_LBUTTONUP = 0x0202,
                WM_MOUSEMOVE = 0x0200,
                WM_MOUSEWHEEL = 0x020A,
                WM_RBUTTONDOWN = 0x0204,
                WM_RBUTTONUP = 0x0205
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MSLLHOOKSTRUCT
            {
                public POINT pt;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook,
              LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
              IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            internal static class NativeMethods
            {
                internal static ushort HIWORD(IntPtr dwValue)
                {
                    return (ushort)((((long)dwValue) >> 0x10) & 0xffff);
                }

                internal static ushort HIWORD(uint dwValue)
                {
                    return (ushort)(dwValue >> 0x10);
                }

                internal static int GET_WHEEL_DELTA_WPARAM(IntPtr wParam)
                {
                    return (short)HIWORD(wParam);
                }

                internal static int GET_WHEEL_DELTA_WPARAM(uint wParam)
                {
                    return (short)HIWORD(wParam);
                }
            }

            public class MouseEventArgs : EventArgs
            {
                public int Wheel, X, Y;
                public MouseEventArgs(int wheel, int x, int y)
                {
                    Wheel = wheel;
                    X = x;
                    Y = y;
                }
            }

        }
        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,
            WM_GETTEXT = 0x000D
        }

        [Flags]
        public enum KeyModifier
        {
            None = 0x0000,
            Alt = 0x0001,
            Ctrl = 0x0002,
            NoRepeat = 0x4000,
            Shift = 0x0004,
            Win = 0x0008
        }

        public enum KeyBDdwFlags
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002
        }

        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Wheel = 0x00000800,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        public enum Vk : byte
        {
            VK_UNKNOWN = 0x00,  //Неизвестно
            VK_LBUTTON = 0x01,  //Левая кнопка мыши
            VK_RBUTTON = 0x02,  //Правая кнопка мыши
            VK_CANCEL = 0x03,   //Обработка Control-break
            VK_MBUTTON = 0x04,  //Средняя кнопка мыши
            VK_XBUTTON1 = 0x05, //кнопка мыши X1
            VK_XBUTTON2 = 0x06, //кнопка мыши X2
            VK_BACK = 0x08,     //BACKSPACE key
                                //0x07	Не определено
            VK_TAB = 0x09,      //TAB key
                                //0x0A-0x0B Зарезервировано
            VK_CLEAR = 0x0C,    //CLEAR key
            VK_RETURN = 0x0D,   //ENTER key
                                //0x0E-0x0F Не определено
            VK_SHIFT = 0x10,    //SHIFT key
            VK_CONTROL = 0x11,  //CTRL key
            VK_MENU = 0x12,     //ALT key
            VK_PAUSE = 0x13,    //PAUSE key
            VK_CAPITAL = 0x14,  //CAPS LOCK key
                                //0x15 IME modes key
                                //0x16 Не определено
                                //0x17-0x19 IME modes key
                                //0x1A Не определено
            VK_ESCAPE = 0x1B,   //ESC key
                                //0x1C-0x1F IME Keys
            VK_SPACE = 0x20,    //Пробел
            VK_PRIOR = 0x21,    //PAGE UP key
            VK_NEXT = 0x22,     //PAGE DOWN key
            VK_END = 0x23,      //END key
            VK_HOME = 0x24,     //HOME key
            VK_LEFT = 0x25,     //LEFT ARROW key
            VK_UP = 0x26,       //UP ARROW key
            VK_RIGHT = 0x27,    //RIGHT ARROW key
            VK_DOWN = 0x28,     //DOWN ARROW key
            VK_SELECT = 0x29,   //SELECT key
            VK_PRINT = 0x2A,    //PRINT key
            VK_EXECUTE = 0x2B,  //EXECUTE key
            VK_SNAPSHOT = 0x2C, //PRINT SCREEN key for Windows 3.0 and later
            VK_INSERT = 0x2D,   //INS key
            VK_DELETE = 0x2E,   //DEL key
            VK_HELP = 0x2F,     //HELP key
            VK_0 = 0x30,        //0 key
            VK_1 = 0x31,        //1 key
            VK_2 = 0x32,        //2 key
            VK_3 = 0x33,        //3 key
            VK_4 = 0x34,        //4 key
            VK_5 = 0x35,        //5 key
            VK_6 = 0x36,        //6 key
            VK_7 = 0x37,        //7 key
            VK_8 = 0x38,        //8 key
            VK_9 = 0x39,        //9 key
                                //0x3A-0x40 Не определено
            VK_A = 0x41,        //A key
            VK_B = 0x42,        //B key
            VK_C = 0x43,        //C key
            VK_D = 0x44,        //D key
            VK_E = 0x45,        //E key
            VK_F = 0x46,        //F key
            VK_G = 0x47,        //G key
            VK_H = 0x48,        //H key
            VK_I = 0x49,        //I key
            VK_J = 0x4A,        //J key
            VK_K = 0x4B,        //K key
            VK_L = 0x4C,        //L key
            VK_M = 0x4D,        //M key
            VK_N = 0x4E,        //N key
            VK_O = 0x4F,        //O key
            VK_P = 0x50,        //P key
            VK_Q = 0x51,        //Q key
            VK_R = 0x52,        //R key
            VK_S = 0x53,        //S key
            VK_T = 0x54,        //T key
            VK_U = 0x55,        //U key
            VK_V = 0x56,        //V key
            VK_W = 0x57,        //W key
            VK_X = 0x58,        //X key
            VK_Y = 0x59,        //Y key
            VK_Z = 0x5A,        //Z key
            VK_LWIN = 0x5B,     //Left Windows key(Microsoft Natural Keyboard)
            VK_RWIN = 0x5C,     //Right Windows key(Microsoft Natural Keyboard)
            VK_APPS = 0x5D,     //Applications key(Microsoft Natural Keyboard)
                                //0x5E Зарезервировано
            VK_SLEEP = 0x5F,    //Computer Sleep key
            VK_NUMPAD0 = 0x60,  //Numeric keypad 0 key
            VK_NUMPAD1 = 0x61,  //Numeric keypad 1 key
            VK_NUMPAD2 = 0x62,  //Numeric keypad 2 key
            VK_NUMPAD3 = 0x63,  //Numeric keypad 3 key
            VK_NUMPAD4 = 0x64,  //Numeric keypad 4 key
            VK_NUMPAD5 = 0x65,  //Numeric keypad 5 key
            VK_NUMPAD6 = 0x66,  //Numeric keypad 6 key
            VK_NUMPAD7 = 0x67,  //Numeric keypad 7 key
            VK_NUMPAD8 = 0x68,  //Numeric keypad 8 key
            VK_NUMPAD9 = 0x69,  //Numeric keypad 9 key
            VK_MULTIPLY = 0x6A, //Multiply key(*)
            VK_ADD = 0x6B,      //Add key(+)
            VK_SEPARATOR = 0x6C,//Separator key
            VK_SUBTRACT = 0x6D, //Subtract key(-)
            VK_DECIMAL = 0x6E,  //Decimal key
            VK_DIVIDE = 0x6F,   //Divide key(/)
            VK_F1 = 0x70,       //F1 key
            VK_F2 = 0x71,       //F2 key
            VK_F3 = 0x72,       //F3 key
            VK_F4 = 0x73,       //F4 key
            VK_F5 = 0x74,       //F5 key
            VK_F6 = 0x75,       //F6 key
            VK_F7 = 0x76,       //F7 key
            VK_F8 = 0x77,       //F8 key
            VK_F9 = 0x78,       //F9 key
            VK_F10 = 0x79,      //F10 key
            VK_F11 = 0x7A,      //F11 key
            VK_F12 = 0x7B,      //F12 key
            VK_F13 = 0x7C,      //F13 key
            VK_F14 = 0x7D,      //F14 key
            VK_F15 = 0x7E,      //F15 key
            VK_F16 = 0x7F,      //F16 key
            VK_F17 = 0x80,      //F17 key
            VK_F18 = 0x81,      //F18 key
            VK_F19 = 0x82,      //F19 key
            VK_F20 = 0x83,      //F20 key
            VK_F21 = 0x84,      //F21 key
            VK_F22 = 0x85,      //F22 key
            VK_F23 = 0x86,      //F23 key
            VK_F24 = 0x87,      //F24 key
                                //0x88-0x8F Не используются
            VK_NUMLOCK = 0x90,  //NUM LOCK key
            VK_SCROLL = 0x91,   //SCROLL LOCK key
                                //0x92-0x96 OEM Keys
                                //0x97-0x9F Не используются
            VK_LSHIFT = 0xA0,   //Left SHIFT key
            VK_RSHIFT = 0xA1,   //Right SHIFT key
            VK_LCONTROL = 0xA2, //Left CONTROL key
            VK_RCONTROL = 0xA3, //Right CONTROL key
            VK_LMENU = 0xA4,    //Left MENU key
            VK_RMENU = 0xA5,    //Right MENU key

            VK_BROWSER_BACK = 0xA6,         //Browser Back key
            VK_BROWSER_FORWARD = 0xA7,      //Browser Forward key
            VK_BROWSER_REFRESH = 0xA8,      //Browser Refresh key
            VK_BROWSER_STOP = 0xA9,         //Browser Stop key
            VK_BROWSER_SEARCH = 0xAA,       //Browser Search key
            VK_BROWSER_FAVORITES = 0xAB,    //Browser Favorites key
            VK_BROWSER_HOME = 0xAC,         //Browser Start and Home key
            VK_VOLUME_MUTE = 0xAD,          //Volume Mute key
            VK_VOLUME_DOWN = 0xAE,          //Volume Down key
            VK_VOLUME_UP = 0xAF,            //Volume Up key
            VK_MEDIA_NEXT_TRACK = 0xB0,     //Next Track key
            VK_MEDIA_PREV_TRACK = 0xB1,     //Previous Track key
            VK_MEDIA_STOP = 0xB2,           //Stop Media key
            VK_MEDIA_PLAY_PAUSE = 0xB3,     //Play/Pause Media key
            VK_LAUNCH_MAIL = 0xB4,          //Start Mail key
            VK_LAUNCH_MEDIA_SELECT = 0xB5,  //Select Media key
            VK_LAUNCH_APP1 = 0xB6,          //Start Application 1 key
            VK_LAUNCH_APP2 = 0xB7,          //Start Application 2 key
                                            //B8-B9 Зарезервировано
            VK_OEM_1 = 0xBA,                //For the US standard keyboard, the ';:' key
            VK_OEM_PLUS = 0xBB,             //For any country/region, the '+' key
            VK_OEM_COMMA = 0xBC,            //For any country/region, the ',' key
            VK_OEM_MINUS = 0xBD,            //For any country/region, the '-' key
            VK_OEM_PERIOD = 0xBE,           //For any country/region, the '.' key
            VK_OEM_2 = 0xBF,                //For the US standard keyboard, the '/?' key
            VK_OEM_3 = 0xC0,                //For the US standard keyboard, the '`~' key

                                //0xC1-0xD7 Зарезервировано
                                //0xD8-0xDA Не используются
            VK_OEM_4 = 0xDB,    //For the US standard keyboard, the '[{' key
            VK_OEM_5 = 0xDC,    //For the US standard keyboard, the '\|' key
            VK_OEM_6 = 0xDD,    //For the US standard keyboard, the ']}' key
            VK_OEM_7 = 0xDE,    //For the US standard keyboard, the 'single-quote/double-quote' key
            VK_OEM_8 = 0xDF,    //Used for miscellaneous characters; it can vary by keyboard.
                                //0xE0 Зарезервировано
                                //0xE1 OEM specific
            VK_OEM_102 = 0xE2,  //Either the angle bracket key or the backslash key on the RT 102-key keyboard
                                //0xE3-0xE4 OEM specific
                                //0xE5 IME Key
                                //0xE6 OEM specific
            VK_PACKET = 0xE7,   //Used to pass Unicode characters as if they were keystrokes.The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
                                //0xE8 Не используется

            VK_OEM_RESET = 0xE9,    //Only used by Nokia.
            VK_OEM_JUMP = 0xEA,     //Only used by Nokia.
            VK_OEM_PA1 = 0xEB,      //Only used by Nokia.
            VK_OEM_PA2 = 0xEC,      //Only used by Nokia.
            VK_OEM_PA3 = 0xED,      //Only used by Nokia.
            VK_OEM_WSCTRL = 0xEE,   //Only used by Nokia.
            VK_OEM_CUSEL = 0xEF,    //Only used by Nokia.
            VK_OEM_ATTN = 0xF0,     //Only used by Nokia.
            VK_OEM_FINNISH = 0xF1,  //Only used by Nokia.
            VK_OEM_COPY = 0xF2,     //Only used by Nokia.
            VK_OEM_AUTO = 0xF3,     //Only used by Nokia.
            VK_OEM_ENLW = 0xF4,     //Only used by Nokia.
            VK_OEM_BACKTAB = 0xF5,  //Only used by Nokia.

            VK_ATTN = 0xF6,     //Attn key
            VK_CRSEL = 0xF7,    //CrSel key
            VK_EXSEL = 0xF8,    //ExSel key
            VK_EREOF = 0xF9,    //Erase EOF key
            VK_PLAY = 0xFA,     //Play key
            VK_ZOOM = 0xFB,     //Zoom key
            VK_NONAME = 0xFC,   //Reserved for future use.
            VK_PA1 = 0xFD,      //PA1 key
            VK_OEM_CLEAR = 0xFE //Clear key
                                //0xFF Мультимедийные клавиши.См.ScanCode клавиши.
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static bool operator ==(MousePoint a, MousePoint b)
            {
                return a.X == b.X && a.Y == b.Y;
            }
            public static bool operator !=(MousePoint a, MousePoint b)
            {
                return a.X != b.X || a.Y != b.Y;
            }
            public override bool Equals(object b)
            {
                MousePoint bx = (MousePoint)b;
                return X == bx.X && Y == bx.Y;
            }
            public override int GetHashCode()
            {
                return X*Y;
            }

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
            public override string ToString()
            {
                return handle + ", " + msg + ", " + wParam + ", " + lParam + ", " + time + ", " + p;
            }
        }
#pragma warning restore IDE1006 // Стили именования
    }
}
