using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DGLablib
{
    /// <summary>
    /// 序列化工具类
    /// </summary>
    public class SB64
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="myStruct">结构</param>
        /// <returns></returns>
        public static string EncryptAndEncode<T>(T myStruct)
        {
            // 序列化
            string json = JsonConvert.SerializeObject(myStruct);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);
            // 压缩
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(plainBytes, 0, plainBytes.Length);
            }
            byte[] compressedBytes = outputStream.ToArray();
            // Base64 编码
            return Convert.ToBase64String(compressedBytes);
        }
        /// <summary>
        /// 解码
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="base64">串</param>
        /// <returns></returns>
        public static T? DecodeAndDecrypt<T>(string base64)
        {
            // Base64 解码
            byte[] compressedBytes = Convert.FromBase64String(base64);
            // 解压缩
            using var inputStream = new MemoryStream(compressedBytes);
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            gzipStream.CopyTo(outputStream);
            byte[] decompressedBytes = outputStream.ToArray();
            // 反序列化
            string json = Encoding.UTF8.GetString(decompressedBytes);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
