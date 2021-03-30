using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using YouLessAPI;

namespace YouLessMonitor.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private IEnumerable<DeviceViewModel> _Devices;
        private DataView _View;
        private DateTime _StartFilter;
        private DateTime _EndFilter;


        public DataView View
        {
            get { return _View; }
            set { UpdateProperty(ref _View, value); }
        }

        public DateTime Startfilter
        {
            get { return _StartFilter; }
            set { UpdateProperty(ref _StartFilter, value); UpdateView(); }
        }

        public DateTime EndFilter
        {
            get { return _EndFilter; }
            set { UpdateProperty(ref _EndFilter, value); UpdateView(); }
        }


        public HistoryViewModel(IEnumerable<DeviceViewModel> devices)
        {
            _Devices = devices;
            View = new DataView();
            var now = DateTime.Now;
            now -= TimeSpan.FromMilliseconds(now.Millisecond);
            EndFilter = now;
            Startfilter = now - TimeSpan.FromHours(1);
        }


        internal void UpdateView()
        {
            var timeHeader = "Time";

            var view = new DataView();
            view.Table = new DataTable(nameof(DataTable));
            view.Table.Columns.Add(timeHeader, typeof(DateTime)).ReadOnly = true;
            foreach (var device in _Devices)
            {
                var col = view.Table.Columns.Add(device.GetDisplayName(), typeof(int), string.Empty);
                col.ReadOnly = true;
            }

            foreach (var time in _Devices.SelectMany(d => d.History).
                                      Select(e => e.Time).
                                      Where(t => t >= Startfilter && t <= EndFilter).
                                      Distinct().OrderBy(t => t))
            {
                var row = view.Table.NewRow();
                row[timeHeader] = time;
                foreach (var device in _Devices)
                {
                    row[device.GetDisplayName()] = device.History.Where(e => e.Time == time).
                                                                  DefaultIfEmpty((Time: default(DateTime), Power: -1)).
                                                                  First().Power;
                }
                view.Table.Rows.Add(row);
            }

            View = view;
        }

        internal bool Export(string filepath, bool useFilter)
        {
            try
            {
                Log.Info("Export started", _Devices.Count(), filepath, useFilter);
                using (var fs = new FileStream(filepath, FileMode.OpenOrCreate))
                using (var csv = new StreamWriter(fs))
                {
                    // HEADER
                    csv.Write("Date,Time");
                    foreach (var device in _Devices)
                    {
                        csv.Write(',');
                        csv.Write(device.GetDisplayName());
                    }
                    csv.WriteLine();

                    // DATA
                    var times = _Devices.SelectMany(d => d.History).Select(e => e.Time);
                    if (useFilter) times = times.Where(t => t >= Startfilter && t <= EndFilter);
                    times = times.Distinct().OrderBy(t => t);

                    foreach (var time in times)
                    {
                        csv.Write(time.ToShortDateString());
                        csv.Write(',');
                        csv.Write(time.ToShortTimeString());
                        csv.Write(',');
                        foreach (var device in _Devices)
                        {
                            csv.Write(device.History.Where(e => e.Time == time).
                                                     DefaultIfEmpty((Time: default(DateTime), Power: -1)).
                                                     First().Power);
                            csv.Write(',');
                        }
                        csv.WriteLine();
                    }
                    Log.Info("Export completed");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Export failed", ex);
                return false;
            }
        }
    }
}