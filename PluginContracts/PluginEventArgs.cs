using System;

namespace DGLablib.PluginContracts
{
    public class PluginEventArgs : EventArgs
    {
        public string Message { get; set; }
        public WaveformV3 Data { get; set; }

        public PluginEventArgs(string message, WaveformV3 data)
        {
            Message = message;
            Data = data;
        }
    }
}
