using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick
{
    /* TODO:
     * - Show current values in app (button)
     * - Toggle Virtual Joystick in app (button)
     * - Add Deadzones
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var joy = new vJoy();
            uint deviceID = 1;

            if (!ValidateJoystick(joy, deviceID)) return;

            Timer timer = new Timer();

            timer.Interval = 50;
            timer.AutoReset = true;
            timer.Start();

            var logger = new TextBlockLogger(tb);

            Feeder feeder = new Feeder(joy, deviceID, logger);
            feeder.AddTimer(timer);
        }

        // TODO:
        // - Acquire somewhere else
        // - Relinquish on exit (or some button press)
        private bool ValidateJoystick(vJoy joy, uint deviceID)
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (joy.DriverMatch(ref DllVer, ref DrvVer) && joy.vJoyEnabled())
            {
                tb.Text += "True\n";

                // Get the state of the requested device
                uint id = 1; // or 1?
                VjdStat status = joy.GetVJDStatus(id);

                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        tb.Text += $"vJoy Device {id} is already owned by this feeder\n";
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        tb.Text += $"vJoy Device {id} is free\n";

                        joy.AcquireVJD(id);

                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        tb.Text += $"vJoy Device {id} is already owned by another feeder\nCannot continue\n";
                        return false;
                    case VjdStat.VJD_STAT_MISS:
                        tb.Text += $"vJoy Device {id} is not installed or disabled\nCannot continue\n";
                        return false;
                    default:
                        tb.Text += $"vJoy Device {id} general error\nCannot continue\n";
                        return false;
                }

                ///// vJoy Device properties
                int nBtn = joy.GetVJDButtonNumber(id);
                int nDPov = joy.GetVJDDiscPovNumber(id);
                int nCPov = joy.GetVJDContPovNumber(id);
                bool X_Exist = joy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
                bool Y_Exist = joy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
                bool Z_Exist = joy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
                bool RX_Exist = joy.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
                tb.Text += $"Device[{id}]: Buttons={nBtn}; DiscPOVs:{nDPov}; ContPOVs:{nCPov}\n";
                return true;
            }
            else return false;
        }
    }
}
