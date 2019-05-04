using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputBase._Source
{
    public enum FileHandleStatus
    {
        Success = 1,
        Fail,      
    }

    public enum EDealMethod
    {
        Delete,
        Move
    }


    /// <summary>
    /// XML 文件得路径
    /// </summary>
    public class XmlConfigPath
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
    }


    public class TaskType
    {
        public string table { get; set; }

        public string type { get; set; }
    }

    /// <summary>
    /// 任务信息
    /// </summary>
    public class TaskInfo
    {
        public string taskid { get; set; }
        public TaskType tasktype { get; set; }
        public string dataid { get; set; }
    }

    /// <summary>
    /// 上传文件信息
    /// </summary>
    public class ZipClass
    {
        public string filename { get; set; }
        public string filepath { get; set; }
    }

    public class DealInfo
    {
       /// <summary>
       /// 数据处理完后拷贝到完成的文件夹
       /// </summary>
        public string CompleteDir { get; set; }
        /// <summary>
        /// 数据处理失败后拷贝的文件
        /// </summary>
        public string ErrorDir { get; set; }
        /// <summary>
        /// 错误文件保存多少天
        /// </summary>
        public int ErrorDataKeepDays { get; set; }
        /// <summary>
        /// 是否删除完成的文件
        /// </summary>
        public bool DeleteCompleteFile { get; set; }

    }

    /// <summary>
    /// 压缩文件信息
    /// </summary>
    public class DecompressionFolder
    {
        public string Name { get; set; }

        public string ExtWithOutDot { get; set; }
    }

    public class TaskFileiInfo
    {
        //解压后的文件夹
        public DecompressionFolder df { get; set; }
        //json 数据
        public JObject jsdata { get; set; }
        public TaskInfo taskinfo { get; set; }

        //压缩文件
        public ZipClass filepath { get; set; }
        public string TaskInfofilepath { get; set; }

        public FileHandleStatus DealResult { get; set; } = FileHandleStatus.Fail;
    }
}
