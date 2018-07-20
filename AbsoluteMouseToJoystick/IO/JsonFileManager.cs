using AbsoluteMouseToJoystick.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.IO
{
    public class JsonFileManager
    {
        public JsonFileManager(ISimpleLogger logger, JsonSerializer jsonSerializer)
        {
            this._logger = logger;
            this._jsonSerializer = jsonSerializer;
        }

        public event EventHandler<object> OnFileLoaded;

        private readonly ISimpleLogger _logger;
        private readonly JsonSerializer _jsonSerializer;
        private readonly string JsonAllFilesFilter = "JSON Files (*.json)|*.json|All Files (*.*)|*";

        public void Save(object obj)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save config",
                OverwritePrompt = true,
                AddExtension = true,
                Filter = JsonAllFilesFilter,
                InitialDirectory = Directory.GetCurrentDirectory(),
                FileName = "default.json",
                Tag = obj,
            };

            dialog.FileOk += this.OnFileForSaveSelected;
            dialog.ShowDialog();
        }

        public void OpenWithDialog<T>()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open config",
                AddExtension = true,
                Filter = JsonAllFilesFilter,
                InitialDirectory = Directory.GetCurrentDirectory(),
                FileName = "default.json",
                Multiselect = false,
            };

            dialog.FileOk += this.OnFileForOpenSelected<T>;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Throws exception when can't read file.
        /// </summary>
        public T Open<T>(string path)
        {
            using (var streamReader = new StreamReader(path))
            using (var jsonTextReader = new JsonTextReader(streamReader))
                return this._jsonSerializer.Deserialize<T>(jsonTextReader);
        }

        private void OnFileForOpenSelected<T>(object sender, CancelEventArgs e)
        {
            if (sender is OpenFileDialog dialog)
            {
                dialog.FileOk -= this.OnFileForOpenSelected<T>;

                try
                {
                    using (var stream = dialog.OpenFile())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var loadResult = this._jsonSerializer.Deserialize<T>(jsonTextReader);

                        this.OnFileLoaded?.Invoke(this, loadResult);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.Log(ex.Message);
                }
            }
            else
            {
                this._logger.Log("Invalid dialog received at OnFileForOpenSelected()");
            }
        }

        private void OnFileForSaveSelected(object sender, CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= this.OnFileForSaveSelected;

                try
                {
                    using (var stream = dialog.OpenFile())
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        this._jsonSerializer.Serialize(streamWriter, dialog.Tag);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.Log(ex.Message);
                }
            }
            else
            {
                this._logger.Log("Invalid dialog received at OnFileForSaveSelected()");
            }
        }
    }
}
