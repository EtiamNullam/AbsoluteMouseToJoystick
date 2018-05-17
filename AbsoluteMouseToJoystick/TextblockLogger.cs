using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AbsoluteMouseToJoystick
{
    public class TextBlockLogger : ISimpleLogger
    {
        public TextBlockLogger(TextBlock textBlock)
        {
            this._textBlock = textBlock;
        }

        private TextBlock _textBlock;

        public void Log(string message)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                this._textBlock.Text = $"{message}\n{this._textBlock.Text}";
            });
        }
    }
}
