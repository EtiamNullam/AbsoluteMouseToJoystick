using AbsoluteMouseToJoystick.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AbsoluteMouseToJoystick.Views
{
    /// <summary>
    /// Interaction logic for AxisPreviewView.xaml
    /// </summary>
    public partial class AxisPreviewView : UserControl
    {
        public AxisPreviewView()
        {
            InitializeComponent();

            RotatePreview(this.MouseAxis);
        }

        public static readonly DependencyProperty MouseAxisProperty =
            DependencyProperty.Register("MouseAxis", typeof(MouseAxis), typeof(AxisPreviewView), new PropertyMetadata(Data.MouseAxis.None, OnMouseAxisChanged));

        private static void OnMouseAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((AxisPreviewView)d).RotatePreview((MouseAxis)e.NewValue);
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AxisPreviewView));

        public MouseAxis MouseAxis
        {
            get => (MouseAxis)GetValue(MouseAxisProperty);
            set
            {
                SetValue(MouseAxisProperty, value);
                RotatePreview(value);
            }
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private List<ColumnDefinition> DisabledColumns = new List<ColumnDefinition>();
        private List<RowDefinition> DisabledRows = new List<RowDefinition>();

        public void RotatePreview(MouseAxis axis)
        {
            switch (axis)
            {
                case MouseAxis.None:
                    DisableRows();
                    DisableColumns();
                    break;
                case MouseAxis.X:
                    DisableRows();
                    EnableColumns();
                    break;
                case MouseAxis.Y:
                    DisableColumns();
                    EnableRows();
                    break;
            }
        }

        private void DisableRows()
        {
            foreach (var row in this.rootGrid.RowDefinitions)
            {
                if (row.Tag != null && row.Tag.ToString() == "d")
                {
                    this.DisabledRows.Add(row);
                }
            }

            foreach (var row in this.DisabledRows)
            {
                this.rootGrid.RowDefinitions.Remove(row);
            }
        }

        private void DisableColumns()
        {
            foreach (var column in this.rootGrid.ColumnDefinitions)
            {
                if (column.Tag != null && column.Tag.ToString() == "d")
                {
                    this.DisabledColumns.Add(column);
                }
            }

            foreach (var column in this.DisabledColumns)
            {
                this.rootGrid.ColumnDefinitions.Remove(column);
            }
        }

        private void EnableRows()
        {
            foreach (var row in this.DisabledRows)
            {
                this.rootGrid.RowDefinitions.Add(row);
            }

            this.DisabledRows.Clear();
        }

        private void EnableColumns()
        {
            foreach (var column in this.DisabledColumns)
            {
                this.rootGrid.ColumnDefinitions.Add(column);
            }

            this.DisabledColumns.Clear();
        }
    }
}
