using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DGLablib
{
    /// <summary>
    /// BF指令构造
    /// </summary>
    /// <remarks>
    /// 构造BF指令
    /// </remarks>
    /// <param name="strengthUpperLimitA">A通道强度软上限</param>
    /// <param name="strengthUpperLimitB">B通道强度软上限</param>
    /// <param name="strengthFormParaA">A通道波形频率平衡参数</param>
    /// <param name="strengthFormParaB">B通道波形频率平衡参数</param>
    /// <param name="strengthVoltParaA">A通道波形强度平衡参数</param>
    /// <param name="strengthVoltParaB">B通道波形强度平衡参数</param>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformBF(byte? strengthUpperLimitA = null, byte? strengthUpperLimitB = null, byte? strengthFormParaA = null, byte? strengthFormParaB = null, byte? strengthVoltParaA = null, byte? strengthVoltParaB = null)
    {
        /// <summary>
        /// 指令头
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte Head = 0xBF;
        /// <summary>
        /// A通道强度软上限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitA = strengthUpperLimitA ?? 0;
        /// <summary>
        /// B通道强度软上限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitB = strengthUpperLimitB ?? 0;
        /// <summary>
        /// A通道波形频率平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaA = strengthFormParaA ?? 0;
        /// <summary>
        /// B通道波形频率平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaB = strengthFormParaB ?? 0;
        /// <summary>
        /// A通道波形强度平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaA = strengthVoltParaA ?? 0;
        /// <summary>
        /// B通道波形强度平衡参数
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaB = strengthVoltParaB ?? 0;

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