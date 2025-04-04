namespace DGLablib
{
    /// <summary>
    /// 郊狼3实例
    /// </summary>
    public readonly struct CoyoteV3
    {
        /// <summary>
        /// 实例名
        /// </summary>
        public static string Name { get; } = "47L121000";
        /// <summary>
        /// 实例无线识别名
        /// </summary>
        public static string WirelessSensorName { get; } = "47L120100";
        /// <summary>
        /// 写服务特性ID
        /// </summary>
        public static Guid ServiceWrite { get; } = new Guid("0000180c-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// 通知服务特性ID
        /// </summary>
        public static Guid ServiceNotify { get; } = new Guid("0000180c-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// 写ID
        /// </summary>
        public static Guid CharacteristicWrite { get; } = new Guid("0000150A-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// 通知ID 
        /// </summary>
        public static Guid CharacteristicNotify { get; } = new Guid("0000150B-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// 电量信息
        /// </summary>
        public static Guid BatteryData { get; } = new Guid("00001500-0000-1000-8000-00805f9b34fb");
    }
}