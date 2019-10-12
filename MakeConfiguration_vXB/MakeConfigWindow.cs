using System;
using System.Diagnostics;
using System.Windows.Forms;
using RawInput_dll;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace MakeConfiguration_vXB
{
    public partial class MakeConfigurationWindow : Form
    {
        private readonly RawInput _rawinput;
        const bool CaptureOnlyInForeground = true;
        private const int numberofKeys = 1 + 174;
        private static int isGameMode = 0;
        private static bool isRecording = false;
        private static bool isNaming = false;
        private static string controllerKeyToSet = "";
        private static string fileName = "";

        private static readonly string[] keyArray = { "GAMEMODE", "A", "ADD", "ALT", "APPS", "ATTN", "B", "BACK", "BROWSERBACK", "BROWSERFAVORITES", "BROWSERFORWARD", "BROWSERHOME", "BROWSERREFRESH", "BROWSERSEARCH", "BROWSERSTOP", "C", "CANCEL", "CAPITAL", "CLEAR",
            "CONTROL", "CONTROLKEY", "CRSEL", "D", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DECIMAL", "DELETE", "DIVIDE", "DOWN", "E", "END", "ENTER", "ERASEEOF", "ESCAPE", "EXECUTE", "EXSEL", "F", "F1", "F10", "F11", "F12", "F13", "F14", "F15",
            "F16", "F17", "F18", "F19", "F2", "F20", "F21", "F22", "F23", "F24", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FINALMODE", "G", "H", "HANGUELMODE", "HANJAMODE", "HELP", "HOME", "I", "IMEACEEPT", "IMECONVERT", "IMEMODECHANGE", "IMENONCONVERT", "INSERT",
            "J", "JUNJAMODE", "K", "KEYCODE", "L", "LAUNCHAPPLICATION1", "LAUNCHAPPLICATION2", "LAUNCHMAIL", "LBUTTON", "LCONTROL", "LEFT", "LINEFEED", "LMENU", "LSHIFT", "LWIN", "M", "MBUTTON", "MEDIANEXTTRACK", "MEDIAPLAYPAUSE", "MEDIAPREVIOUSTRACK", "MEDIASTOP",
            "MENU", "MULTIPLY", "N", "NEXT", "NONAME", "NONE", "NUMLOCK", "NUMPAD0", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6", "NUMPAD7", "NUMPAD8", "NUMPAD9", "O", "OEM8", "OEMBACKSLASH", "OEMCLEAR", "OEMCLOSEBRACKETS", "OEMCOMMA", "OEMMINUS",
            "OEMOPENBRACKETS", "OEMPERIOD", "OEMPIPE", "OEMPLUS", "OEMQUESTION", "OEMQUOTES", "OEMSEMICOLON", "OEMTILDE", "P", "PA1", "PAUSE", "PLAY", "PRINT","PRINTSCREEN", "PRIOR", "PROCESSKEY", "Q", "R", "RBUTTON", "RCONTROL", "RIGHT", "RMENU", "RSHIFT", "RWIN",
            "S", "SCROLL", "SELECT", "SELECTMEDIA", "SEPARATOR", "SHIFT", "SHIFTKEY", "SPACE", "SUBTRACT", "T", "TAB", "U", "UP", "V", "VOLUMEDOWN", "VOLUMEMUTE", "VOLUMEUP", "W", "X", "XBUTTON1", "XBUTTON2", "Y", "Z", "ZOOM" };

        private static readonly string[] arrayControllerButtonNames = { "A", "B", "X", "Y", "RB", "LB", "Start", "Back", "Right Thumbstick Click", "Left Thumbstick Click", "Right Trigger", "Left Trigger", "D-pad Up", "D-pad Down", "D-pad Left", "D-pad Right",
                                                                        "Left Thumbstick Up", "Left Thumbstick Down", "Left Thumbstick Left", "Left Thumbstick Right", "Right Thumbstick Up", "Right Thumbstick Down", "Right Thumbstick Left", "Right Thumbstick Right" };

        private static StringCollection[,] RecordBook = new StringCollection[24, 2];

        private static string[,] controllerKeys = new string[numberofKeys, 2];

        public MakeConfigurationWindow()
        {
            InitializeComponent();

            Button[] arrayButton = { B_A, B_B, B_X, B_Y, B_LB, B_RB, B_ST, B_BK, B_RC, B_LC, T_LT, T_RT, D_U, D_D, D_L, D_R, S_LU, S_LD, S_LL, S_LR, S_RU, S_RD, S_RL, S_RR };

            for (int i = 0; i < 24; i++)
            {
                arrayButton[i].Click += new EventHandler(SetKey_Click);
                for (int j = 0; j < 2; j++)
                {
                    RecordBook[i, j] = new StringCollection();
                }
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _rawinput = new RawInput(Handle, CaptureOnlyInForeground);

            _rawinput.AddMessageFilter();

            _rawinput.KeyPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            string recordedVertualKey = e.KeyPressEvent.VKeyName;

            if ((isNaming) && (e.KeyPressEvent.KeyPressState == "MAKE"))
            {
                if (SetFileName(recordedVertualKey))
                {
                    if (fileName != "")
                        buttonName.Text = fileName;
                    else
                        buttonName.Text = "Set Configuration Name";
                    buttonName.BackColor = Control.DefaultBackColor;
                    labelMessage.Text = "";
                    isNaming = false;
                }
                else
                    buttonName.Text = fileName + "_";
            }
            else if ((isRecording) && (e.KeyPressEvent.KeyPressState != "MAKE"))
            {
                isRecording = false;
                labelMessage.Text = "";
                int keyPosition = SearchKeyPosition(recordedVertualKey);

                if (controllerKeyToSet == "keyDelete")
                {
                    ClearKey(keyPosition, recordedVertualKey);
                    UpdateState();
                }
                else if (((recordedVertualKey == controllerKeys[0, 0]) || (recordedVertualKey == controllerKeys[0, 1])) || (controllerKeys[keyPosition, isGameMode] != null))
                {
                    if (WarningResponse("You already assigned this key to elsewhere. Do you want to keep the new setting?", "Duplicate Key"))
                    {
                        ClearKey(keyPosition, recordedVertualKey);
                        AssignKey(keyPosition, recordedVertualKey);
                    }
                }
                else
                    AssignKey(keyPosition, recordedVertualKey);
            }


        }

        private bool WarningResponse(string caption, string title)
        {
            DialogResult result = MessageBox.Show(caption, title, MessageBoxButtons.YesNo);
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
                RecordBook[SearchButtonPosition(controllerKeyToSet), isGameMode].Add(recordedVertualKey);
            }
            UpdateState();
        }

        private void ClearKey(int keyPosition, string recordedVertualKey)
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
                RecordBook[SearchButtonPosition(controllerKeys[keyPosition, isGameMode]), isGameMode].Remove(recordedVertualKey);
                controllerKeys[keyPosition, isGameMode] = null;
            }

        }

        private int SearchKeyPosition(string keyName)
        {
            switch (keyName)
            {
                case "A": return 1;
                case "ADD": return 2;
                case "ALT": return 3;
                case "APPS": return 4;
                case "ATTN": return 5;
                case "B": return 6;
                case "BACK": return 7;
                case "BROWSERBACK": return 8;
                case "BROWSERFAVORITES": return 9;
                case "BROWSERFORWARD": return 10;
                case "BROWSERHOME": return 11;
                case "BROWSERREFRESH": return 12;
                case "BROWSERSEARCH": return 13;
                case "BROWSERSTOP": return 14;
                case "C": return 15;
                case "CANCEL": return 16;
                case "CAPITAL": return 17;
                case "CLEAR": return 18;
                case "CONTROL": return 19;
                case "CONTROLKEY": return 20;
                case "CRSEL": return 21;
                case "D": return 22;
                case "D0": return 23;
                case "D1": return 24;
                case "D2": return 25;
                case "D3": return 26;
                case "D4": return 27;
                case "D5": return 28;
                case "D6": return 29;
                case "D7": return 30;
                case "D8": return 31;
                case "D9": return 32;
                case "DECIMAL": return 33;
                case "DELETE": return 34;
                case "DIVIDE": return 35;
                case "DOWN": return 36;
                case "E": return 37;
                case "END": return 38;
                case "ENTER": return 39;
                case "ERASEEOF": return 40;
                case "ESCAPE": return 41;
                case "EXECUTE": return 42;
                case "EXSEL": return 43;
                case "F": return 44;
                case "F1": return 45;
                case "F10": return 46;
                case "F11": return 47;
                case "F12": return 48;
                case "F13": return 49;
                case "F14": return 50;
                case "F15": return 51;
                case "F16": return 52;
                case "F17": return 53;
                case "F18": return 54;
                case "F19": return 55;
                case "F2": return 56;
                case "F20": return 57;
                case "F21": return 58;
                case "F22": return 59;
                case "F23": return 60;
                case "F24": return 61;
                case "F3": return 62;
                case "F4": return 63;
                case "F5": return 64;
                case "F6": return 65;
                case "F7": return 66;
                case "F8": return 67;
                case "F9": return 68;
                case "FINALMODE": return 69;
                case "G": return 70;
                case "H": return 71;
                case "HANGUELMODE": return 72;
                case "HANJAMODE": return 73;
                case "HELP": return 74;
                case "HOME": return 75;
                case "I": return 76;
                case "IMEACEEPT": return 77;
                case "IMECONVERT": return 78;
                case "IMEMODECHANGE": return 79;
                case "IMENONCONVERT": return 80;
                case "INSERT": return 81;
                case "J": return 82;
                case "JUNJAMODE": return 83;
                case "K": return 84;
                case "KEYCODE": return 85;
                case "L": return 86;
                case "LAUNCHAPPLICATION1": return 87;
                case "LAUNCHAPPLICATION2": return 88;
                case "LAUNCHMAIL": return 89;
                case "LBUTTON": return 90;
                case "LCONTROL": return 91;
                case "LEFT": return 92;
                case "LINEFEED": return 93;
                case "LMENU": return 94;
                case "LSHIFT": return 95;
                case "LWIN": return 96;
                case "M": return 97;
                case "MBUTTON": return 98;
                case "MEDIANEXTTRACK": return 99;
                case "MEDIAPLAYPAUSE": return 100;
                case "MEDIAPREVIOUSTRACK": return 101;
                case "MEDIASTOP": return 102;
                case "MENU": return 103;
                case "MULTIPLY": return 104;
                case "N": return 105;
                case "NEXT": return 106;
                case "NONAME": return 107;
                case "NONE": return 108;
                case "NUMLOCK": return 109;
                case "NUMPAD0": return 110;
                case "NUMPAD1": return 111;
                case "NUMPAD2": return 112;
                case "NUMPAD3": return 113;
                case "NUMPAD4": return 114;
                case "NUMPAD5": return 115;
                case "NUMPAD6": return 116;
                case "NUMPAD7": return 117;
                case "NUMPAD8": return 118;
                case "NUMPAD9": return 119;
                case "O": return 120;
                case "OEM8": return 121;
                case "OEMBACKSLASH": return 122;
                case "OEMCLEAR": return 123;
                case "OEMCLOSEBRACKETS": return 124;
                case "OEMCOMMA": return 125;
                case "OEMMINUS": return 126;
                case "OEMOPENBRACKETS": return 127;
                case "OEMPERIOD": return 128;
                case "OEMPIPE": return 129;
                case "OEMPLUS": return 130;
                case "OEMQUESTION": return 131;
                case "OEMQUOTES": return 132;
                case "OEMSEMICOLON": return 133;
                case "OEMTILDE": return 134;
                case "P": return 135;
                case "PA1": return 136;
                case "PAUSE": return 137;
                case "PLAY": return 138;
                case "PRINT": return 139;
                case "PRINTSCREEN": return 140;
                case "PRIOR": return 141;
                case "PROCESSKEY": return 142;
                case "Q": return 143;
                case "R": return 144;
                case "RBUTTON": return 145;
                case "RCONTROL": return 146;
                case "RIGHT": return 147;
                case "RMENU": return 148;
                case "RSHIFT": return 149;
                case "RWIN": return 150;
                case "S": return 151;
                case "SCROLL": return 152;
                case "SELECT": return 153;
                case "SELECTMEDIA": return 154;
                case "SEPARATOR": return 155;
                case "SHIFT": return 156;
                case "SHIFTKEY": return 157;
                case "SPACE": return 158;
                case "SUBTRACT": return 159;
                case "T": return 160;
                case "TAB": return 161;
                case "U": return 162;
                case "UP": return 163;
                case "V": return 164;
                case "VOLUMEDOWN": return 165;
                case "VOLUMEMUTE": return 166;
                case "VOLUMEUP": return 167;
                case "W": return 168;
                case "X": return 169;
                case "XBUTTON1": return 170;
                case "XBUTTON2": return 171;
                case "Y": return 172;
                case "Z": return 173;
                case "ZOOM": return 174;

                default: return -1;
            }

        }

        private int SearchButtonPosition(string btnName)
        {
            switch (btnName)
            {
                case "B_A": return 0;
                case "B_B": return 1;
                case "B_X": return 2;
                case "B_Y": return 3;
                case "B_RB": return 4;
                case "B_LB": return 5;
                case "B_ST": return 6;
                case "B_BK": return 7;
                case "B_RC": return 8;
                case "B_LC": return 9;
                case "T_RT": return 10;
                case "T_LT": return 11;
                case "D_U": return 12;
                case "D_D": return 13;
                case "D_L": return 14;
                case "D_R": return 15;
                case "S_LU": return 16;
                case "S_LD": return 17;
                case "S_LL": return 18;
                case "S_LR": return 19;
                case "S_RU": return 20;
                case "S_RD": return 21;
                case "S_RL": return 22;
                case "S_RR": return 23;
                default: return -1;
            }
        }

        private void UpdateState()
        {
            System.Text.StringBuilder textToShow = new System.Text.StringBuilder("");

            for (int i = 0; i < 2; i++)
            {
                textToShow.Append("Mode ");
                textToShow.Append(i + 1);
                if (controllerKeys[0, i] != null)
                {
                    textToShow.Append("   <-   ");
                    textToShow.Append(controllerKeys[0, i]);
                }
                textToShow.Append(Environment.NewLine);
            }

            for (int i = 0; i < 24; i++)
            {
                textToShow.Append(Environment.NewLine);
                textToShow.Append(arrayControllerButtonNames[i]);
                if (RecordBook[i, isGameMode].Count != 0)
                {
                    textToShow.Append("   <-");
                    foreach (Object obj in RecordBook[i, isGameMode])
                    {
                        textToShow.Append("  ");
                        textToShow.Append(obj.ToString());
                    }
                }
            }
            textBoxResponse.Text = textToShow.ToString();
        }

        private bool SetFileName(string key)
        {
            if (key == "ENTER")
                return true;
            else
            {
                switch (key)
                {
                    case "A":
                    case "B":
                    case "C":
                    case "D":
                    case "E":
                    case "F":
                    case "G":
                    case "H":
                    case "I":
                    case "J":
                    case "K":
                    case "L":
                    case "M":
                    case "N":
                    case "O":
                    case "P":
                    case "Q":
                    case "R":
                    case "S":
                    case "T":
                    case "U":
                    case "V":
                    case "W":
                    case "X":
                    case "Y":
                    case "Z":
                    case "D0":
                    case "D1":
                    case "D2":
                    case "D3":
                    case "D4":
                    case "D5":
                    case "D6":
                    case "D7":
                    case "D8":
                    case "D9":
                    case "NUMPAD0":
                    case "NUMPAD1":
                    case "NUMPAD2":
                    case "NUMPAD3":
                    case "NUMPAD4":
                    case "NUMPAD5":
                    case "NUMPAD6":
                    case "NUMPAD7":
                    case "NUMPAD8":
                    case "NUMPAD9":
                        fileName = fileName + key[key.Length - 1];
                        break;
                    case "BACK":
                        if (fileName.Length != 0)
                            fileName = fileName.Substring(0, fileName.Length - 1);
                        break;
                    default:
                        break;
                }
                return false;
            }
        }

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rawinput.KeyPressed -= OnKeyPressed;
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (null == ex) return;
            Debug.WriteLine("Unhandled Exception (Config): " + ex.Message);
            Debug.WriteLine("Unhandled Exception (Config): " + ex);
            MessageBox.Show(ex.Message);
        }

        private void buttonMode1_Click(object sender, EventArgs e)
        {
            isGameMode = 0;
            ((Button)(sender)).BackColor = System.Drawing.Color.Lime;
            ((Button)buttonMode2).BackColor = Control.DefaultBackColor;
            UpdateState();
        }

        private void buttonMode2_Click(object sender, EventArgs e)
        {
            isGameMode = 1;
            ((Button)(sender)).BackColor = System.Drawing.Color.Lime;
            ((Button)buttonMode1).BackColor = Control.DefaultBackColor;
            UpdateState();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "keyDelete";
            labelMessage.Text = "Press the key to clear...";
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            bool writeFile = false;
            if (fileName.Length != 0)
            {
                if (File.Exists(fileName))
                {
                    if (WarningResponse("Configuration file with same name exists. Do you want to replace it?", "Duplicate Name"))
                        writeFile = true;
                }
                else
                    writeFile = true;
            }

            if (writeFile)
            {
                if (controllerKeys[0, 0] != null)
                {
                    controllerKeys[SearchKeyPosition(controllerKeys[0, 0]), 0] = "MODE1";
                    controllerKeys[SearchKeyPosition(controllerKeys[0, 0]), 1] = "MODE1";
                }
                else
                {
                    controllerKeys[0, 0] = "MODE1";
                }
                if (controllerKeys[0, 1] != null)
                {
                    controllerKeys[SearchKeyPosition(controllerKeys[0, 1]), 0] = "MODE2";
                    controllerKeys[SearchKeyPosition(controllerKeys[0, 1]), 1] = "MODE2";
                }
                else
                {
                    controllerKeys[0, 1] = "MODE2";
                }


                using (StreamWriter outputFile = new StreamWriter(fileName))
                {

                    for (int i = 0; i < numberofKeys; i++)
                    {
                        string line = keyArray[i];
                        for (int j = 0; j < 2; j++)
                        {
                            if (controllerKeys[i, j] == null)
                                line = line + " --";
                            else line = line + " " + controllerKeys[i, j];
                        }

                        outputFile.WriteLine(line);
                    }
                }
                labelMessage.Text = "Close the application...";
                Application.Exit();
            }
            else
            {
                buttonName.BackColor = System.Drawing.Color.Red;
            }
        }

        private void MODE1_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "MODE1";
            labelMessage.Text = "Recording... Press your desired key.";
        }

        private void MODE2_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = "MODE2";
            labelMessage.Text = "Recording... Press your desired key.";
        }

        private void SetKey_Click(object sender, EventArgs e)
        {
            isRecording = true;
            controllerKeyToSet = ((Button)(sender)).Name.ToString();
            labelMessage.Text = "Recording... Press your desired key.";
        }

        private void buttonName_Click(object sender, EventArgs e)
        {
            isNaming = true;
            labelMessage.Text = "Type this configuration name. Only 0-9 and A-Z allowed.";
            buttonName.BackColor = System.Drawing.Color.White;
            buttonName.Text = fileName + "_";
        }
               

    }
}
