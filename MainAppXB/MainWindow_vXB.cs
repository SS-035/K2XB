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
        private const short TriggerMaxima = 255;

        //Number of keys to be mapped
        private const int CFGsize = 27;
        private static string[] CFGarray = new string[CFGsize];

        //Data to output defence configuration
        private static readonly string[] BTNs = { "Y", "X", "B", "A", "LB", "RB", "LT" };
        private static readonly string[] BTNlbl = { "Rush GK", "Sliding Tackle", "Standing Tackle",
                                                    "Contain", "Change Player", "Teammate Contain", "Jockey" };

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

        private const float povU = 0.0f;
        private const float povR = 90.0f;
        private const float povD = 180.0f;
        private const float povL = 270.0f;

        private const uint axLX = 1;
        private const uint axLY = 2;
        private const uint axRT = 3;
        private const uint axRX = 4;
        private const uint axRY = 5;
        private const uint axLT = 6;

        //Stick status
        private static bool isLL = false;
        private static bool isLR = false;
        private static bool isLU = false;
        private static bool isLD = false;
        private static bool isRL = false;
        private static bool isRR = false;
        private static bool isRU = false;
        private static bool isRD = false;

        //Forced Shut down
        private static bool ForceSD = false;

        //Counter variables for First Run
        private static string FirstKeyboard = "$&%#";
        private static int nRun = 0;

        // Declaring one joystick (Device id 1)
        static public uint id = 1;

        //GAME Mode
        private static int isGameMode = 0;
        private static bool isDebug = false;

        //Error logging file
        private const string fileNameLog = "LastLog.out";

        private static string DefenceConfig = "";

        //LED number of virtual xBox
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
            bool findid = true;
            for (id = 1; id <= 4; id++)
            {
                findid = isControllerExists(id);
                //LogW("Device ID : " + id + "  ( " + findid + " )\n");
                if (findid == false)
                    break;
            }
            if (findid == true)
                LogNExit("No available vJoy Device found :( \n   Cannot continue\n");

            // Acquire the target
            if (PlugIn(id) != true)
                LogNExit("Failed to acquire vJoy device\n");
            else
            {
                //GetLedNumber(id, out Led);
                LogW("Acquired :: vXbox ID : " + id.ToString() + "\n");
            }

            this.Text = " :: K2J :: ID " + id.ToString() + " :: Blank";

            if (ForceSD == true)
            {
                LogW("Close this application");
            }
            #endregion
        }

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            //Forced exit on unsuccessful vXbox setup
            if (ForceSD == true) return;

            if (e.KeyPressEvent.DeviceType == "KEYBOARD")
            {
                //Add Keyboard and configuration in first run
                if (nRun == 1)
                {
                    //Set the first keyboard as the input
                    if (e.KeyPressEvent.KeyPressState == "MAKE")
                    {
                        FirstKeyboard = e.KeyPressEvent.DeviceHandle.ToString();
                        LogW("Connected to the Keyboad : " + FirstKeyboard + "\n");
                    }
                    else
                    {
                        //Prevent running twice while in ofd
                        nRun = 0;

                        #region "Load Configuration"

                        LogW("Select the Custom Configuration file\n:: NOTE ::\nProgram does not check legitimacy of the file\n");

                        OpenFileDialog ofd = new OpenFileDialog();
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            //load the configuration
                            string line;
                            StreamReader file;
                            file = new StreamReader(ofd.FileName);

                            try
                            {
                                for (int i = 0; i < CFGsize; i++)
                                {
                                    line = file.ReadLine();
                                    string[] words_ = line.Split(' ');
                                    CFGarray[i] = words_[0];
                                }
                                LogW("Conguration Loaded successfully.\n");


                            }
                            catch(Exception exception_)
                            {
                                Debug.WriteLine(exception_.ToString());
                                LogNExit("Corrupted Configuration file \n Cannot continue\n");
                            }
                            this.Text = " :: K2J :: ID " + id.ToString() + " :: "+ Path.GetFileNameWithoutExtension(ofd.FileName);
                        }
                        else
                            LogNExit("Unusable Configuration file not found \n Cannot continue\n");

                        #endregion

                        LogW("Feeding Started...\n");

                        #region "Process defence configuration to show"

                        for (int def_ = 20; def_ <= 26; def_++)
                        {
                            for (int atk_ = 0; atk_ <= 6; atk_++)
                            {
                                if (CFGarray[def_] == CFGarray[atk_])
                                    if (atk_ != def_ - 20)
                                        DefenceConfig = DefenceConfig + BTNs[atk_] + "\t" + BTNlbl[def_ - 20] + "\n";
                            }
                        }

                        if (DefenceConfig == "")
                            DefenceConfig = "No Change needed\n";
                        else
                            DefenceConfig = "Set these settings in\nFIFA > controller configuration > Classic > Defence\n" + DefenceConfig;
                        ShowDef.Text = "Show Defence";

                        #endregion

                        //Show Info onwindow
                        labelG.Visible = true;
                        labelM.Visible = true;
                        labelF.Visible = true;

                        //set nRun = 2 and send to next step
                        nRun = 2;
                    }

                }
                else if (e.KeyPressEvent.DeviceHandle.ToString() == FirstKeyboard)
                {
                    ExecuteCalls(e.KeyPressEvent.VKeyName, e.KeyPressEvent.KeyPressState);
                }
            }
        }

        #region "Virtual Key to custom calls (userConfig)"
        private static string UserConfig(string _e)
        {
            //return GAMEmode
            switch (_e)
            {
                case "END":
                    return "NOFEED";
                case "ADD":
                    return "GAMEON";
                case "SUBTRACT":
                    return "MENUON";
                default:
                    break;
            }
            #region "Mode 1: Fifa Mode"
            if (isGameMode == 1)
            {
                //Naming COnvension: B_* = Button, S_** = Stick Up/Down/Left/Right 
                //P_* = POV_Hat_Switch Up/Down/Left/Right 

                //Important::
                // Do not change this order
                if (_e == CFGarray[0]) return "B_Y";
                else if (_e == CFGarray[1]) return "B_X";
                else if (_e == CFGarray[2]) return "B_B";
                else if (_e == CFGarray[3]) return "B_A";
                else if (_e == CFGarray[4]) return "B_LB";
                else if (_e == CFGarray[5]) return "B_RB";
                else if (_e == CFGarray[6]) return "S_LT";
                else if (_e == CFGarray[7]) return "S_RT";
                else if (_e == CFGarray[8]) return "S_LU";
                else if (_e == CFGarray[9]) return "S_LD";
                else if (_e == CFGarray[10]) return "S_LL";
                else if (_e == CFGarray[11]) return "S_LR";
                else if (_e == CFGarray[12]) return "P_U";
                else if (_e == CFGarray[13]) return "P_L";
                else if (_e == CFGarray[14]) return "P_D";
                else if (_e == CFGarray[15]) return "P_R";
                else if (_e == CFGarray[16]) return "S_RU";
                else if (_e == CFGarray[17]) return "S_RL";
                else if (_e == CFGarray[18]) return "S_RD";
                else if (_e == CFGarray[19]) return "S_RR";
                else if (_e == CFGarray[20]) return "B_Y";
                else if (_e == CFGarray[21]) return "B_X";
                else if (_e == CFGarray[22]) return "B_B";
                else if (_e == CFGarray[23]) return "B_A";
                else if (_e == CFGarray[24]) return "B_LB";
                else if (_e == CFGarray[25]) return "B_RB";
                else if (_e == CFGarray[26]) return "S_LT";
                else if (_e == "ENTER") return "B_A";
                else if (_e == "ESCAPE") return "B_ST";
                else return _e;
            }
            #endregion

            #region "Mode 2: Menu Mode for FIFA"
            else if (isGameMode == 2)
            {
                switch (_e)
                {
                    //Team customization menu
                    case "UP":
                        return "S_LU";
                    case "DOWN":
                        return "S_LD";
                    case "LEFT":
                        return "S_LL";
                    case "RIGHT":
                        return "S_LR";
                    case "ENTER":
                        return "B_A";
                    case "Q":
                        return "B_RT";
                    case "F":
                        return "B_BK";
                    case "E":
                        return "S_RT";
                    case "W":
                        return "S_LT";
                    case "S":
                        return "B_X";
                    case "D":
                        return "B_Y";
                    case "X":
                        return "B_LB";
                    case "C":
                        return "B_RB";
                    case "NUMPAD8":
                        return "S_RU";
                    case "NUMPAD4":
                        return "S_RL";
                    case "NUMPAD2":
                        return "S_RD";
                    case "NUMPAD6":
                        return "S_RR";
                    case "ESCAPE":
                        return "B_B";

                    //Special case for team select menu to navigate
                    // Space = Enter    &    Backspace = Esc
                    // Arrow keys = IJKL
                    case "I":
                        return "S_LU";
                    case "K":
                        return "S_LD";
                    case "J":
                        return "S_LL";
                    case "L":
                        return "S_LR";
                    case "SPACE":
                        return "B_A";
                    case "BACK":
                        return "B_B";

                    default:
                        return _e;
                }
            }
            #endregion

            #region "Idle Mode: Delete = Debug"
            else
            {
                if (_e == "DELETE")
                    return "DEBUG";
                else
                    return _e;
            }
            #endregion
        }
        #endregion

        #region "Customs calls execution"
        private void ExecuteCalls (string vKey,string keyState)
        {
            //Virtual Key name and state
            string command = UserConfig(vKey);

            if (isDebug == true)
                LogW("\t" + vKey + ((keyState == "MAKE") ? " (D)" : " (U)") + "\t" + command + "\n");

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
                case "B_LT":
                    BtnPress(btnLT, keyState);
                    break;
                case "B_RT":
                    BtnPress(btnRT, keyState);
                    break;

                //POV
                case "P_U":
                    POVPress(povU, keyState);
                    break;
                case "P_L":
                    POVPress(povL, keyState);
                    break;
                case "P_D":
                    POVPress(povD, keyState);
                    break;
                case "P_R":
                    POVPress(povR, keyState);
                    break;

                //Trigger
                case "S_LT":
                    AxisMove(keyState, axLT, TriggerMaxima);
                    break;
                case "S_RT":
                    AxisMove(keyState, axRT, TriggerMaxima);
                    break;

                //Sticks
                case "S_LU":
                    if (keyState == "MAKE")
                    {
                        isLU = true;
                        AxisMove("MAKE", axLY, AxisMaxima);
                    }
                    else
                    {
                        isLU = false;
                        if (isLD == true)
                            AxisMove("MAKE", axLY, -AxisMaxima);
                        else
                            AxisMove(keyState, axLY, 0);
                    }
                    break;

                case "S_LD":
                    if (keyState == "MAKE")
                    {
                        isLD = true;
                        AxisMove("MAKE", axLY, -AxisMaxima);
                    }
                    else
                    {
                        isLD = false;
                        if (isLU == true)
                            AxisMove("MAKE", axLY, AxisMaxima);
                        else
                            AxisMove(keyState, axLY, 0);
                    }
                    break;

                case "S_LL":
                    if (keyState == "MAKE")
                    {
                        isLL = true;
                        AxisMove("MAKE", axLX, -AxisMaxima);
                    }
                    else
                    {
                        isLL = false;
                        if (isLR == true)
                            AxisMove("MAKE", axLX, AxisMaxima);
                        else
                            AxisMove(keyState, axLX, 0);
                    }
                    break;

                case "S_LR":
                    if (keyState == "MAKE")
                    {
                        isLR = true;
                        AxisMove("MAKE", axLX, AxisMaxima);
                    }
                    else
                    {
                        isLR = false;
                        if (isLL == true)
                            AxisMove("MAKE", axLX, -AxisMaxima);
                        else
                            AxisMove(keyState, axLX, 0);
                    }
                    break;

                case "S_RU":
                    if (keyState == "MAKE")
                    {
                        isRU = true;
                        AxisMove("MAKE", axRY, AxisMaxima);
                    }
                    else
                    {
                        isRU = false;
                        if (isRD == true)
                            AxisMove("MAKE", axRY, -AxisMaxima);
                        else
                            AxisMove(keyState, axRY, 0);
                    }
                    break;

                case "S_RD":
                    if (keyState == "MAKE")
                    {
                        isRD = true;
                        AxisMove("MAKE", axRY, -AxisMaxima);
                    }
                    else
                    {
                        isRD = false;
                        if (isRU == true)
                            AxisMove("MAKE", axRY, AxisMaxima);
                        else
                            AxisMove(keyState, axRY, 0);
                    }
                    break;

                case "S_RL":
                    if (keyState == "MAKE")
                    {
                        isRL = true;
                        AxisMove("MAKE", axRX, -AxisMaxima);
                    }
                    else
                    {
                        isRL = false;
                        if (isRR == true)
                            AxisMove("MAKE", axRX, AxisMaxima);
                        else
                            AxisMove(keyState, axRX, 0);
                    }
                    break;

                case "S_RR":
                    if (keyState == "MAKE")
                    {
                        isRR = true;
                        AxisMove("MAKE", axRX, AxisMaxima);
                    }
                    else
                    {
                        isRR = false;
                        if (isRL == true)
                            AxisMove("MAKE", axRX, -AxisMaxima);
                        else
                            AxisMove(keyState, axRX, 0);
                    }
                    break;

                //Extra Mode Function
                case "NOFEED":
                    if (keyState == "MAKE")
                        LogW("FEED  Off\n");
                    else
                        isGameMode = 0;
                    break;

                case "DEBUG":
                    if (keyState != "MAKE")
                    {
                        LogW("Debugging ");
                        isDebug = !isDebug;
                        if (isDebug == true) LogW("ON\n");
                        else LogW("OFF\n");
                    }
                    break;

                case "GAMEON":
                    if (keyState == "MAKE")
                        LogW("GAMEMODE   :)\n");
                    else
                        isGameMode = 1;
                    break;

                case "MENUON":
                    if (keyState == "MAKE")
                        LogW("Customization menu mode\n");
                    else
                        isGameMode = 2;
                    break;

                default:
                    break;
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
                    SetBtnA(id, isPressed);
                    break;
                case btnB:
                    SetBtnB(id, isPressed);
                    break;
                case btnX:
                    SetBtnX(id, isPressed);
                    break;
                case btnY:
                    SetBtnY(id, isPressed);
                    break;
                case btnLB:
                    SetBtnLB(id, isPressed);
                    break;
                case btnRB:
                    SetBtnRB(id, isPressed);
                    break;
                case btnLT:
                    SetBtnLT(id, isPressed);
                    break;
                case btnRT:
                    SetBtnRT(id, isPressed);
                    break;
                case btnST:
                    SetBtnStart(id, isPressed);
                    break;
                case btnBK:
                    SetBtnBack(id, isPressed);
                    break;
                default:
                    break;
            }

        }
        #endregion

        #region "Function for POV Hat Press"
        private void POVPress(float povdir, string btnState)
        {
            if (btnState == "MAKE")
                switch (povdir)
                {
                    case povL:
                        SetDpadLeft(id);
                        break;
                    case povR:
                        SetDpadRight(id);
                        break;
                    case povU:
                        SetDpadUp(id);
                        break;
                    case povD:
                        SetDpadDown(id);
                        break;
                    default:
                        break;
                }
            else
                SetDpadOff(id);
        }
        #endregion

        #region "Function for Stick/Axis Movement"
        private void AxisMove(string btnState, uint axis, short value)
        {
            short toPress = value;
            if (btnState != "MAKE")
                toPress = 0;

            switch (axis)
            {
                case axLX:
                    SetAxisX(id, toPress);
                    break;
                case axLY:
                    SetAxisY(id, toPress);
                    break;
                case axRX:
                    SetAxisRx(id, toPress);
                    break;
                case axRY:
                    SetAxisRy(id, toPress);
                    break;
                case axLT:
                    SetTriggerL(id, (byte)toPress);
                    break;
                case axRT:
                    SetTriggerR(id, (byte)toPress);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region "Function for Window Form button click"
        private void ShowDef_Click(object sender, EventArgs e)
        {
            //Set Keyboard at first run
            if (nRun == 0)
            {
                LogW("Press any key on the desired keyboard...\n");
                nRun++;
            }
            else
                LogW(DefenceConfig);
        }
        #endregion

        #region "Function for logging"
        //Logging to the Text Box Window
        private void LogW(string _log)
        {
            string _x = _log.Replace("\n", Environment.NewLine);
            LogBox.AppendText(_x);
        }

        //Log Closing Reason to LastLog and exit
        private static void LogNExit(string _err)
        {
            ForceSD = true;
            //logging error msge
            using (StreamWriter sw = File.AppendText(fileNameLog))
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\n" + _err);
            Application.Exit();
        }
        #endregion

        private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnPlug(id);
            _rawinput.KeyPressed -= OnKeyPressed;
        }

        #region "UnhandledException"
        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (null == ex) return;

            // Log this error. Logging the exception doesn't correct the problem but at least now
            // you may have more insight as to why the exception is being thrown.
            Debug.WriteLine("Unhandled Exception: " + ex.Message);
            Debug.WriteLine("Unhandled Exception: " + ex);
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

#region "Template for Configuration"
//Template of Fifa User's Custom Configuration file and currently set to classic 
//THis order directly matches to the the FFIA mode (game mode = 1) of UserConfig function
#if false

K         //Through Ball (Y)
J         //Lob Pass (X)
SPACE     //Shoot (B)
L         //Short Pass (A)
LCONTROL  //Player Run/Modifier (LB)
I         //Finesse Shot (RB)
CAPITAL   //Pace Control(LT)
LSHIFT    //Sprint	(RT)
W         //Left Stick UP (LU)
S         //Left Stick Down (LD)
A         //Left Stick Left (LL)
D         //Left Stick Right (LR)
UP        //Tactics UP (PovU)
LEFT      //Mentality Left (PovL)
DOWN      //Tactics Down (PovD)
RIGHT     //Mentality Right (PovR)
NUMPAD8   //Right Stick UP	(RU)
NUMPAD4   //Right Stick Left (RL)
NUMPAD2   //Right Stick Down (RD)
NUMPAD6   //Right Stick Right (RR)
K         //Rush GK (Y)
J         //Sliding Tackle (X)
SPACE     //Standing Tackle (B)
L         //Contain (A)
LCONTROL  //Change Player (LB)
I         //Teammate Contain (RB)
CAPITAL   //Jockey (LT)

#endif
#endregion
