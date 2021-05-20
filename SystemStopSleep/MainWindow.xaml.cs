using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
namespace SystemStopSleep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _dt = new DispatcherTimer();
        public bool _mouse = true;
        public int _time = 10000;
        public int _shake = 10;
        public int _pos = 1;
        public MainWindow()
        {
            InitializeComponent();
            _dt.Tick += new EventHandler(MoveMouse);
            LoadAppSettings();
            Active();
        }
        private void LoadAppSettings()
        {
            try
            {
                bool mouse = Convert.ToBoolean(ConfigurationManager.AppSettings["IsMoveMouse"]);
                int shakeOption = Convert.ToInt32(ConfigurationManager.AppSettings["ShakeOption"]);
                int timeOption = Convert.ToInt32(ConfigurationManager.AppSettings["TimeOption"]);
                //_mouse = mouse;
                //ShakeChange(shakeOption);
                //TimeChange(timeOption);
                ckbMoveMouse.IsChecked = mouse;
                cmbShake.SelectedIndex = shakeOption;
                cmbTime.SelectedIndex = timeOption;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail load ConfigFile <Erro: " + ex.Message + ">");
            }
        }
        private void Active()
        {
            lbStart.IsEnabled = false;
            lbStop.IsEnabled = true;
            if (_mouse)
            {
                _dt.Interval = new TimeSpan(0, 0, 0, 0, _time);
                _dt.Start();
            }
            PowerHelper.ForceSystemAwake();
        }
        private void Desactive()
        {
            lbStop.IsEnabled = false;
            lbStart.IsEnabled = true;
            _dt.Stop();
            PowerHelper.ResetSystemDefault();
        }
        private void lbStart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Active();
        }
        private void lbStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Desactive();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Desactive();
        }
        private void ckbMoveMouse_Click(object sender, RoutedEventArgs e)
        {
            if (ckbMoveMouse.IsChecked == true)
                _mouse = true;
            else
                _mouse = false;
            Desactive();
        }
        private void MoveMouse(object sender, EventArgs e)
        {
            NativeMethods.POINT pGet;
            NativeMethods.GetCursorPos(out pGet);
            NativeMethods.POINT pSet = new NativeMethods.POINT(pGet.x, pGet.y);
            NativeMethods.SetCursorPos(pSet.x + _pos, pSet.y + _pos);
            Thread.Sleep(_shake);
            NativeMethods.SetCursorPos(pGet.x, pGet.y);
        }
        private void cmbShake_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ShakeChange(cmbShake.SelectedIndex);
            Desactive();
        }
        private void cmbTime_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TimeChange(cmbTime.SelectedIndex);
            Desactive();
        }
        private void ShakeChange(int option)
        {
            switch (option)
            {
                case 0:
                    _shake = 10;
                    _pos = 1;
                    break;
                case 1:
                    _shake = 20;
                    _pos = 5;
                    break;
                case 2:
                    _shake = 40;
                    _pos = 15;
                    break;
                case 3:
                    _shake = 60;
                    _pos = 40;
                    break;
                case 4:
                    _shake = 100;
                    _pos = 100;
                    break;
                default:
                    break;
            }
        }
        private void TimeChange(int option)
        {
            switch (option)
            {
                case 0:
                    _time = 5000;
                    break;
                case 1:
                    _time = 10000;
                    break;
                case 2:
                    _time = 30000;
                    break;
                case 3:
                    _time = 90000;
                    break;
                default:
                    break;
            }
        }

    }
}
public class PowerHelper
{
    public static void ForceSystemAwake()
    {
        NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS |
                                              NativeMethods.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                                              NativeMethods.EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                                              NativeMethods.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
    }
    public static void ResetSystemDefault()
    {
        NativeMethods.SetThreadExecutionState(NativeMethods.EXECUTION_STATE.ES_CONTINUOUS);
    }
}
internal static partial class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
    // MOVE MOUSE
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT pPoint);
    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    // MOVE MOUSE
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
        public POINT(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }
}