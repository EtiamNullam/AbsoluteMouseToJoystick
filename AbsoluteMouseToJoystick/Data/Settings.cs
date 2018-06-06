using AbsoluteMouseToJoystick.IO;
using AbsoluteMouseToJoystick.Logging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class Settings : ObservableObject, ISettingsManager
    {
        public Settings(ISimpleLogger logger, JsonFileManager jsonFileManager)
        {
            this._logger = logger;
            this._jsonFileManager = jsonFileManager;

            LoadDefault();
        }

        public void Load(ISettings source)
        {
            this.ResolutionX = source.ResolutionX;
            this.ResolutionY = source.ResolutionY;
            this.TimerInterval = source.TimerInterval;
            this.DeviceID = source.DeviceID;
            this.AxisX = source.AxisX;
            this.AxisY = source.AxisY;
            this.AxisZ = source.AxisZ;
            this.Buttons = source.Buttons;
        }

        public int ResolutionX
        {
            get => _resolutionX;
            set
            {
                Set(ref _resolutionX, value);
                _logger?.Log("Resolution X changed");
            }
        }
        public int ResolutionY
        {
            get => _resolutionY;
            set
            {
                Set(ref _resolutionY, value);
                _logger?.Log("Resolution Y changed");
            }
        }

        public double TimerInterval
        {
            get => _timerInterval;
            set
            {
                Set(ref _timerInterval, value);
                _logger?.Log("Timer interval changed");
            }
        }

        public uint DeviceID
        {
            get => _deviceID;
            set
            {
                Set(ref _deviceID, value);
                _logger?.Log("Device ID changed");
            }
        }

        public AxisSettings AxisX
        {
            get => _axisX;
            set => Set(ref _axisX, value);
        }

        public AxisSettings AxisY
        {
            get => _axisY;
            set => Set(ref _axisY, value);
        }

        public AxisSettings AxisZ
        {
            get => _axisZ;
            set => Set(ref _axisZ, value);
        }

        public bool[] Buttons
        {
            get => _buttons;
            set => Set(ref _buttons, value);
        }

        private readonly ISimpleLogger _logger;
        private readonly JsonFileManager _jsonFileManager;

        private readonly string DefaultPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "default.json";

        private int _resolutionX, _resolutionY;
        private double _timerInterval;
        private uint _deviceID;

        private AxisSettings _axisX, _axisY, _axisZ;

        private bool[] _buttons;

        private void LoadDefault()
        {
            try
            {
                Load(new SettingsRaw());
                Load(_jsonFileManager.Open<SettingsRaw>(this.DefaultPath));
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }

        public void SaveToFile()
            => _jsonFileManager.Save(this);

        public void LoadFromFile()
        {
            _jsonFileManager.OnFileLoaded += OnSettingsLoaded;
            _jsonFileManager.OpenWithDialog<SettingsRaw>();
        }

        private void OnSettingsLoaded(object sender, object settings)
        {
            _jsonFileManager.OnFileLoaded -= OnSettingsLoaded;

            if (settings is ISettings castedSettings)
            {
                Load(castedSettings);
            }
        }
    }
}
