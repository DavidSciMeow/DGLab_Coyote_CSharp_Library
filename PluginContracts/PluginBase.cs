using System;
using System.Collections.Generic;
using System.Threading;

namespace DGLablib.PluginContracts
{
    public abstract class PluginBase : IPlugin, IPluginSettings
    {
        private readonly Dictionary<string, object> _settings = [];
        private readonly Lock _lock = new();

        // IPlugin 属性
        public abstract string Name { get; }
        public abstract string? Description { get; }
        public abstract void Init(CancellationToken ctl);

        public Action<string> Say { get; set; } = _ => { };
        public Action<WaveformV3> SetWave { get; set; } = _ => { };

        // IPluginSettings 属性
        public Dictionary<string, object> Settings { get; } = [];

        // IPluginSettings 方法
        public void UpdateSetting(string key, object value)
        {
            lock (_lock)
            {
                if (!_settings.TryAdd(key, value))
                {
                    _settings[key] = value;
                }
            }
        }

        // 提供一个方法供子类初始化默认设置
        protected void AddDefaultSetting(string key, object value)
        {
            lock (_lock)
            {
                _settings.TryAdd(key, value);
            }
        }
    }
}

