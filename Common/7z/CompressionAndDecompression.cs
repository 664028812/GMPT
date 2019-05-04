using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common._7z
{
    public class CompressionAndDecompression
    {
        private static CompressionAndDecompression Instance;

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        private string dllPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.dll");
        private CompressionAndDecompression()
        {
            SevenZipExtractor.SetLibraryPath(this.dllPath);
        }

        public static CompressionAndDecompression GetInstance()
        {
            if (Instance == null)
            {
                lock (locker)
                {
                    if (Instance == null)
                    {
                        Instance = new CompressionAndDecompression();
                    }
                }
            }
            return Instance;
        }

        /// <summary>
        /// 解压
        /// </summary>
        public bool Decompression(string filePath, string savePath)
        {
            bool res = true;
            try
            {
                if (!File.Exists(filePath))
                    Console.WriteLine("文件不存在");
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (SevenZipExtractor tmp = new SevenZipExtractor(filePath))
                {
                    for (int i = 0; i < tmp.ArchiveFileData.Count; i++)
                    {
                        tmp.ExtractFiles(savePath, tmp.ArchiveFileData[i].Index);
                        //tmp.ExtractFiles(savePath, tmp.ArchiveFileData[i].FileName);
                    }
                    res = true;
                }
            }
            catch (Exception ex)
            {
                res = false;
            }
            return res;
        }
    }
}
