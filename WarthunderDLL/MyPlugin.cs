using DGLablib;
using DGLablib.PluginContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace WarthunderDLL
{
    [Export(typeof(IPlugin))]
    public class MyPlugin : IPlugin
    {
        public string Name => "战雷郊狼控制器";
        public string? Description => "";
        public Dictionary<string, string> Settings { get; } = [];


        public void Init(CoyoteDeviceV3 dev, CancellationToken ctl)
        {
            Settings["WaveformFrequency"] = 60.ToString();
            Settings["WaveformIntensity"] = 30.ToString();

            dev.Start();

            while (true)
            {
                if (ctl.IsCancellationRequested) return;
                var frequency = int.Parse(Settings["WaveformFrequency"]);
                var intensity = int.Parse(Settings["WaveformIntensity"]);
                byte _frequency = frequency > 255 ? (byte)255 : (byte)frequency;
                byte _intensity = intensity > 255 ? (byte)255 : (byte)intensity;
                var wav1 = new WaveformV3(_intensity, [_frequency, _frequency, _frequency, _frequency]);
                dev.WaveNow = wav1;
                try
                {
                    Task.Delay(1000, ctl).Wait(ctl);
                }
                catch
                {

                }
            }
        }

        public void Stop(CoyoteDeviceV3 dev, CancellationToken ctl) => dev.Stop();
    }
}
