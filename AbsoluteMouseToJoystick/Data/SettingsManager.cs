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
    public class SettingsManager : ISettingsManager
    {
        public SettingsManager(ISimpleLogger logger, JsonFileManager jsonFileManager, ISettingsBindable settings)
        {
            this._logger = logger;
            this._jsonFileManager = jsonFileManager;
            this._settings = settings;

            LoadDefault();
        }

        public void Load(ISettings source)
        {
            this._settings.ResolutionX = source.ResolutionX;
            this._settings.ResolutionY = source.ResolutionY;
            this._settings.TimerInterval = source.TimerInterval;
            this._settings.DeviceID = source.DeviceID;
            this._settings.AxisX = source.AxisX;
            this._settings.AxisY = source.AxisY;
            this._settings.Buttons = source.Buttons;
        }

        private readonly ISimpleLogger _logger;
        private readonly JsonFileManager _jsonFileManager;
        private readonly ISettingsBindable _settings;

        private readonly string DefaultPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "default.json";

        private void LoadDefault()
        {
            try
            {
                Load(_jsonFileManager.Open<Settings>(this.DefaultPath));
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }

        public void SaveToFile()
            => _jsonFileManager.Save(_settings);

        public void LoadFromFile()
        {
            _jsonFileManager.OnFileLoaded += OnSettingsLoaded;
            _jsonFileManager.OpenWithDialog<Settings>();
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
