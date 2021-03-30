using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YouLessAPI;
using WPFMVVM.ViewModel;

namespace YouLessMonitor.ViewModel
{
    public class DeviceViewModel : BaseVM
    {
        private string _Name;
        private Uri _Address;
        private bool _Status;

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { UpdateProperty(ref _Name, value); }
        }

        /// <summary>
        /// Gets or sets the device address.
        /// </summary>
        public Uri Address
        {
            get { return _Address; }
            private set { UpdateProperty(ref _Address, value); }
        }


        /// <summary>
        /// Gets or sets the device status.
        /// </summary>
        public bool Status
        {
            get { return _Status; }
            set { UpdateProperty(ref _Status, value); }
        }

        public IEnumerable<(DateTime Time, int Power)> History { get; private set; }


        // RELAYs ###########################################

        /// <summary>
        /// Counter in kWh
        /// </summary>
        public double TotalKwH { get; private set; }

        /// <summary>
        /// Power consumption in Watt
        /// </summary>
        public int Consumption { get; private set; }

        /// <summary>
        /// Moving average level (intensity of reflected light on analog meters)
        /// </summary>
        public string Sensor { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewModel"/> class.
        /// </summary>
        /// <param name="address">The device address.</param>
        public DeviceViewModel(Uri address)
        {
            Name = String.Empty;
            Address = address ?? throw new ArgumentNullException(nameof(address));
            if (!address.IsAbsoluteUri) throw new ArgumentException(nameof(address));
            History = Enumerable.Empty<(DateTime Time, int Power)>();
        }
        public DeviceViewModel(BinaryReader reader) : this(new Uri(reader.ReadString()))
        {
            Deserialize(reader);
        }


        public void UpdateStatus(StatusInfo info)
        {
            if (info == null) return;

            UpdateProperty(nameof(TotalKwH), v => TotalKwH = v, TotalKwH, double.TryParse(info.TotalKwH.Trim(), out double kwh) ? kwh : -1);
            UpdateProperty(nameof(Consumption), v => Consumption = v, Consumption, info.Consumption);

            var start = info.ReflectionDeviation.IndexOf(';') + 1;
            var end = info.ReflectionDeviation.Length - 2;
            var raw = info.ReflectionDeviation.Substring(start, end - start);
            var deviation = int.TryParse(raw, out int d) ? d : 9001;
            UpdateProperty(nameof(Sensor), v => Sensor = v, Sensor, $"{info.ReflectionLevelAverage}% +/- {deviation}");
        }

        internal void UpdateHistory(History history)
        {
            var temp = new (DateTime Time, int Power)[history.Samples.Count];
            for (int i = 0; i < temp.Length; i++)
            {
                var time = history.StartTime + TimeSpan.FromSeconds(history.Delta * i);
                var power = int.TryParse(history.Samples[i], out int p) ? p : -1;
                temp[i] = (time, power);
            }
            History = History.Concat(temp).Distinct().OrderBy(p => p.Time).ToArray();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Address.AbsoluteUri);
            writer.Write(Name);
            writer.Write(History.Count());
            foreach (var item in History)
            {
                writer.Write(item.Time.ToUniversalTime().ToBinary());
                writer.Write(item.Power);
            }
        }

        private void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();
            var temp = new (DateTime Time, int Power)[reader.ReadInt32()];
            for (int i = 0; i < temp.Length; i++)
            {
                var time = DateTime.FromBinary(reader.ReadInt64()).ToLocalTime();
                var power = reader.ReadInt32();
                temp[i] = (time, power);
            }
            History = temp;
        }

        internal string GetDisplayName()
        {
            if (!String.IsNullOrWhiteSpace(Name)) return Name;
            return new String(Address.Host.Select(c => Char.IsPunctuation(c) ? ' ' : c).ToArray());
        }
    }
}