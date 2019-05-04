using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputBase._Source.tool
{
    public class tool
    {
        ///
        /// <summary>
        /// 压缩文件备份路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CreateCompressPath()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"file" ,"fileback");
            return path;
        }

        /// <summary>
        /// 错误文件路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CreateError()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file","erroe");
            return path;
        }

        /// <summary>
        /// 完成文件路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CreateTemp()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "temp");
            return path;
        }

        /// <summary>
        /// 待用
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string Createdir()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "dir");
            return path;
        }

        public static string ss()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "dir");
            return path;
        }
    }
}
