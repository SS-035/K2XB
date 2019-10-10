using System;
using RawInput_dll;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace AppK2J
{
    public partial class MainWindow : Form
    {
        #region "Used Variables"
        //Rest positions are 0
        private const short AxisMaxima = 32767;
        private const byte TriggerMaxima = 255;

        //Number of keys to be mapped
        private const int numberofKeys = 1+174;
        private static string[ , ] keyConfiguration = new string[numberofKeys, 2];

        //Buton and axis IDs
        private const uint btnA = 1;
        private const uint btnB = 2;
        private const uint btnX = 3;
        private const uint btnY = 4;
        private const uint btnLB = 5;
        private const uint btnRB = 6;
        private const uint btnBK = 7;
        private const uint btnST = 8;
        private const uint btnLT = 9;
        private const uint btnRT = 10;

        private const float dpadU = 0.0f;
        private const float dpadR = 90.0f;
        private const float dpadD = 180.0f;
        private const float dpadL = 270.0f;

        private const uint stkLX = 1;
        private const uint stkLY = 2;
        private const uint stkRX = 3;
        private const uint stkRY = 4;

        //Stick status [no of axis, -ve and +ve]
        private static bool[,] stkStatus = new bool [4,2];

        private const uint trgLT = 5;
        private const uint trgRT = 6;

        //Forced Shut down
        private static bool forceSD = false;

        //Counter variables for First Run
        private static string firstKeyboard = "$&%#";
        private static int nRun = 0;

        // Declaring one joystick (Device vxbID 1)
        static public uint vxbID = 1;

        //GAME Mode
        private static int isGameMode = 2;
        private static bool isDebug = false;
        private static bool universalController = false;

        //LED number of virtual xBox //Does not work
        //private static byte Led = 0xFF;
        #endregion

        private readonly RawInput _rawinput;


        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            #region "Set up RawInput & vXbox"
            _rawinput = new RawInput(Handle, false);

            _rawinput.AddMessageFilter();   // Adding a message filter will cause keypresses to be handled

            _rawinput.KeyPressed += OnKeyPressed;

            // Check if vBus exists?
            if (isVBusExists() == false)
            {
                LogNExit("vBus for xBox controller not enabled...\n");
            }
            else
                LogW("vBus found...\n");

            //Check for avaiable devices
            bool findID = true;
            for (vxbID = 1; vxbID <= 4; vxbID++)
            {
                findID = isControllerExists(vxbID);
                //LogW("Device vxbID : " + vxbID + "  ( " + findID + " )\n");
                if (findID == false)
                    break;
            }
            if (findID == true)
                LogNExit("No available vJoy Device found :( \n");

            // Acquire the target
            if (PlugIn(vxbID) != true)
                LogNExit("Failed to acquire vJoy device\n");
            else
            {
                //GetLedNumber(vxbID, out Led);
                LogW("Acquired :: vXbox vxbID : " + vxbID.ToString() + "\n");
            }

            this.Text = " :: K2J :: vxbID " + vxbID.ToString() + " :: Blank";

            if (forceSD == true)
            {
                LogW("Close this application");
            }

            Btn1.Text = "One-to-One Controller";
            Btn2.Text = "Usiversal Controller";

            #endregion
        }

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            //Forced exit on unsuccessful vXbox setup
            if (forceSD == true) return;

            if (e.KeyPressEvent.DeviceType == "KEYBOARD")
            {
                //Add Keyboard and configuration in first run
                if (nRun == 1)
                {
                    //Set the first keyboard as the input
                    if (e.KeyPressEvent.KeyPressState == "MAKE")
                    {
                        if (universalController != true)
                        {
                            firstKeyboard = e.KeyPressEvent.DeviceHandle.ToString();
                            LogW("Connected to the Keyboad : " + firstKeyboard + "\n");
                        }

                    }
                    else
                    {
                        //Prevent running twice while in ofd
                        nRun = 0;

                        #region "Load Configuration"

                        LogW("Select a Configuration file \n");

                        OpenFileDialog ofd = new OpenFileDialog();
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            //load the configuration
                            string line;
                            StreamReader file;
                            file = new StreamReader(ofd.FileName);

                            try
                            {
                                for (int i = 0; i < numberofKeys; i++)
                                {
                                    line = file.ReadLine();
                                    string[] words_ = line.Split(' ');
                                    keyConfiguration[i,0] = words_[1];
                                    keyConfiguration[i,1] = words_[2];
                                }
                                LogW("Conguration Loaded successfully.\n");
                            }
                            catch
                            {
                                LogNExit("Error in Configuration file \n");
                            }
                            this.Text = " :: K2J :: vxbID " + vxbID.ToString() + " :: "+ Path.GetFileNameWithoutExtension(ofd.FileName);
                        }
                        else
                            LogNExit("No file selected.. \n");

                        #endregion

                        //Changed to GameMode button
                        Btn1.Text = "Mode 1 <" + keyConfiguration[0, 0] + ">";
                        Btn2.Text = "Mode 2 <" + keyConfiguration[0, 1] + ">";

                        //set nRun = 2 and send to next step
                        nRun = 2;
                    }

                }
                else if ((e.KeyPressEvent.DeviceHandle.ToString() == firstKeyboard)||(universalController==true))
                {
                    ExecuteCalls(e.KeyPressEvent.VKeyName, e.KeyPressEvent.KeyPressState);
                }
            }
        }

        #region "Customs calls execution"
        private void ExecuteCalls (string vKey,string keyState)
        {
            //Virtual Key name and state
            string command = UserConfig(vKey);

            if (isDebug == true)
                LogW(vKey + ((keyState == "MAKE") ? " (D)" : " (U)") + "\t" + command + "\n");

            switch (command)
            {
                //Buttons
                case "B_Y":
                    BtnPress(btnY, keyState);
                    break;
                case "B_X":
                    BtnPress(btnX, keyState);
                    break;
                case "B_B":
                    BtnPress(btnB, keyState);
                    break;
                case "B_A":
                    BtnPress(btnA, keyState);
                    break;
                case "B_LB":
                    BtnPress(btnLB, keyState);
                    break;
                case "B_RB":
                    BtnPress(btnRB, keyState);
                    break;
                case "B_ST":
                    BtnPress(btnST, keyState);
                    break;
                case "B_BK":
                    BtnPress(btnBK, keyState);
                    break;
                case "B_LC":
                    BtnPress(btnLT, keyState);
                    break;
                case "B_RC":
                    BtnPress(btnRT, keyState);
                    break;

                //D-pad
                case "D_U":
                    DpadPress(dpadU, keyState);
                    break;
                case "D_L":
                    DpadPress(dpadL, keyState);
                    break;
                case "D_D":
                    DpadPress(dpadD, keyState);
                    break;
                case "D_R":
                    DpadPress(dpadR, keyState);
                    break;

                //Trigger
                case "T_LT":
                    TriggerPress(keyState, trgLT);
                    break;
                case "T_RT":
                    TriggerPress(keyState, trgRT);
                    break;

                //Sticks
                case "S_LU":
                    StickMove(stkLY, AxisMaxima, keyState);
                    break;

                case "S_LD":
                    StickMove(stkLY, -AxisMaxima, keyState);
                    break;

                case "S_LL":
                    StickMove(stkLX, -AxisMaxima, keyState);
                    break;

                case "S_LR":
                    StickMove(stkLX, AxisMaxima, keyState);
                    break;

                case "S_RU":
                    StickMove(stkRY, AxisMaxima, keyState);
                    break;

                case "S_RD":
                    StickMove(stkRY, -AxisMaxima, keyState);
                    break;

                case "S_RL":
                    StickMove(stkRX, -AxisMaxima, keyState);
                    break;

                case "S_RR":
                    StickMove(stkRX, AxisMaxima, keyState);
                    break;

                //Extra Mode Function
                case "DEBUG":
                    if (keyState != "MAKE")
                        isDebug = !isDebug;
                    break;

                case "MODE1":
                    if (keyState != "MAKE")
                    {
                        if ((isGameMode == 1) || (isGameMode == 2))
                        {
                            isGameMode = 0;
                            Btn1.BackColor = System.Drawing.Color.Lime;
                            Btn2.BackColor = Control.DefaultBackColor;
                        }
                        else
                        {
                            isGameMode = 2;
                            Btn1.BackColor = Control.DefaultBackColor;
                            Btn2.BackColor = Control.DefaultBackColor;
                        }
                    }
                    break;

                case "MODE2":
                    if (keyState != "MAKE")
                    {
                        if ((isGameMode == 0) || (isGameMode == 2))
                        {
                            isGameMode = 1;
                            Btn1.BackColor = Control.DefaultBackColor;
                            Btn2.BackColor = System.Drawing.Color.Lime;
                        }
                        else
                        {
                            isGameMode = 2;
                            Btn1.BackColor = Control.DefaultBackColor;
                            Btn2.BackColor = Control.DefaultBackColor;
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region "Virtual Key to custom calls (userConfig)"
        private static string UserConfig(string _e)
        {
            //Naming COnvension in the configuration file
            //B_ ** = Button;  ** = A/B/X/Y/RB/LB/ST(start)/BK(back)/RC(right stick click)/LC(left stick click)
            //S_ ** = Stick Up/Down/Left/Right; ** = LU(stands for Letf stick up and so on)/LL/LD/LR/RU/RL/RD/RR
            //T_** = Trigger; ** = RT/LT 
            //D_** = D-pad_Switch; **=U/L/D/R

            if (isGameMode != 2)
            {
                switch (_e)
                {
                    case "A": return keyConfiguration[1, isGameMode];
                    case "ADD": return keyConfiguration[2, isGameMode];
                    case "ALT": return keyConfiguration[3, isGameMode];
                    case "APPS": return keyConfiguration[4, isGameMode];
                    case "ATTN": return keyConfiguration[5, isGameMode];
                    case "B": return keyConfiguration[6, isGameMode];
                    case "BACK": return keyConfiguration[7, isGameMode];
                    case "BROWSERBACK": return keyConfiguration[8, isGameMode];
                    case "BROWSERFAVORITES": return keyConfiguration[9, isGameMode];
                    case "BROWSERFORWARD": return keyConfiguration[10, isGameMode];
                    case "BROWSERHOME": return keyConfiguration[11, isGameMode];
                    case "BROWSERREFRESH": return keyConfiguration[12, isGameMode];
                    case "BROWSERSEARCH": return keyConfiguration[13, isGameMode];
                    case "BROWSERSTOP": return keyConfiguration[14, isGameMode];
                    case "C": return keyConfiguration[15, isGameMode];
                    case "CANCEL": return keyConfiguration[16, isGameMode];
                    case "CAPITAL": return keyConfiguration[17, isGameMode];
                    case "CLEAR": return keyConfiguration[18, isGameMode];
                    case "CONTROL": return keyConfiguration[19, isGameMode];
                    case "CONTROLKEY": return keyConfiguration[20, isGameMode];
                    case "CRSEL": return keyConfiguration[21, isGameMode];
                    case "D": return keyConfiguration[22, isGameMode];
                    case "D0": return keyConfiguration[23, isGameMode];
                    case "D1": return keyConfiguration[24, isGameMode];
                    case "D2": return keyConfiguration[25, isGameMode];
                    case "D3": return keyConfiguration[26, isGameMode];
                    case "D4": return keyConfiguration[27, isGameMode];
                    case "D5": return keyConfiguration[28, isGameMode];
                    case "D6": return keyConfiguration[29, isGameMode];
                    case "D7": return keyConfiguration[30, isGameMode];
                    case "D8": return keyConfiguration[31, isGameMode];
                    case "D9": return keyConfiguration[32, isGameMode];
                    case "DECIMAL": return keyConfiguration[33, isGameMode];
                    case "DELETE": return keyConfiguration[34, isGameMode];
                    case "DIVIDE": return keyConfiguration[35, isGameMode];
                    case "DOWN": return keyConfiguration[36, isGameMode];
                    case "E": return keyConfiguration[37, isGameMode];
                    case "END": return keyConfiguration[38, isGameMode];
                    case "ENTER": return keyConfiguration[39, isGameMode];
                    case "ERASEEOF": return keyConfiguration[40, isGameMode];
                    case "ESCAPE": return keyConfiguration[41, isGameMode];
                    case "EXECUTE": return keyConfiguration[42, isGameMode];
                    case "EXSEL": return keyConfiguration[43, isGameMode];
                    case "F": return keyConfiguration[44, isGameMode];
                    case "F1": return keyConfiguration[45, isGameMode];
                    case "F10": return keyConfiguration[46, isGameMode];
                    case "F11": return keyConfiguration[47, isGameMode];
                    case "F12": return keyConfiguration[48, isGameMode];
                    case "F13": return keyConfiguration[49, isGameMode];
                    case "F14": return keyConfiguration[50, isGameMode];
                    case "F15": return keyConfiguration[51, isGameMode];
                    case "F16": return keyConfiguration[52, isGameMode];
                    case "F17": return keyConfiguration[53, isGameMode];
                    case "F18": return keyConfiguration[54, isGameMode];
                    case "F19": return keyConfiguration[55, isGameMode];
                    case "F2": return keyConfiguration[56, isGameMode];
                    case "F20": return keyConfiguration[57, isGameMode];
                    case "F21": return keyConfiguration[58, isGameMode];
                    case "F22": return keyConfiguration[59, isGameMode];
                    case "F23": return keyConfiguration[60, isGameMode];
                    case "F24": return keyConfiguration[61, isGameMode];
                    case "F3": return keyConfiguration[62, isGameMode];
                    case "F4": return keyConfiguration[63, isGameMode];
                    case "F5": return keyConfiguration[64, isGameMode];
                    case "F6": return keyConfiguration[65, isGameMode];
                    case "F7": return keyConfiguration[66, isGameMode];
                    case "F8": return keyConfiguration[67, isGameMode];
                    case "F9": return keyConfiguration[68, isGameMode];
                    case "FINALMODE": return keyConfiguration[69, isGameMode];
                    case "G": return keyConfiguration[70, isGameMode];
                    case "H": return keyConfiguration[71, isGameMode];
                    case "HANGUELMODE": return keyConfiguration[72, isGameMode];
                    case "HANJAMODE": return keyConfiguration[73, isGameMode];
                    case "HELP": return keyConfiguration[74, isGameMode];
                    case "HOME": return keyConfiguration[75, isGameMode];
                    case "I": return keyConfiguration[76, isGameMode];
                    case "IMEACEEPT": return keyConfiguration[77, isGameMode];
                    case "IMECONVERT": return keyConfiguration[78, isGameMode];
                    case "IMEMODECHANGE": return keyConfiguration[79, isGameMode];
                    case "IMENONCONVERT": return keyConfiguration[80, isGameMode];
                    case "INSERT": return keyConfiguration[81, isGameMode];
                    case "J": return keyConfiguration[82, isGameMode];
                    case "JUNJAMODE": return keyConfiguration[83, isGameMode];
                    case "K": return keyConfiguration[84, isGameMode];
                    case "KEYCODE": return keyConfiguration[85, isGameMode];
                    case "L": return keyConfiguration[86, isGameMode];
                    case "LAUNCHAPPLICATION1": return keyConfiguration[87, isGameMode];
                    case "LAUNCHAPPLICATION2": return keyConfiguration[88, isGameMode];
                    case "LAUNCHMAIL": return keyConfiguration[89, isGameMode];
                    case "LBUTTON": return keyConfiguration[90, isGameMode];
                    case "LCONTROL": return keyConfiguration[91, isGameMode];
                    case "LEFT": return keyConfiguration[92, isGameMode];
                    case "LINEFEED": return keyConfiguration[93, isGameMode];
                    case "LMENU": return keyConfiguration[94, isGameMode];
                    case "LSHIFT": return keyConfiguration[95, isGameMode];
                    case "LWIN": return keyConfiguration[96, isGameMode];
                    case "M": return keyConfiguration[97, isGameMode];
                    case "MBUTTON": return keyConfiguration[98, isGameMode];
                    case "MEDIANEXTTRACK": return keyConfiguration[99, isGameMode];
                    case "MEDIAPLAYPAUSE": return keyConfiguration[100, isGameMode];
                    case "MEDIAPREVIOUSTRACK": return keyConfiguration[101, isGameMode];
                    case "MEDIASTOP": return keyConfiguration[102, isGameMode];
                    case "MENU": return keyConfiguration[103, isGameMode];
                    case "MULTIPLY": return keyConfiguration[104, isGameMode];
                    case "N": return keyConfiguration[105, isGameMode];
                    case "NEXT": return keyConfiguration[106, isGameMode];
                    case "NONAME": return keyConfiguration[107, isGameMode];
                    case "NONE": return keyConfiguration[108, isGameMode];
                    case "NUMLOCK": return keyConfiguration[109, isGameMode];
                    case "NUMPAD0": return keyConfiguration[110, isGameMode];
                    case "NUMPAD1": return keyConfiguration[111, isGameMode];
                    case "NUMPAD2": return keyConfiguration[112, isGameMode];
                    case "NUMPAD3": return keyConfiguration[113, isGameMode];
                    case "NUMPAD4": return keyConfiguration[114, isGameMode];
                    case "NUMPAD5": return keyConfiguration[115, isGameMode];
                    case "NUMPAD6": return keyConfiguration[116, isGameMode];
                    case "NUMPAD7": return keyConfiguration[117, isGameMode];
                    case "NUMPAD8": return keyConfiguration[118, isGameMode];
                    case "NUMPAD9": return keyConfiguration[119, isGameMode];
                    case "O": return keyConfiguration[120, isGameMode];
                    case "OEM8": return keyConfiguration[121, isGameMode];
                    case "OEMBACKSLASH": return keyConfiguration[122, isGameMode];
                    case "OEMCLEAR": return keyConfiguration[123, isGameMode];
                    case "OEMCLOSEBRACKETS": return keyConfiguration[124, isGameMode];
                    case "OEMCOMMA": return keyConfiguration[125, isGameMode];
                    case "OEMMINUS": return keyConfiguration[126, isGameMode];
                    case "OEMOPENBRACKETS": return keyConfiguration[127, isGameMode];
                    case "OEMPERIOD": return keyConfiguration[128, isGameMode];
                    case "OEMPIPE": return keyConfiguration[129, isGameMode];
                    case "OEMPLUS": return keyConfiguration[130, isGameMode];
                    case "OEMQUESTION": return keyConfiguration[131, isGameMode];
                    case "OEMQUOTES": return keyConfiguration[132, isGameMode];
                    case "OEMSEMICOLON": return keyConfiguration[133, isGameMode];
                    case "OEMTILDE": return keyConfiguration[134, isGameMode];
                    case "P": return keyConfiguration[135, isGameMode];
                    case "PA1": return keyConfiguration[136, isGameMode];
                    case "PAUSE": return keyConfiguration[137, isGameMode];
                    case "PLAY": return keyConfiguration[138, isGameMode];
                    case "PRINT": return keyConfiguration[139, isGameMode];
                    case "PRINTSCREEN": return keyConfiguration[140, isGameMode];
                    case "PRIOR": return keyConfiguration[141, isGameMode];
                    case "PROCESSKEY": return keyConfiguration[142, isGameMode];
                    case "Q": return keyConfiguration[143, isGameMode];
                    case "R": return keyConfiguration[144, isGameMode];
                    case "RBUTTON": return keyConfiguration[145, isGameMode];
                    case "RCONTROL": return keyConfiguration[146, isGameMode];
                    case "RIGHT": return keyConfiguration[147, isGameMode];
                    case "RMENU": return keyConfiguration[148, isGameMode];
                    case "RSHIFT": return keyConfiguration[149, isGameMode];
                    case "RWIN": return keyConfiguration[150, isGameMode];
                    case "S": return keyConfiguration[151, isGameMode];
                    case "SCROLL": return keyConfiguration[152, isGameMode];
                    case "SELECT": return keyConfiguration[153, isGameMode];
                    case "SELECTMEDIA": return keyConfiguration[154, isGameMode];
                    case "SEPARATOR": return keyConfiguration[155, isGameMode];
                    case "SHIFT": return keyConfiguration[156, isGameMode];
                    case "SHIFTKEY": return keyConfiguration[157, isGameMode];
                    case "SPACE": return keyConfiguration[158, isGameMode];
                    case "SUBTRACT": return keyConfiguration[159, isGameMode];
                    case "T": return keyConfiguration[160, isGameMode];
                    case "TAB": return keyConfiguration[161, isGameMode];
                    case "U": return keyConfiguration[162, isGameMode];
                    case "UP": return keyConfiguration[163, isGameMode];
                    case "V": return keyConfiguration[164, isGameMode];
                    case "VOLUMEDOWN": return keyConfiguration[165, isGameMode];
                    case "VOLUMEMUTE": return keyConfiguration[166, isGameMode];
                    case "VOLUMEUP": return keyConfiguration[167, isGameMode];
                    case "W": return keyConfiguration[168, isGameMode];
                    case "X": return keyConfiguration[169, isGameMode];
                    case "XBUTTON1": return keyConfiguration[170, isGameMode];
                    case "XBUTTON2": return keyConfiguration[171, isGameMode];
                    case "Y": return keyConfiguration[172, isGameMode];
                    case "Z": return keyConfiguration[173, isGameMode];
                    case "ZOOM": return keyConfiguration[174, isGameMode];
                    default: return _e;
                }
            }
            //Idle mode
            else
            {
                if (_e == keyConfiguration[0, 0])
                {
                    return "MODE1";
                }
                else if (_e == keyConfiguration[0, 1])
                {
                    return "MODE2";
                }
                else if (_e == "DELETE")
                    return "DEBUG";
                else
                    return "--";
            }
        }
        #endregion

        #region "Function for Button Press" 
        private void BtnPress(uint btnID, string btnState)
        {
            bool isPressed = false;
            if (btnState == "MAKE")
                isPressed = true;

            switch (btnID)
            {
                case btnA:
                    SetBtnA(vxbID, isPressed);
                    break;
                case btnB:
                    SetBtnB(vxbID, isPressed);
                    break;
                case btnX:
                    SetBtnX(vxbID, isPressed);
                    break;
                case btnY:
                    SetBtnY(vxbID, isPressed);
                    break;
                case btnLB:
                    SetBtnLB(vxbID, isPressed);
                    break;
                case btnRB:
                    SetBtnRB(vxbID, isPressed);
                    break;
                case btnLT:
                    SetBtnLT(vxbID, isPressed);
                    break;
                case btnRT:
                    SetBtnRT(vxbID, isPressed);
                    break;
                case btnST:
                    SetBtnStart(vxbID, isPressed);
                    break;
                case btnBK:
                    SetBtnBack(vxbID, isPressed);
                    break;
                default:
                    break;
            }

        }
        #endregion

        #region "Function for D-pad Press"
        private void DpadPress(float povdir, string btnState)
        {
            if (btnState == "MAKE")
                switch (povdir)
                {
                    case dpadL:
                        SetDpadLeft(vxbID);
                        break;
                    case dpadR:
                        SetDpadRight(vxbID);
                        break;
                    case dpadU:
                        SetDpadUp(vxbID);
                        break;
                    case dpadD:
                        SetDpadDown(vxbID);
                        break;
                    default:
                        break;
                }
            else
                SetDpadOff(vxbID);
        }
        #endregion

        #region "Function for Stick Movement"
        private void StickMove(uint axis, short value, string btnState)
        {
            short toPress = value;

            int [] indexSet = { 0, 0 }; //set,check

            if (value > 0)
                indexSet[0] = 1;
            if (value < 0)
                indexSet[1] = 1;

            if (btnState == "MAKE")
                stkStatus[axis-1, indexSet[0]] = true;
            else
            {
                stkStatus[axis-1, indexSet[0]] = false;
                if (stkStatus[axis-1, indexSet[1]] == false)
                    toPress = 0;
                else
                    toPress *= -1;
            }

            switch (axis)
            {
                case stkLX:
                    SetAxisX(vxbID, toPress);
                    break;
                case stkLY:
                    SetAxisY(vxbID, toPress);
                    break;
                case stkRX:
                    SetAxisRx(vxbID, toPress);
                    break;
                case stkRY:
                    SetAxisRy(vxbID, toPress);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region "Function for Trigger Movement"
        private void TriggerPress(string btnState, uint trigger)
        {
            byte toPress = TriggerMaxima;
            if (btnState != "MAKE")
                toPress = 0;

            switch (trigger)
            {
                case trgLT:
                    SetTriggerL(vxbID, toPress);
                    break;
                case trgRT:
                    SetTriggerR(vxbID, toPress);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region "Function for Window Form button click"
        private void Btn1_Click(object sender, EventArgs e)
        {
            //Set Keyboard at first run
            if (nRun == 0)
            {
                LogW("Press any key of the desired keyboard...\n");
                nRun = 1;
            }
            //Changed to GameMode button
            else if (nRun > 1)
            {
                ExecuteCalls(keyConfiguration[0,0],"BREAK");
            }
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            //Set Keyboard at first run
            if (nRun == 0)
            {
                LogW("Press any key of any keyboard...\n");
                universalController = true;
                nRun = 1;
            }
            //Changed to GameMode button
            else if (nRun > 1)
            {
                ExecuteCalls(keyConfiguration[0, 1], "BREAK");
            }
        }
        #endregion

        #region "Function for logging on the app window"
        //Logging to the Text Box Window
        private void LogW(string _log)
        {
            string _x = _log.Replace("\n", Environment.NewLine);
            LogBox.AppendText(_x);
        }

        //Log Closing Reason to LastLog and exit
        private void LogNExit(string _err)
        {
            forceSD = true;
            //logging error msge
            string _x = _err.Replace("\n", Environment.NewLine);
            MessageBox.Show(_x, "Error :: Application shutting down...");
            Application.Exit();
        }
        #endregion

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnPlug(vxbID);
            _rawinput.KeyPressed -= OnKeyPressed;
        }

        #region "UnhandledException"
        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (null == ex) return;

            // Log this error. Logging the exception doesn't correct the problem but at least now
            // you may have more insight as to why the exception is being thrown.
            Debug.WriteLine("Unhandled Exception (K2JMain):" + ex.Message);
            Debug.WriteLine("Unhandled Exception (K2JMain): " + ex);
            MessageBox.Show(ex.Message);
        }
        #endregion

        #region "Imported functions from vXboxInterface.dll"

        [DllImport("vXboxInterface.dll", EntryPoint = "isVBusExists")]
        private static extern bool isVBusExists();

        [DllImport("vXboxInterface.dll", EntryPoint = "isControllerExists")]
        private static extern bool isControllerExists(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "isControllerOwned")]
        private static extern bool isControllerOwned(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "PlugIn")]
        private static extern bool PlugIn(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "UnPlug")]
        private static extern bool UnPlug(uint UserIndex);

        //Does not work
        [DllImport("vXboxInterface.dll", EntryPoint = "GetLedNumber")]
        private static extern bool GetLedNumber(uint userIndex, out byte ledNumber);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnA")]
        private static extern bool SetBtnA(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnB")]
        private static extern bool SetBtnB(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnX")]
        private static extern bool SetBtnX(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnY")]
        private static extern bool SetBtnY(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnLB")]
        private static extern bool SetBtnLB(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnRB")]
        private static extern bool SetBtnRB(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnLT")]
        private static extern bool SetBtnLT(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnRT")]
        private static extern bool SetBtnRT(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnStart")]
        private static extern bool SetBtnStart(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetBtnBack")]
        private static extern bool SetBtnBack(uint UserIndex, bool Press);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetTriggerL")]
        private static extern bool SetTriggerL(uint UserIndex, byte Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetTriggerR")]
        private static extern bool SetTriggerR(uint UserIndex, byte Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetAxisX")]
        private static extern bool SetAxisX(uint UserIndex, Int16 Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetAxisY")]
        private static extern bool SetAxisY(uint UserIndex, Int16 Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetAxisRx")]
        private static extern bool SetAxisRx(uint UserIndex, Int16 Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetAxisRy")]
        private static extern bool SetAxisRy(uint UserIndex, Int16 Value);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetDpadUp")]
        private static extern bool SetDpadUp(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetDpadDown")]
        private static extern bool SetDpadDown(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetDpadLeft")]
        private static extern bool SetDpadLeft(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetDpadRight")]
        private static extern bool SetDpadRight(uint UserIndex);

        [DllImport("vXboxInterface.dll", EntryPoint = "SetDpadOff")]
        private static extern bool SetDpadOff(uint UserIndex);

        #endregion

    }
}

