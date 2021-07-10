using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using OpenTabletDriver.Plugin.Devices;
using HidSharp.Reports;

namespace OpenTabletDriver.Devices
{
    public class HidSharpDeviceRootHub : IRootHub
    {
        private HidSharpDeviceRootHub()
        {
            foreach (var hidDevice in DeviceList.Local.GetHidDevices())
            {
                var reportDescriptor = hidDevice.GetReportDescriptor();
                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    foreach (var usage in deviceItem.Usages.GetAllValues())
                    {
                        Console.WriteLine(string.Format("Usage: {0:X4} {1}", usage, (Usage)usage));
                    }
                    foreach (var report in deviceItem.Reports)
                    {
                        Console.WriteLine(string.Format("{0}: ReportID={1}, Length={2}, Items={3}",
                                            report.ReportType, report.ReportID, report.Length, report.DataItems.Count));
                        foreach (var dataItem in report.DataItems)
                        {
                            Console.WriteLine(string.Format("  {0} Elements x {1} Bits, Units: {2}, Expected Usage Type: {3}, Flags: {4}, Usages: {5}",
                                dataItem.ElementCount, dataItem.ElementBits, dataItem.Unit.System, dataItem.ExpectedUsageType, dataItem.Flags,
                                string.Join(", ", dataItem.Usages.GetAllValues().Select(usage => usage.ToString("X4") + " " + ((Usage)usage).ToString()))));
                        }
                    }
                }

                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(BitConverter.ToString(hidDevice.GetRawReportDescriptor()).Replace("-", " "));
            }
            DeviceList.Local.Changed += (sender, e) =>
            {
                var newList = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
                var changes = new DevicesChangedEventArgs(hidDevices, newList);
                if (changes.Changes.Any())
                {
                    Plugin.Log.Debug(nameof(HidSharpDeviceRootHub.Current.DevicesChanged), $"Changes: {changes.Changes.Count()}, Add: {changes.Additions.Count()}, Remove: {changes.Removals.Count()}");
                    DevicesChanged?.Invoke(this, changes);
                    hidDevices = newList;
                }
            };
        }

        public static IRootHub Current { get; } = new HidSharpDeviceRootHub();

        private IEnumerable<IDeviceEndpoint> hidDevices = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
        }
    }
}
