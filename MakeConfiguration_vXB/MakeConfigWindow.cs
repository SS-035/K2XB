using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using RawInput_dll;

namespace MakeConfiguration_vXB
{
    public partial class MakeConfigurationWindow : Form
    {
        private readonly RawInput _rawinput;
        const bool CaptureOnlyInForeground = true;

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
    }
}
