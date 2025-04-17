using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace DGLablib
{
    /// <summary>
    /// B0指令构造
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformV3
    {
        /// <summary>
        /// 指令头
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        [JsonProperty]
        public byte Head;
        /// <summary>
        /// 强度值解读方式
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        [JsonProperty]
        public byte StrengthMode;
        /// <summary>
        /// A通道强度设定值
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        [JsonProperty]
        public byte StrengthA;
        /// <summary>
        /// B通道强度设定值
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        [JsonProperty]
        public byte StrengthB;
        /// <summary>
        /// A通道波形频率，4个字节
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        [JsonProperty]
        public byte[] FrequencyA = [0, 0, 0, 0];
        /// <summary>
        /// A通道波形强度，4个字节
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        [JsonProperty]
        public byte[] IntensityA = [0, 0, 0, 0];
        /// <summary>
        /// B通道波形频率，4个字节
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        [JsonProperty]
        public byte[] FrequencyB = [0, 0, 0, 0];
        /// <summary>
        /// B通道波形强度，4个字节
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        [JsonProperty]
        public byte[] IntensityB = [0, 0, 0, 0];

        /// <summary>
        /// 序列号
        /// </summary>
        [JsonIgnore]
        public byte Number;
        /// <summary>
        /// A通道波形持续时间
        /// </summary>
        [JsonIgnore]
        public readonly int MilisecondLastA => GetMiliLast(FrequencyA);
        /// <summary>
        /// B通道波形持续时间
        /// </summary>
        [JsonIgnore]
        public readonly int MilisecondLastB => GetMiliLast(FrequencyB);

        /// <summary>
        /// 构造B0函数
        /// </summary>
        /// <param name="strengthA">A通道强度设定值</param>
        /// <param name="strengthB">B通道强度设定值</param>
        /// <param name="frequencyA">A通道波形频率，4个字节</param>
        /// <param name="intensityA">A通道波形强度，4个字节</param>
        /// <param name="frequencyB">B通道波形频率，4个字节</param>
        /// <param name="intensityB">B通道波形强度，4个字节</param>
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
        /// <summary>
        /// 构造B0函数(AB通道相同)
        /// </summary>
        /// <param name="strength">通道强度设定值</param>
        /// <param name="frequency">通道波形频率，4个字节</param>
        /// <param name="intensity">通道波形强度，4个字节</param>
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
        /// <summary>
        /// 构造B0函数(快速输出A通道)
        /// </summary>
        /// <param name="strength">通道强度设定值</param>
        /// <param name="frequency">通道波形频率，4个字节</param>
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

        /// <summary>
        /// 隐式转换byte[]
        /// </summary>
        /// <param name="waveform">波形模式</param>
        public static implicit operator byte[](WaveformV3 waveform)
        {
            byte[] arr = new byte[20];
            IntPtr ptr = Marshal.AllocHGlobal(arr.Length);
            Marshal.StructureToPtr(waveform, ptr, true);
            Marshal.Copy(ptr, arr, 0, arr.Length);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取持续时间
        /// </summary>
        /// <param name="freq">频率列</param>
        /// <returns></returns>
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
        /// <summary>
        /// V3映射频率到毫秒
        /// </summary>
        /// <param name="value">频率值</param>
        /// <returns>毫秒值</returns>
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
        /// <summary>
        /// V3映射毫秒到频率byte[]
        /// </summary>
        /// <param name="milisec">毫秒值</param>
        /// <returns>频率值</returns>
        public static byte MapValueToByte(double milisec) => (byte)(milisec * (7 / 45) + 84.4444);
    }
}