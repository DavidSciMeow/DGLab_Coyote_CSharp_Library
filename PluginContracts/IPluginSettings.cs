using System.Collections.Generic;

namespace DGLablib.PluginContracts
{
    public interface IPluginSettings
    {
        Dictionary<string, object> Settings { get; }
    }
}

