using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputModule
{


    /// <summary>
    /// 文件执行的优先级
    /// </summary>
    enum Executivelevel
    {
        add = 0,
        delete,
        update,
        select
    };

    /// <summary>
    /// api返回的状态
    /// </summary>
    public enum ApiStatus
    {
        success = 1,
        fail,
        cancel,
        unknown,

    }

    

    /// <summary>
    /// 配置文件
    /// </summary>
    public class ConfigXml
    {
        //输入目录
        public string InputFilePath { get; set; }
        //输出目录
        public string OutFilePath { get; set; }
        //域名
        public string Domain { get; set; }
        //处理线程数量
        public int ThreadNum { get; set; }
        /// <summary>
        /// g挂马平台用户名和账号
        /// </summary>
        public string Name { get; set; }
        public string Password { get; set; }

        public string Ip { get; set; }
        public string Port { get; set; }
        public bool IsDelete { get; set; }
    }

    /// <summary>
    /// api 返回的字段
    /// </summary>
    public class MesageInfo
    {
        public string resultData { get; set; }

        public string taskId { get; set; }

        public string TaskType { get; set; }
    }

    public class TaskType
    {
        public string table { get; set; }

        public string type { get; set; }
    }


    public class TaskInfo
    {
        public string taskid { get; set; }
        public TaskType tasktype { get; set; }
        public string dataid { get; set; }
    }

    public class ZipClass
    {
        public string filename { get; set; }
        public string filepath { get; set; }
    }


    public class TaskFileiInfo
    {
        public JObject jsdata { get; set; }
        public TaskInfo taskinfo { get; set; }

        public ZipClass filepath { get; set; }
        public string TaskInfofilepath { get; set; }

        public ApiStatus DealResult { get; set; } = ApiStatus.unknown;
    }

    public class TempAndErrorfile
    {
        //完成目录
        public string tempfile { get; set; }
        //错误目录
        public string errorfile { get; set; }
    }


    public class WebAPI
    {
        public string addApi { get; set; }
        public string deleteApi { get; set; }
        public string updateApi { get; set; }
        public string selectApi { get; set; }
    }

    public class AccessLog
    {
        public string count { get; set; }
        public string browserlang { get; set; }
        public string browserver { get; set; }
        public string flashtype { get; set; }
        public string ip { get; set; }
        public string fingerprint { get; set; }
        public string accesstime { get; set; }
        public string browserplatform { get; set; }
        public string osplat { get; set; }
        public string flashversion { get; set; }
        public string sid { get; set; }
        public string useragent { get; set; }
        public string clientip { get; set; }
        public string os { get; set; }
        public string id { get; set; }
        public string browser { get; set; }
    }
}
