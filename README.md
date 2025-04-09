# CoyoteLibCSharp
## Nuget : [Meow.DGLablib](https://www.nuget.org/packages/Meow.DGLablib/)   
![Nuget包版本](https://img.shields.io/nuget/vpre/Meow.DGLablib?label=NuGet%20Version) 
![Nuget下载数](https://img.shields.io/nuget/dt/Meow.DGLablib) 
![GitHub last commit](https://img.shields.io/github/last-commit/DavidSciMeow/DGLab_Coyote_CSharp_Library) 
[![CodeFactor](https://www.codefactor.io/repository/github/davidscimeow/dglab_coyote_csharp_library/badge)](https://www.codefactor.io/repository/github/davidscimeow/dglab_coyote_csharp_library)
## 郊狼控制库 C#\[.NET\]版本 \(非跨平台/仅限Windows\) \(目前只有V3\)

### 基础使用方式
```csharp
static async Task Main(string[] args)
{
    
    Console.WriteLine("Scanning for Coyote devices...");
    List<CoyoteDeviceV3> devices = await CoyoteDeviceV3.Scan(); //通用的扫描方法

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

    var coyoteDevice = devices[0]; //选择一个对的

    coyoteDevice.SetWaveBFAsync(new WaveformBF(200)).Wait(); //设置最大幅值 (BF命令)
    coyoteDevice.Start(); //启动郊狼输出

    //coyoteDevice.NotificationReceived += (s, e) => //全局通知回调
    //{
    //    Console.WriteLine($"Notification received: {BitConverter.ToString(e)}");
    //};

    //coyoteDevice.B1MessageReceived += (s, e) => //B1通知回调
    //{
    //    Console.WriteLine($"Number:{s}, intA/B [{e[0]}][{e[1]}]");
    //};

    Console.WriteLine($"Connecting to {coyoteDevice.Name}...");
    Console.WriteLine($"Battery level: {coyoteDevice.BatteryLevel}%"); //读取电池电量

    // Example: Set waveform
    WaveformV3 waveform = new(150, [100, 100, 100, 100], [60, 60, 60, 60]); //仅输出到A通道的波形
    WaveformV3 zeroform = new(); //零幅值波形

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
```
