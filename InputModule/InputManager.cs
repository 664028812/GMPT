using Common._7z;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InputModule
{
    public class InputManager
    {
        private TempAndErrorfile te = new TempAndErrorfile();

        //输入输出的目录
        private ConfigXml XmlData { get; set; }
        //保存token
        private string Token { get; set; }
        //这是7z 解压的类
        private CompressionAndDecompression cd { get; set; }


        public InputManager(string tempfile, string erropath, string config)
        {
            this.te.tempfile = tempfile;
            this.te.errorfile = erropath;
            Init(config);

        }

        public void Init(string config)
        {
            ReadFile(config);
            //判断文件夹是否存在，不存在就创建
            IsFolderExist();
            string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll", "7z.dll");
            cd = CompressionAndDecompression.GetInstance(dll);

        }

        public void Start()
        {
            ILog.log.Debug("开始执行任务");
            RealDealFile();
            DealApi();
        }

        private void RealDealFile()
        {
            Task taskDeal = Task.Factory.StartNew(delegate { GetFileName(this.XmlData.InputFilePath); });
        }

        private void DealApi()
        {
            Task taskDealAPI = Task.Factory.StartNew(delegate { Work(); });
        }

        private void Work()
        {
            HttpManager hm = new HttpManager(this.te,this.XmlData);
            hm.Start();
        }

        private void GetFileName(string path)
        {
            DirectoryInfo root = new DirectoryInfo(XmlData.InputFilePath);
            IEnumerable<FileInfo> enumFile = root.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
            while (true)
            {
                try
                {
                   // DirectoryInfo root = new DirectoryInfo(XmlData.InputFilePath);
                    foreach (FileInfo f in enumFile)
                    {
                        if (f.Extension == ".7z")
                        {                           
                            //开始解压 解压后的文件夹是GMPT
                            cd.Decompression(f.FullName, this.XmlData.InputFilePath);
                            //获取GMPT文件夹的绝对路径
                            string sourceDir = Path.Combine(this.XmlData.InputFilePath, "GMPT");
                            string destDir = Path.Combine(this.XmlData.InputFilePath, Path.GetFileNameWithoutExtension(f.FullName));
                            //解压后move一次  不然文件名会不一致
                            Directory.Move(sourceDir, destDir);
                            ILog.log.Debug($"文件解压：{f.Name}文件解压成功为{destDir}");
                            string newPath = Path.Combine(this.XmlData.InputFilePath, Path.GetFileNameWithoutExtension(f.Name));
                            //因为问价解压后是 GMPT文件夹形式  所以这里添加修改文件夹
                            ReadCompression(newPath, this.te.tempfile);
                            f.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ILog.log.Error("错误信息：" + ex);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 读取xml配置文件 path是xml文件的配置路径
        /// </summary>
        /// <param name="path">xml文件的配置路径</param>
        private void ReadFile(string ConfigPath)
        {
            if (Directory.Exists(ConfigPath))
            {
                ILog.log.Error("错误信息：" + "文件不存在");
                goto __END;
            }
            XmlSerializer deserializer = new XmlSerializer(typeof(ConfigXml));
            TextReader reader = new StreamReader(ConfigPath);
            object obj = deserializer.Deserialize(reader);
            XmlData = (ConfigXml)obj;
            reader.Close();
        __END:
            return;
        }

        /// <summary>
        /// 判断输入输出文件夹是否存在 不存在则创建
        /// </summary>
        private void IsFolderExist()
        {
            if (!Directory.Exists(this.XmlData.InputFilePath))
                Directory.CreateDirectory(this.XmlData.InputFilePath);
                ILog.log.Debug($"从{this.XmlData.InputFilePath}文件中读取数据");
            if (!Directory.Exists(this.XmlData.OutFilePath))
                Directory.CreateDirectory(this.XmlData.OutFilePath);
        }

        /// <summary>
        /// 读取解压的文件夹
        /// </summary>
        private void ReadCompression(string compressionPath, string scoreDir)
        {
            DirectoryInfo root = new DirectoryInfo(compressionPath);
            TaskFileiInfo tfi = new TaskFileiInfo();
            ZipClass zc = new ZipClass();
            ILog.log.Debug("开始读取文件夹内的文件");
            foreach (FileInfo f in root.GetFiles())
            {
                if (Path.GetExtension(f.FullName) == ".json")
                {
                    tfi.jsdata = ReadJson(f.FullName);
                }
                else if (Path.GetExtension(f.FullName) == ".task")
                {
                    tfi.taskinfo = ReadTask(f.FullName);
                }
                else if (Path.GetExtension(f.FullName) != ".json" && Path.GetExtension(f.FullName) != ".task")
                {
                    zc.filename = f.Name;
                    zc.filepath = f.FullName;
                    tfi.filepath = zc;
                }

            }
            DataClassification(tfi);
            MoveFile(compressionPath, scoreDir, tfi);

        }
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
        /// 读取key value 文件返回对象
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
            catch (Exception e)
            {
                ILog.log.Error($"错误信息:读取任务文件{path}失败");
                ILog.log.Error($"错误信息：{e}");
            }
            return ti;
        }


        private void DataClassification(TaskFileiInfo tfi)
        {
            switch (tfi.taskinfo.tasktype.type)
            {
                case "1":
                    if (!StaticDataObject.AddData.ContainsKey(tfi))
                        StaticDataObject.AddData.Add(tfi, tfi.jsdata);
                    break;
                case "2":
                    if (!StaticDataObject.DeleteData.ContainsKey(tfi))
                        StaticDataObject.DeleteData.Add(tfi, tfi.jsdata);
                    break;
                case "3":
                    if (!StaticDataObject.UpdateData.ContainsKey(tfi))
                        StaticDataObject.UpdateData.Add(tfi, tfi.jsdata);
                    break;
                case "4":
                    if (!StaticDataObject.SelectData.ContainsKey(tfi))
                        StaticDataObject.SelectData.Add(tfi, tfi.jsdata);
                    break;
                default:
                    break;
            }
            ILog.log.Debug($"写入文件到集合中去{tfi.jsdata}");
        }


        public void MoveFile(string dir, string desdir, TaskFileiInfo tfi)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            desdir = Path.Combine(desdir, di.Name);
            tfi.TaskInfofilepath = desdir;
            try
            {
                if (!Directory.Exists(dir))
                {
                    ILog.log.Error($"{dir}文件路径不存在");
                    return;
                }

                di.MoveTo(desdir);

                ILog.log.Debug($"把{dir}拷贝到{desdir }");
                //压缩文件中除了 json和task 文件其他的文件保存在 zipDIc 字典中  key value  
                DirectoryInfo des = new DirectoryInfo(desdir);
                foreach (FileInfo f in des.GetFiles())
                {
                    if (Path.GetExtension(f.FullName) != ".json" && Path.GetExtension(f.FullName) != ".task")
                    {
                        tfi.filepath.filepath = f.FullName;
                        tfi.filepath.filename = f.Name;                     
                    }
                }                         
            }
            catch (Exception ex)
            {               
                ILog.log.Error($"错误信息：{ex }");
                Directory.Delete(desdir);
            }

        }
    }
}
