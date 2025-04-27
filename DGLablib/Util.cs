using System.Diagnostics;
using Windows.Devices.Radios;

namespace DGLablib
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 检查蓝牙状态
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> IsBluetoothEnabledAsync()
        {
            foreach (var radio in await Radio.GetRadiosAsync())
            {
                if (radio.Kind == RadioKind.Bluetooth)
                {
                    return radio.State == RadioState.On;
                }
            }
            return false;
        }
        /// <summary>
        /// 蓝牙请求开启
        /// </summary>
        /// <returns></returns>
        public static async Task RequestEnableBluetoothAsync()
        {
            foreach (var radio in await Radio.GetRadiosAsync())
            {
                if (radio.Kind == RadioKind.Bluetooth && radio.State != RadioState.On)
                {
                    var result = await radio.SetStateAsync(RadioState.On);
                    if (result == RadioAccessStatus.Allowed)
                    {
                        Debug.WriteLine("Bluetooth has been enabled.");
                    }
                    else
                    {
                        Debug.WriteLine("Please enable Bluetooth manually.");
                    }
                }
            }
        }
    }
}
