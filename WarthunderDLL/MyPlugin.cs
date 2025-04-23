using DGLablib;
using DGLablib.PluginContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WarthunderDLL
{
    [Export(typeof(IPlugin))]
    public class MyPlugin : PluginBase
    {
        public override string Name => "战雷郊狼控制器";
        public override string? Description => "";
        public override void Init(CancellationToken ctl)
        {
            Settings["WaveformFrequency"] = 60;
            Settings["WaveformIntensity"] = 30;

            Say.Invoke("Plugin initialized.");
            while (true)
            {
                if (ctl.IsCancellationRequested)
                {
                    Say.Invoke("Plugin Stopped.");
                    return;
                }
                var frequency = (int)Settings["WaveformFrequency"];
                var intensity = (int)Settings["WaveformIntensity"];
                byte _frequency = frequency > 255 ? (byte)255 : (byte)frequency;
                byte _intensity = intensity > 255 ? (byte)255 : (byte)intensity;
                var wav1 = new WaveformV3(_intensity, [_frequency, _frequency, _frequency, _frequency]);
                SetWave.Invoke(wav1);
                try
                {
                    Task.Delay(1000, ctl).Wait(ctl);
                }
                catch
                {

                }
            }
        }
    }
}
