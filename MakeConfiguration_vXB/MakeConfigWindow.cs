using System;
using System.Diagnostics;
using System.Windows.Forms;
using RawInput_dll;

namespace MakeConfiguration_vXB
{
    public partial class MakeConfigurationWindow : Form
    {
        private readonly RawInput _rawinput;
        const bool CaptureOnlyInForeground = true;
        private const int numberofKeys = 1 + 174;
        private static int isGameMode = 0;

        private static readonly string[] keyArray = { "GAMEMODE", "A", "ADD", "ALT", "APPS", "ATTN", "B", "BACK", "BROWSERBACK", "BROWSERFAVORITES", "BROWSERFORWARD", "BROWSERHOME", "BROWSERREFRESH", "BROWSERSEARCH", "BROWSERSTOP", "C", "CANCEL", "CAPITAL", "CLEAR",
            "CONTROL", "CONTROLKEY", "CRSEL", "D", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DECIMAL", "DELETE", "DIVIDE", "DOWN", "E", "END", "ENTER", "ERASEEOF", "ESCAPE", "EXECUTE", "EXSEL", "F", "F1", "F10", "F11", "F12", "F13", "F14", "F15",
            "F16", "F17", "F18", "F19", "F2", "F20", "F21", "F22", "F23", "F24", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FINALMODE", "G", "H", "HANGUELMODE", "HANJAMODE", "HELP", "HOME", "I", "IMEACEEPT", "IMECONVERT", "IMEMODECHANGE", "IMENONCONVERT", "INSERT",
            "J", "JUNJAMODE", "K", "KEYCODE", "L", "LAUNCHAPPLICATION1", "LAUNCHAPPLICATION2", "LAUNCHMAIL", "LBUTTON", "LCONTROL", "LEFT", "LINEFEED", "LMENU", "LSHIFT", "LWIN", "M", "MBUTTON", "MEDIANEXTTRACK", "MEDIAPLAYPAUSE", "MEDIAPREVIOUSTRACK", "MEDIASTOP",
            "MENU", "MULTIPLY", "N", "NEXT", "NONAME", "NONE", "NUMLOCK", "NUMPAD0", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6", "NUMPAD7", "NUMPAD8", "NUMPAD9", "O", "OEM8", "OEMBACKSLASH", "OEMCLEAR", "OEMCLOSEBRACKETS", "OEMCOMMA", "OEMMINUS",
            "OEMOPENBRACKETS", "OEMPERIOD", "OEMPIPE", "OEMPLUS", "OEMQUESTION", "OEMQUOTES", "OEMSEMICOLON", "OEMTILDE", "P", "PA1", "PAUSE", "PLAY", "PRINT","PRINTSCREEN", "PRIOR", "PROCESSKEY", "Q", "R", "RBUTTON", "RCONTROL", "RIGHT", "RMENU", "RSHIFT", "RWIN",
            "S", "SCROLL", "SELECT", "SELECTMEDIA", "SEPARATOR", "SHIFT", "SHIFTKEY", "SPACE", "SUBTRACT", "T", "TAB", "U", "UP", "V", "VOLUMEDOWN", "VOLUMEMUTE", "VOLUMEUP", "W", "X", "XBUTTON1", "XBUTTON2", "Y", "Z", "ZOOM" };

        private static string[,] controllerKeys = new string[numberofKeys, 2];

        public MakeConfigurationWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _rawinput = new RawInput(Handle, CaptureOnlyInForeground);

            _rawinput.AddMessageFilter();   
 
            _rawinput.KeyPressed += OnKeyPressed;
        }
        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            int keyPosition = SearchKeyPosition(e.KeyPressEvent.VKeyName);
            if (controllerKeys[keyPosition,isGameMode] == null)
            {

            }
        }

        private int SearchKeyPosition(string keyName)
        {
            int ite;
            for (ite = 0; ite < numberofKeys; ite++)
            {
                if (keyName == keyArray[ite])
                    return ite;
            }
            return -1;
        }

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rawinput.KeyPressed -= OnKeyPressed;
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (null == ex) return;
            Debug.WriteLine("Unhandled Exception: " + ex.Message);
            Debug.WriteLine("Unhandled Exception: " + ex);
            MessageBox.Show(ex.Message);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
