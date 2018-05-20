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
     * - Add some kind of MVVM
     *  - Split into regions (region for axisX region for axisX preview etc)
     *  - Make them reusable (region for axisX and Y should only differ in a paramter passed
     * - Save options to files (json)
     * - Add textbox for device ID
     * - Allow for use of different axes
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _logger = new TextBoxLogger(tb);

            ShowJoystickInfo(_joy, DeviceID);
            UpdateZoneDistributions();
        }

        private const uint DeviceID = 1;

        private Feeder _feeder;
        private vJoy _joy = new vJoy();
        private ISimpleLogger _logger;

        private ZoneDistribution _zoneDistributionX = new ZoneDistribution();
        private ZoneDistribution _zoneDistributionY = new ZoneDistribution();

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

        /* TODO:
         * - Introduce star-like behaviour for zones
         */
        private void OnZoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _logger?.Log("Zone text changed");

            UpdateZoneDistributions();
        }

        private void UpdateZoneDistributions()
        {
            try
            {
                negativeDeadColumnX.Width = new GridLength(_zoneDistributionX.NegativeDeadZone = Convert.ToDouble(this.negativeDeadXtb.Text), GridUnitType.Star);
                negativeColumnX.Width = new GridLength(_zoneDistributionX.NegativeZone = Convert.ToDouble(this.negativeXtb.Text), GridUnitType.Star);
                neutralColumnX.Width = new GridLength(_zoneDistributionX.NeutralDeadZone = Convert.ToDouble(this.neutralXtb.Text), GridUnitType.Star);
                positiveColumnX.Width = new GridLength(_zoneDistributionX.PositiveZone = Convert.ToDouble(this.positiveXtb.Text), GridUnitType.Star);
                positiveDeadColumnX.Width = new GridLength(_zoneDistributionX.PositiveDeadZone = Convert.ToDouble(this.positiveDeadXtb.Text), GridUnitType.Star);

                negativeDeadRowY.Height = new GridLength(_zoneDistributionY.NegativeDeadZone = Convert.ToDouble(this.negativeDeadYtb.Text), GridUnitType.Star);
                negativeRowY.Height = new GridLength(_zoneDistributionY.NegativeZone = Convert.ToDouble(this.negativeYtb.Text), GridUnitType.Star);
                neutralRowY.Height = new GridLength(_zoneDistributionY.NeutralDeadZone = Convert.ToDouble(this.neutralYtb.Text), GridUnitType.Star);
                positiveRowY.Height = new GridLength(_zoneDistributionY.PositiveZone = Convert.ToDouble(this.positiveYtb.Text), GridUnitType.Star);
                positiveDeadRowY.Height = new GridLength(_zoneDistributionY.PositiveDeadZone = Convert.ToDouble(this.positiveDeadYtb.Text), GridUnitType.Star);
            }
            catch (Exception ex)
            {
                _logger?.Log(ex.Message);
                _logger?.Log("Invalid zone format");
            }
        }
    }
}
