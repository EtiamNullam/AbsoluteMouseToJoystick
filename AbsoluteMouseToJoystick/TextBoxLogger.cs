using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AbsoluteMouseToJoystick
{
    public class TextBoxLogger : ISimpleLogger
    {
        public TextBoxLogger(TextBox textBox)
        {
            this._textBox = textBox;
        }

        private TextBox _textBox;

        public void Log(string message)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                this._textBox.Text = $"{message}\n{this._textBox.Text}";
            });
        }
    }
}
