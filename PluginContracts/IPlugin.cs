using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;

namespace DGLablib.PluginContracts
{
    [InheritedExport]
    public interface IPlugin
    {
        public string Name { get; }
        public string? Description { get; }
        public Dictionary<string, string> Settings { get; }
        public void Init(CoyoteDeviceV3 dev, CancellationToken ctl);
        public void Stop(CoyoteDeviceV3 dev, CancellationToken ctl);
    }
}

