namespace DGLablib
{
    public readonly struct CoyoteV3
    {
        public static string Name { get; } = "47L121000";
        public static string WirelessSensorName { get; } = "47L120100";
        public static Guid ServiceWrite { get; } = new Guid("0000180c-0000-1000-8000-00805f9b34fb");
        public static Guid ServiceNotify { get; } = new Guid("0000180c-0000-1000-8000-00805f9b34fb");
        public static Guid CharacteristicWrite { get; } = new Guid("0000150A-0000-1000-8000-00805f9b34fb");
        public static Guid CharacteristicNotify { get; } = new Guid("0000150B-0000-1000-8000-00805f9b34fb");
        public static Guid BatteryData { get; } = new Guid("00001500-0000-1000-8000-00805f9b34fb");
    }
}