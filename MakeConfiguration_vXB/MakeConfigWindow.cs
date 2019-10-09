using System;
using System.Diagnostics;
using System.Windows.Forms;
using RawInput_dll;
using System.IO;

namespace MakeConfiguration_vXB
{
    public partial class MakeConfigurationWindow : Form
    {
        private readonly RawInput _rawinput;
        const bool CaptureOnlyInForeground = true;
        private const int numberofKeys = 1 + 174;
        private static int isGameMode = 0;
        private static bool isRecording = false;
        private static string controllerKeyToSet = "";

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

            Button[] arrayButton = { B_A,B_B,B_X,B_Y,B_LB,B_RB,B_ST,B_BK,B_RC,B_LC,T_LT,T_RT,D_U,D_D,D_L,D_R,S_LU,S_LD,S_LL,S_LR,S_RU,S_RD,S_RL,S_RR };

            for (int i = 0; i < 24; i++)
            {
                arrayButton[i].Click += new EventHandler(SetKey_Click);
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _rawinput = new RawInput(Handle, CaptureOnlyInForeground);

            _rawinput.AddMessageFilter();   
 
            _rawinput.KeyPressed += OnKeyPressed;
        }


        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            if (isRecording)
            {
                string recordedVertualKey = e.KeyPressEvent.VKeyName;
                int keyPosition = SearchKeyPosition(recordedVertualKey);

                if (controllerKeyToSet == "keyDelete")
                {
                    ClearKey(keyPosition, recordedVertualKey);
                }
                else if (((recordedVertualKey == controllerKeys[0, 0])|| (recordedVertualKey == controllerKeys[0, 1])) || (controllerKeys[keyPosition, isGameMode] != null))
                {
                    if(warningResponse())
                    {
                        ClearKey(keyPosition, recordedVertualKey);
                        AssignKey(keyPosition, recordedVertualKey);
                    }
                }
                else
                    AssignKey(keyPosition, recordedVertualKey);
            }
            isRecording = false;
        }

        private bool warningResponse()
        {
            DialogResult result = MessageBox.Show("You already assigned this key to elsewhere. Do you want to keep the new setting?", "Duplicate Key", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
                return true;
            else
                return false;
        }   

        private void AssignKey(int keyPosition, string recordedVertualKey)
        {
            if (controllerKeyToSet == "MODE1")
            {
                controllerKeys[0, 0] = recordedVertualKey;
            }
            else if (controllerKeyToSet == "MODE2")
            {
                controllerKeys[0, 1] = recordedVertualKey;
            }
            else
            {
                controllerKeys[keyPosition, isGameMode] = controllerKeyToSet;
            }
        }

        private void ClearKey(int keyPosition,string recordedVertualKey)
        {
            if (recordedVertualKey == controllerKeys[0, 0])
            {
                controllerKeys[0, 0] = null;
            }
            else if (recordedVertualKey == controllerKeys[0, 1])
            {
                controllerKeys[0, 1] = null;
            }
            else
            {
                controllerKeys[keyPosition, isGameMode] = null;
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
            DialogResult result = MessageBox.Show("Are you sure that you would like to close the form?", "Form Closing", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
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

        private void textBoxConfiruration_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = System.Text.RegularExpressions.Regex.Replace(textboxSender.Text, "[^0-9a-zA-Z]", "");
            textboxSender.SelectionStart = cursorPosition;
        }

        private void buttonMode1_Click(object sender, EventArgs e)
        {
            isGameMode = 0;
            ((Button)(sender)).BackColor = System.Drawing.Color.Lime;
        }

        private void buttonMode2_Click(object sender, EventArgs e)
        {
            isGameMode = 1;
            ((Button)(sender)).BackColor = System.Drawing.Color.Lime;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "keyDelete";
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            
        }

        private void MODE1_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "MODE1";
        }

        private void MODE2_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "MODE2";
        }

        private void SetKey_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = ((Button)(sender)).Name.ToString();
        }

    }
}
