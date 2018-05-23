﻿using AbsoluteMouseToJoystick.Logging;
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
            _logger = logger;
            _jsonSerializer = jsonSerializer;
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

            dialog.FileOk += OnFileForSaveSelected;
            dialog.ShowDialog();
        }

        public void Open<T>()
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

            dialog.FileOk += OnFileForOpenSelected<T>;
            dialog.ShowDialog();
        }

        private void OnFileForOpenSelected<T>(object sender, CancelEventArgs e)
        {
            if (sender is OpenFileDialog dialog)
            {
                dialog.FileOk -= OnFileForOpenSelected<T>;

                try
                {
                    using (var stream = dialog.OpenFile())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var loadResult = _jsonSerializer.Deserialize<T>(jsonTextReader);

                        this.OnFileLoaded?.Invoke(this, loadResult);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.Message);
                }
            }
            else
            {
                _logger.Log("Invalid dialog received at OnFileForOpenSelected()");
            }
        }

        private void OnFileForSaveSelected(object sender, CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= OnFileForSaveSelected;

                try
                {
                    using (var stream = dialog.OpenFile())
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        _jsonSerializer.Serialize(streamWriter, dialog.Tag);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.Message);
                }
            }
            else
            {
                _logger.Log("Invalid dialog received at OnFileForSaveSelected()");
            }
        }
    }
}