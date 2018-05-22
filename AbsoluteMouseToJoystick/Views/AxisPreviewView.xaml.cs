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

            RotatePreview(Orientation);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AxisPreviewView), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((AxisPreviewView)d).RotatePreview((Orientation)e.NewValue);
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AxisPreviewView));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set
            {
                SetValue(OrientationProperty, value);
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

        public void RotatePreview(Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                DisableRows();
                EnableColumns();
            }
            else
            {
                DisableColumns();
                EnableRows();
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
