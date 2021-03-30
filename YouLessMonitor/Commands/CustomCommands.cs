using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WPFMVVM.Commands;
using YouLessMonitor.ViewModel;

namespace YouLessMonitor.Commands
{
    public class CustomCommands
    {
        public string InstallFolder { get; } = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);


        public ICommand Connect { get; } = new DelegateCommand<MasterViewModel>(vm => RequestUri(vm));

        public ICommand Export { get; } = new DelegateCommand<MasterViewModel>(ExportCSV);

        public ICommand Exit { get; } = new DelegateCommand<MasterViewModel>(ExitApplication);

        public ICommand Browse { get; } = new DelegateCommand<string>(s => Process.Start(s), s => !string.IsNullOrWhiteSpace(s));

        public ICommand Delete { get; } = new DelegateCommand<MasterViewModel>(DeleteDevice);

        public ICommand About { get; } = new DelegateCommand<CustomCommands>(c => new AboutBox() { DataContext = c }.ShowDialog());

        public ICommand ManualStatusUpdate { get; } = new DelegateCommand<MasterViewModel>(vm => _ = vm.UpdateStatus(vm.SelectedDevice));

        public ICommand ManualHistoryUpdate { get; } = new DelegateCommand<MasterViewModel>(vm => _ = vm.UpdateHistory(false));


        private static void RequestUri(MasterViewModel masterVM, string def = "http://enterAdressOrIP")
        {
            var input = new InputBox() { Title="Connect...", Label="_Device address", Text = def };
            if (input.ShowDialog().GetValueOrDefault())
            {
                if (Uri.IsWellFormedUriString(input.Text, UriKind.Absolute))
                {
                    var vm = new DeviceViewModel(new Uri(input.Text));
                    masterVM.Devices.Add(vm);
                    _ = masterVM.UpdateStatus(vm);
                    _ = masterVM.UpdateHistory(false);
                }
                else
                {
                    MessageBox.Show($"\"{input.Text}\" isn't a valid device address.", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                                    MessageBoxOptions.DefaultDesktopOnly);
                    RequestUri(masterVM, input.Text);
                }
            }
        }

        private static void DeleteDevice(MasterViewModel vm)
        {
            var displayName = String.IsNullOrWhiteSpace(vm.SelectedDevice.Name) ? vm.SelectedDevice.Address.AbsoluteUri : vm.SelectedDevice.Name;
            if (MessageBox.Show($"Do you really want to remove \"{displayName}\"?", "Confirm Delete", MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
            {
                vm.Devices.Remove(vm.SelectedDevice);
            }
        }

        private static void ExitApplication(MasterViewModel vm)
        {
            if (MessageBox.Show("Do you really want to quit?", "Shutting down...", MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
            {
                vm.Shutdown();
                Application.Current.Shutdown();
            }
        }

        private static void ExportCSV(MasterViewModel vm)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "export.csv",
                Filter = "CSV file (*.csv)|*.csv|Text file(*.txt)|*.txt|All files|*.*",
                DefaultExt = ".csv",
                OverwritePrompt = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filter = MessageBox.Show($"Do you want to apply the chosen filters to the export?", "Export Details", MessageBoxButton.YesNo,
                             MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes;

                if (vm.History.Export(saveFileDialog.FileName, filter))
                {
                    MessageBox.Show("Export completed successfully.", "Export Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Export failed. See Log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}