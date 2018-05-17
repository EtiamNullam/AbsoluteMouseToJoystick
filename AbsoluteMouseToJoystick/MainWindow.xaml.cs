using AbsoluteMouseToJoystick.Data;
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
     * - Right now its fixed for 1080p make it customizable
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _joy = new vJoy();
            _logger = new TextBlockLogger(tb);

            _zoneDistributionX = new ZoneDistribution
            {
                NegativeDeadZone = 0.05f,
                NegativeZone = 0.45f,
                NeutralDeadZone = 0f,
                PositiveZone = 0.45f,
                PositiveDeadZone = 0.05f
            };

            _zoneDistributionY = new ZoneDistribution
            {
                NegativeDeadZone = 0.1f,
                NegativeZone = 0.45f,
                NeutralDeadZone = 0.05f,
                PositiveZone = 0.3f,
                PositiveDeadZone = 0.1f
            };

            ShowJoystickInfo(_joy, DeviceID);
        }

        private const uint DeviceID = 1;

        private Feeder _feeder;
        private vJoy _joy;
        private ISimpleLogger _logger;

        private ZoneDistribution _zoneDistributionX;
        private ZoneDistribution _zoneDistributionY;

        // TODO:
        // - Acquire somewhere else
        // - Relinquish on exit (or some button press)
        private bool ShowJoystickInfo(vJoy joy, uint deviceID)
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (joy.DriverMatch(ref DllVer, ref DrvVer) && joy.vJoyEnabled())
            {
                // Get the state of the requested device
                VjdStat status = joy.GetVJDStatus(deviceID);

                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        _logger.Log($"vJoy Device {deviceID} is already owned by this feeder");
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        _logger.Log($"vJoy Device {deviceID} is free");
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        _logger.Log($"vJoy Device {deviceID} is already owned by another feeder\nCannot continue");
                        return false;
                    case VjdStat.VJD_STAT_MISS:
                        _logger.Log($"vJoy Device {deviceID} is not installed or disabled\nCannot continue");
                        return false;
                    default:
                        _logger.Log($"vJoy Device {deviceID} general error\nCannot continue");
                        return false;
                }

                ///// vJoy Device properties
                int nBtn = joy.GetVJDButtonNumber(deviceID);
                int nDPov = joy.GetVJDDiscPovNumber(deviceID);
                int nCPov = joy.GetVJDContPovNumber(deviceID);
                bool X_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_X);
                bool Y_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_Y);
                bool Z_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_Z);
                bool RX_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_RX);
                _logger.Log($"Device[{deviceID}]: Buttons={nBtn}; DiscPOVs:{nDPov}; ContPOVs:{nCPov}");
                return true;
            }
            else return false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Running) Start();
            else Stop();
        }

        private void Start()
        {
            if (_joy.AcquireVJD(DeviceID))
            {
                Running = true;

                _logger.Log("Device acquired.");

                Timer timer = new Timer
                {
                    Interval = 5,
                    AutoReset = true
                };

                timer.Start();

                _feeder = new Feeder(_joy, DeviceID, _logger, timer, _zoneDistributionX, _zoneDistributionY, Convert.ToInt32(resXtb.Text), Convert.ToInt32(resYtb.Text));
            }
            else
            {
                _logger.Log("Device acquire FAILED.");
            }
        }

        private void Stop()
        {
            Running = false;

            _joy.RelinquishVJD(DeviceID);

            _logger.Log("Device relinquished.");

            if (_feeder != null)
            {
                _feeder.Dispose();
                _feeder = null;
            }

            ShowJoystickInfo(_joy, DeviceID);
        }

        public bool Running
        {
            get
            {
                return _running;
            }
            set
            {
                _running = value;
                this.runningCheckBox.IsChecked = _running;
            }
        }

        private bool _running;
    }
}
