using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YouLessMonitor.ViewModel;

namespace YouLessMonitor
{
    public partial class MainWindow : Window
    {
        private Queue<string> _Log;

        public MainWindow()
        {
            InitializeComponent();
            _Log = new Queue<string>();
            Log.OnLog += Log_OnLog;

            if (!DesignerProperties.GetIsInDesignMode(this) && DataContext is MasterViewModel mvm) mvm.Start();
        }

        private void Log_OnLog(string msg, LogLevel lvl)
        {
            if (_Log.Count >= 1000) _Log.Dequeue();
            _Log.Enqueue(msg);
            _LogBox.Text = string.Join(Environment.NewLine, _Log);
            _LogBox.CaretIndex = _LogBox.Text.Length;
            _LogBox.ScrollToEnd();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Log.OnLog -= Log_OnLog;
        }

        private void ToggleMainWindow(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible) Hide();
            else Show();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
            {
                var date = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var time = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                (e.Column as DataGridTextColumn).Binding.StringFormat = $"{date} - {time}";
            }
        }

        private void DataGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null && sender is DataGrid grid && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
            {
                var dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                dgr.IsSelected = dgr.IsMouseOver;
            }
        }
    }
}