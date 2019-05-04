using Common.log;
using InputBase._Source;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputBase.Interface
{
    /// <summary>
    /// 基础数据流
    /// </summary>
    public class DataStream :IDisposable
    {
        public DataStream(string filePath)
        {
            Init(filePath);
        }

        public void Init(string filePath)
        {
            ReadData(filePath);
        }
        /// <summary>
        /// 释放标记
        /// </summary>
        private bool disposed;

        /// <summary>
        /// 压缩包里面的json 数据
        /// </summary>
        public JObject Jsdata { get; set; }

        /// <summary>
        /// 压缩包里面任务文件信息
        /// </summary>
        public TaskInfo Taskinfo { get; set; }


        /// <summary>
        /// 要上传的压缩文件
        /// </summary>
        public ZipClass ZipFile { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string TaskInfoFilePath { get; set; }


        public void ReadData(string filePath)
        {
            DirectoryInfo root = new DirectoryInfo(filePath);
            
            foreach (FileInfo f in root.GetFiles())
            {
                if (Path.GetExtension(f.FullName) == ".json")
                {
                   Jsdata = ReadJson(f.FullName);
                }
                else if (Path.GetExtension(f.FullName) == ".task")
                {
                    Taskinfo = ReadTask(f.FullName);
                }
                else if (Path.GetExtension(f.FullName) != ".json" && Path.GetExtension(f.FullName) != ".task")
                {
                    ZipClass zf = new ZipClass();
                    zf.filename = f.Name;
                    zf.filepath = f.FullName;
                    ZipFile = zf;
                }

            }
        }


        /// <summary>
        /// 读取json文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private JObject ReadJson(string path)
        {
            JObject o = null;
            using (System.IO.StreamReader file = System.IO.File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    o = (JObject)JToken.ReadFrom(reader);
                }
            }
            return o;
        }

        /// <summary>
        /// 读取任务文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public TaskInfo ReadTask(string path)
        {
            TaskInfo ti = new TaskInfo();
            TaskType tt = new TaskType();
            try
            {

                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] value = line.Split(':');
                        if ("TaskId" == value[0])
                            ti.taskid = value[1];
                        else if ("Data" == value[0])
                            ti.dataid = value[1];
                        else if ("TaskType" == value[0])
                        {
                            string[] va = value[1].Split('.');
                            tt.table = va[0];
                            tt.type = va[1];
                            ti.tasktype = tt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ILog.log.Error($"读取任务文件：{ex.ToString()}");
            }
            return ti;
        }
        /// <summary>
        /// 清理接口
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


    }
}
