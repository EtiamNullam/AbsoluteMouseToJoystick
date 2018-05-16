using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick
{
    public class Feeder
    {
        public Feeder(vJoy joy, uint deviceID, ISimpleLogger logger)
        {
            _joy = joy;
            _deviceID = deviceID;
            _logger = logger;

            _joy.ResetVJD(deviceID);
        }

        private ISimpleLogger _logger;
        private vJoy _joy;
        private uint _deviceID;
        
        public void AddTimer(Timer timer)
        {
            timer.Elapsed += Execute;
        }

        // TODO: use effiecent way instead? (from readme.pdf)
        private void Execute(object sender, EventArgs e)
        {
            Interop.POINT point;

            Interop.GetCursorPos(out point);

            point.X = Convert.ToInt32((double)point.X / (1920 - 1) * short.MaxValue);
            point.Y = Convert.ToInt32((double)point.Y / (1080 - 1) * short.MaxValue);


            this._joy.SetAxis(point.X, _deviceID, HID_USAGES.HID_USAGE_X);
            //this._joy.SetAxis(short.MaxValue / 1, _deviceID, HID_USAGES.HID_USAGE_X);

            this._joy.SetAxis(point.Y, _deviceID, HID_USAGES.HID_USAGE_Y);
/*
            App.Current?.Dispatcher.Invoke(() =>
            {
                _logger.Log($"Feeder: {point.X}/{point.Y}");
            });
            */
        }
    }
}
