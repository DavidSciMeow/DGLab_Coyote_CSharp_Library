using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace DGLablib.PluginContracts
{
    [InheritedExport]
    public interface IPlugin
    {
        string Name { get; }
        string? Description { get; }
        void Init(CancellationToken ctl);
        Action<WaveformV3> SetWave { get; set; }
        Action<string> Say { get; set; }
    }

    public abstract class PluginBase : IPlugin
    {
        public abstract string Name { get; }
        public abstract string? Description { get; }
        public abstract void Init(CancellationToken ctl);
        public Action<string> Say { get; set; } = null!;
        public Action<WaveformV3> SetWave { get; set; } = null!;
    }
}

