using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class pubtool
    {
        ///
        /// <summary>
        /// 压缩文件备份路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CreateCompressPath()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "fileback");
            return path;
        }

        /// <summary>
        /// 错误文件路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CreateError()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "erroe");
            return path;
        }
    }
}
