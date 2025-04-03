using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using DGLablib;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            Console.WriteLine("Scanning for Coyote devices...");
            List<CoyoteDeviceV3> devices = await CoyoteDeviceV3.Scan();

            if (devices.Count == 0)
            {
                Console.WriteLine("No Coyote devices found.");
                return;
            }

            Console.WriteLine($"Found {devices.Count} Coyote device(s):");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {devices[i].Name}");
            }

            var coyoteDevice = devices[0];

            coyoteDevice.SetWaveBFAsync(new WaveformBF(200)).Wait();
            coyoteDevice.Start();

            //coyoteDevice.NotificationReceived += (s, e) =>
            //{
            //    Console.WriteLine($"Notification received: {BitConverter.ToString(e)}");
            //};

            //coyoteDevice.B1MessageReceived += (s, e) =>
            //{
            //    Console.WriteLine($"Number:{s}, intA/B [{e[0]}][{e[1]}]");
            //};

            Console.WriteLine($"Connecting to {coyoteDevice.Name}...");
            Console.WriteLine($"Battery level: {coyoteDevice.BatteryLevel}%");

            // Example: Set waveform
            WaveformV3 waveform = new(150, [100, 100, 100, 100], [60, 60, 60, 60]);
            WaveformV3 zeroform = new();

            Console.WriteLine("Press A / B to change waveform or ESC to stop");
            
            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.A:
                        Console.WriteLine("set to wave");
                        coyoteDevice.WaveNow = waveform;
                        break;
                    case ConsoleKey.B:
                        Console.WriteLine("set to zero");
                        coyoteDevice.WaveNow = zeroform;
                        break;
                    case ConsoleKey.Escape: return;
                }
            }
        }
    }
}