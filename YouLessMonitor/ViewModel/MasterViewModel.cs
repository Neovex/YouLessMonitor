using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using YouLessAPI;
using System.Collections.ObjectModel;
using YouLessMonitor.Commands;
using System.Windows.Threading;

namespace YouLessMonitor.ViewModel
{
    public class MasterViewModel : ViewModelBase
    {
        private const string _FILE = "history.bin";
        private const int _IMPULSE = 30;

        private DispatcherTimer _Timer;

        private int _StatusCounter=10;
        private ObservableCollection<DeviceViewModel> _Devices;
        private DeviceViewModel _SelectedDevice;

        private int _HistoryCounter=400;
        private HistoryViewModel _History;



        public CustomCommands Commands { get; }

        public int StatusCounter
        {
            get { return _StatusCounter; }
            private set { UpdateProperty(ref _StatusCounter, value); }
        }

        public int HistoryCounter
        {
            get { return _HistoryCounter; }
            private set { UpdateProperty(ref _HistoryCounter, value); }
        }

        public ObservableCollection<DeviceViewModel> Devices
        {
            get { return _Devices; }
            private set { UpdateProperty(ref _Devices, value); }
        }
        public DeviceViewModel SelectedDevice
        {
            get { return _SelectedDevice; }
            set { UpdateProperty(ref _SelectedDevice, value); }
        }

        public HistoryViewModel History
        {
            get { return _History; }
            private set { UpdateProperty(ref _History, value); }
        }




        public MasterViewModel()
        {
            Commands = new CustomCommands();
            Devices = new ObservableCollection<DeviceViewModel>();
            History = new HistoryViewModel(Devices);
        }

        public void Start()
        {
            Load();
            History.UpdateView();

            Log.Debug("Starting update cycle");
            if (_Timer == null)
            {
                _Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                _Timer.Tick += Timer_Tick;
            }
            _Timer.Start();
        }

        public void Shutdown()
        {
            Log.Debug("Stopping update cycle");
            _Timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            StatusCounter--;
            if (StatusCounter <= 0)
            {
                StatusCounter = 60;
                Log.Debug("Running scheduled status updates");
                _ = Task.WhenAll(Devices.Select(UpdateStatus));
            }

            HistoryCounter--;
            if (HistoryCounter <= 0)
            {
                HistoryCounter = 3600;
                _ = UpdateHistory(true);
            }
        }

        // STATUS ########################################################################

        internal async Task UpdateStatus(DeviceViewModel deviceVm)
        {
            try
            {
                Log.Debug(deviceVm.Address.AbsoluteUri, "Running status update");

                // Connect
                var service = new YouLessService(deviceVm.Address);
                // Update Status
                var status = await service.GetStatus();
                deviceVm.UpdateStatus(status);
                deviceVm.Status = true;

                Log.Debug(deviceVm.Address.AbsoluteUri, "done");
            }
            catch (Exception ex)
            {
                deviceVm.Status = false;
                Log.Debug(deviceVm.Address.AbsoluteUri, "FAILED");
                Log.Error(deviceVm.Address.AbsoluteUri, ex);
            }
        }


        // HISTORY ##############################################################################

        internal async Task UpdateHistory(bool scheduled)
        {
            if (scheduled) Log.Info("Running scheduled history updates");
            else History.EndFilter = DateTime.Now;

            await Task.WhenAll(Devices.Select(UpdateHistory));
            _History.UpdateView();
            if (scheduled) Log.Info("Done");
        }

        private async Task UpdateHistory(DeviceViewModel deviceVm)
        {
            try
            {
                Log.Debug(deviceVm.Address.AbsoluteUri, "Running history update");

                // Figure out how much history needs to be loaded
                // Get last entry from history of device X
                var lastUpdate = Devices.First(d => d.Address == deviceVm.Address).History.LastOrDefault().Time;
                // Calculate required history pages (max20)
                var timePassed = DateTime.Now - lastUpdate;
                var requiredPages = (int)Math.Ceiling(timePassed.TotalMinutes / _IMPULSE);
                Log.Debug(deviceVm.Address.AbsoluteUri, "Last value was from", lastUpdate, "therefore pages required:", requiredPages);
                if (requiredPages <= 0) requiredPages = 1;
                else if (requiredPages > 20) requiredPages = 20;
                // Connect
                var service = new YouLessService(deviceVm.Address);
                // get each page and add to devices history
                for (int i = requiredPages; i != 0; i--)
                {
                    // Update History
                    var history = await service.GetHistory(i);
                    deviceVm.UpdateHistory(history);
                }

                // Save
                Save();

                Log.Debug(deviceVm.Address.AbsoluteUri, "done");
            }
            catch (Exception ex)
            {
                Log.Debug(deviceVm.Address.AbsoluteUri, "FAILED");
                Log.Error(deviceVm.Address.AbsoluteUri, ex);
            }
        }

        // IO ##############################################################################

        private void Load()
        {
            if (File.Exists(_FILE))
            {
                Log.Debug("Loading device history");
                using (var fs = new FileStream(_FILE, FileMode.OpenOrCreate))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        var ammount = reader.ReadInt32();
                        for (int i = 0; i < ammount; i++)
                        {
                            Devices.Add(new DeviceViewModel(reader));
                        }
                    }
                }
                Log.Debug("done");
            }
            else
            {
                Log.Warning("History file not found!");
            }
        }

        private void Save()
        {
            Log.Debug("Saving device history");
            using (var fs = new FileStream(_FILE, FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write(Devices.Count);
                    foreach (var device in Devices)
                    {
                        device.Serialize(writer);
                    }
                }
            }
            Log.Debug("done");
        }
    }
}