using DGLablib;
using DGLablib.PluginContracts;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace WarthunderDLL
{
    [Export(typeof(IPlugin))]
    public class MyPlugin : PluginBase
    {
        public override string Name => "战雷郊狼控制器";
        public override string? Description => "";
        public override void Init(CancellationToken ctl)
        {
            Say.Invoke("Plugin initialized.");
            while (true)
            {
                if (ctl.IsCancellationRequested)
                {
                    Say.Invoke("Plugin Stopped.");
                    return;
                }

                var wav1 = new WaveformV3(30, [60, 60, 60, 60]);
                SetWave.Invoke(wav1);
                Task.Delay(1000).Wait(ctl);
            }
        }
    }
}
