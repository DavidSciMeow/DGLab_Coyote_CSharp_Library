using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DGLablib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformBF
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte Head; // 指令头
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitA; // A通道强度软上限
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthUpperLimitB; // B通道强度软上限
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaA; // A通道波形频率平衡参数
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthFormParaB; // B通道波形频率平衡参数
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaA; // A通道波形强度平衡参数
        [MarshalAs(UnmanagedType.U1)]
        public byte StrengthVoltParaB; // B通道波形强度平衡参数

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