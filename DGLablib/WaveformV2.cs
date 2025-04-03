using System;
using System.Runtime.InteropServices;

namespace DGLablib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveformV2
    {
        public byte StrengthA; // A通道强度设定值
        public byte StrengthB; // B通道强度设定值
        public byte X_A; // A通道波形参数X
        public byte Y_A; // A通道波形参数Y
        public byte Z_A; // A通道波形参数Z
        public byte X_B; // B通道波形参数X
        public byte Y_B; // B通道波形参数Y
        public byte Z_B; // B通道波形参数Z

        public WaveformV2(byte? strengthA = null, byte? strengthB = null, byte? x_A = null, byte? y_A = null, byte? z_A = null, byte? x_B = null, byte? y_B = null, byte? z_B = null)
        {
            StrengthA = strengthA ?? 0;
            StrengthB = strengthB ?? 0;
            X_A = x_A ?? 0;
            Y_A = y_A ?? 0;
            Z_A = z_A ?? 0;
            X_B = x_B ?? 0;
            Y_B = y_B ?? 0;
            Z_B = z_B ?? 0;
        }

        public static implicit operator byte[](WaveformV2 waveform)
        {
            byte[] v2Data = new byte[9];
            // 合成 PWM_AB2 数据
            int pwmAB2 = (waveform.StrengthA << 11) | waveform.StrengthB;
            byte[] pwmAB2Bytes = BitConverter.GetBytes(pwmAB2);
            // 合成 PWM_A34 数据
            int pwmA34 = (waveform.Z_A << 15) | (waveform.Y_A << 5) | waveform.X_A;
            byte[] pwmA34Bytes = BitConverter.GetBytes(pwmA34);
            // 合成 PWM_B34 数据
            int pwmB34 = (waveform.Z_B << 15) | (waveform.Y_B << 5) | waveform.X_B;
            byte[] pwmB34Bytes = BitConverter.GetBytes(pwmB34);
            Buffer.BlockCopy(pwmAB2Bytes, 0, v2Data, 0, 3);
            Buffer.BlockCopy(pwmA34Bytes, 0, v2Data, 3, 3);
            Buffer.BlockCopy(pwmB34Bytes, 0, v2Data, 6, 3);
            return v2Data;
        }
    }
}