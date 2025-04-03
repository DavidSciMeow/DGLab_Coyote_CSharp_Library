using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DGLablib
{
    /// <summary>
    /// BF指令构造
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformBF
    {
        /// <summary>
        /// 指令头
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte Head;
        /// <summary>
        /// A通道强度软上限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitA;
        /// <summary>
        /// B通道强度软上限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitB;
        /// <summary>
        /// A通道波形频率平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaA;
        /// <summary>
        /// B通道波形频率平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaB;
        /// <summary>
        /// A通道波形强度平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaA;
        /// <summary>
        /// B通道波形强度平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaB;

        /// <summary>
        /// 构造BF指令
        /// </summary>
        /// <param name="strengthUpperLimitA">A通道强度软上限</param>
        /// <param name="strengthUpperLimitB">B通道强度软上限</param>
        /// <param name="strengthFormParaA">A通道波形频率平衡参数</param>
        /// <param name="strengthFormParaB">B通道波形频率平衡参数</param>
        /// <param name="strengthVoltParaA">A通道波形强度平衡参数</param>
        /// <param name="strengthVoltParaB">B通道波形强度平衡参数</param>
        public WaveformBF(byte? strengthUpperLimitA = null, byte? strengthUpperLimitB = null, byte? strengthFormParaA = null, byte? strengthFormParaB = null, byte? strengthVoltParaA = null, byte? strengthVoltParaB = null)
        {
            Head = 0xBF;
            StrengthUpperLimitA = strengthUpperLimitA ?? 0;
            StrengthUpperLimitB = strengthUpperLimitB ?? 0;
            StrengthFormParaA = strengthFormParaA ?? 0;
            StrengthFormParaB = strengthFormParaB ?? 0;
            StrengthVoltParaA = strengthVoltParaA ?? 0;
            StrengthVoltParaB = strengthVoltParaB ?? 0;
        }
        /// <summary>
        /// 隐式转换为byte[]
        /// </summary>
        /// <param name="waveform">BF指令</param>
        public static implicit operator byte[](WaveformBF waveform)
        {
            byte[] arr = new byte[7];
            IntPtr ptr = Marshal.AllocHGlobal(arr.Length);
            Marshal.StructureToPtr(waveform, ptr, true);
            Marshal.Copy(ptr, arr, 0, arr.Length);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}