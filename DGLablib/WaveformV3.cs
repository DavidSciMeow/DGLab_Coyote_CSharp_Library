using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DGLablib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformV3
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte Head; // 指令头
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthMode; // 强度值解读方式
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthA; // A通道强度设定值
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthB; // B通道强度设定值
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] FrequencyA = [0, 0, 0, 0]; // A通道波形频率，4个字节
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] IntensityA = [0, 0, 0, 0]; // A通道波形强度，4个字节
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] FrequencyB = [0, 0, 0, 0]; // B通道波形频率，4个字节
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] IntensityB = [0, 0, 0, 0]; // B通道波形强度，4个字节

        public byte Number;
        public readonly int MilisecondLastA => GetMiliLast(FrequencyA);
        public readonly int MilisecondLastB => GetMiliLast(FrequencyB);

        public WaveformV3(byte? strengthA = null, byte? strengthB = null, byte[]? frequencyA = null, byte[]? intensityA = null, byte[]? frequencyB = null, byte[]? intensityB = null)
        {
            Head = 0xB0;
            Number = (byte)(Random.Shared.Next(0b0000, 0b1111));
            StrengthMode = (byte)(Number << 4 | 0b1111);
            StrengthA = strengthA ?? 0;
            StrengthB = strengthB ?? 0;
            FrequencyA = frequencyA ?? [0, 0, 0, 0];
            IntensityA = intensityA ?? [0, 0, 0, 0];
            FrequencyB = frequencyB ?? [0, 0, 0, 0];
            IntensityB = intensityB ?? [0, 0, 0, 0];
        }
        public WaveformV3(byte? strength = null, byte[]? frequency = null, byte[]? intensity = null)
        {
            Head = 0xB0;
            Number = (byte)(Random.Shared.Next(0b0000, 0b1111));
            StrengthMode = (byte)(Number << 4 | 0b1111);
            StrengthA = strength ?? 0;
            StrengthB = 0;
            FrequencyA = frequency ?? [0, 0, 0, 0];
            IntensityA = intensity ?? [0, 0, 0, 0];
            FrequencyB = [0,0,0,0];
            IntensityB = [0,0,0,101];
        }
        public WaveformV3(byte? strength = null, byte[]? frequency = null)
        {
            Head = 0xB0;
            Number = (byte)(Random.Shared.Next(0b0000, 0b1111));
            StrengthMode = (byte)(Number << 4 | 0b1111);
            StrengthA = strength ?? 0;
            StrengthB = 0;
            FrequencyA = frequency ?? [0, 0, 0, 0];
            IntensityA = new byte[4] { StrengthA, StrengthA, StrengthA, StrengthA } ?? [0, 0, 0, 0];
            FrequencyB = [0, 0, 0, 0];
            IntensityB = [0, 0, 0, 101];
        }

        public static implicit operator byte[](WaveformV3 waveform)
        {
            byte[] arr = new byte[20];
            IntPtr ptr = Marshal.AllocHGlobal(arr.Length);
            Marshal.StructureToPtr(waveform, ptr, true);
            Marshal.Copy(ptr, arr, 0, arr.Length);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
        public override readonly string ToString()
        {
            static string FormatWaveData(string waveType, byte strength, byte[] frequency, byte[] intensity)
            {
                return $"{waveType}:{strength} [" +
                       $"{MapValueToMilliseconds(frequency[0])}/{intensity[0]}," +
                       $"{MapValueToMilliseconds(frequency[1])}/{intensity[1]}," +
                       $"{MapValueToMilliseconds(frequency[2])}/{intensity[2]}," +
                       $"{MapValueToMilliseconds(frequency[3])}/{intensity[3]}]";
            }

            string baseString = BitConverter.ToString(this);

            if (FrequencyA != null && IntensityA != null && StrengthA != 0 && StrengthB == 0)
            {
                return $"{baseString}\n{FormatWaveData("WAVE A", StrengthA, FrequencyA, IntensityA)}";
            }
            else if (FrequencyB != null && IntensityB != null && StrengthB != 0 && StrengthA == 0)
            {
                return $"{baseString}\n{FormatWaveData("WAVE B", StrengthB, FrequencyB, IntensityB)}";
            }
            else if (FrequencyA != null && IntensityA != null && FrequencyB != null && IntensityB != null)
            {
                return $"{baseString}\n{FormatWaveData("WAVE A", StrengthA, FrequencyA, IntensityA)}\n{FormatWaveData("WAVE B", StrengthB, FrequencyB, IntensityB)}";
            }
            else
            {
                return baseString;
            }
        }

        static int GetMiliLast(byte[] freq)
        {
            if (freq == null) return 100;
            int lst = 0;
            foreach(var i in freq)
            {
                lst += (int)MapValueToMilliseconds(i);
            }
            return lst;
        }
        public static double MapValueToMilliseconds(double value)
        {
            if (value >= 100 && value <= 180)
            {
                return 5 * value - 400;
            }
            else if (value > 180 && value <= 200)
            {
                return 5 * (value - 180) + 500;
            }
            else if (value > 200 && value <= 240)
            {
                return 10 * (value - 200) + 600;
            }
            else
            {
                return 100;
            }
        }
        public static byte MapValueToByte(double milisec) => (byte)(milisec * (7 / 45) + 84.4444);
    }
}