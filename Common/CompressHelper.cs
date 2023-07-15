using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Roim.Common
{
    public class CompressHelper
    {
        /// <summary>
        /// 单文件压缩（生成的压缩包和第三方的解压软件兼容）
        /// </summary>
        /// <param name="sourceFilePath">虚拟路径</param>
        /// <returns></returns>
        public static string CompressSingle(string sourceFilePath)
        {
            string zipFileName = sourceFilePath + ".gz";
            var path = Directory.GetCurrentDirectory();
            using (FileStream sourceFileStream = new FileInfo(string.Format("{0}{1}", path, zipFileName)).OpenRead())
            {
                using (FileStream zipFileStream = File.Create(string.Format("{0}{1}", path, zipFileName)))
                {
                    using (GZipStream zipStream = new GZipStream(zipFileStream, CompressionMode.Compress))
                    {
                        sourceFileStream.CopyTo(zipStream);
                    }
                }
            }
            return zipFileName;
        }

        /// <summary>   
        /// 压缩文件   
        /// </summary>   
        /// <param name="fileToZip">要压缩的文件全名</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码</param>   
        /// <returns>压缩结果</returns>   
        public static string ZipFile(string sourceFilePath, string password = null)
        {
            string zipFileName = sourceFilePath + ".zip";
            var path = Directory.GetCurrentDirectory();
            string fileToZip = string.Format("{0}{1}", path, sourceFilePath);
            string zipedFile = string.Format("{0}{1}", path, zipFileName);

            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZip))
                return sourceFilePath;

            try
            {
                fs = File.OpenRead(fileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFile);
                zipStream = new ZipOutputStream(fs);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                ent = new ZipEntry(Path.GetFileName(fileToZip));
                zipStream.PutNextEntry(ent);
                zipStream.SetLevel(6);

                zipStream.Write(buffer, 0, buffer.Length);

            }
            catch
            {

            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            GC.Collect();
            GC.Collect(1);

            return zipFileName;
        }

    }
}
