using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common._7z;
using Common.log;
using InputBase.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InputBase._Source
{
    /// <summary>
    /// 表示被处理一个文件
    /// </summary>
    public class DealingFileBase : DealDataBase
    {
        #region   属性

        /// <summary>
        /// 解压后文件保存路径   解压后的工作文件夹
        /// </summary>
        public override string ComPressPath { get; set; }
        public override string Name { get; protected set; }

        public override string ExtWithoutDot { get; protected set; }

        public override string FullName { get; protected set; }

        //文件全路径
        public override string TaskInfoFilePath { get; protected set; }

      //  public string InputPath { get;  set; }
      //  public string OutputPath { get; set; }
        public long fileLength { get; set; }
        /// <summary>
        /// 解压后得文件夹
        /// </summary>
        public string WorkFolder { get; set; }

        protected bool _isConpress;
        public override bool IsConpress
        {
            get;
            protected set;
        }

        protected bool _isDataLoad;
        public override bool IsDataLoad
        {
            get;
            protected set;
        }

        public override DataStream bstm { get;protected set; }
        /// <summary>
        /// 压缩文件
        /// </summary>
        private FileInfo _fileInfo;

        public FileInfo FileInfo_
        {
            get
            {
                if (_fileInfo != null)
                {
                    _fileInfo.Refresh();
                }
                return _fileInfo;
            }
            protected set
            {
                _fileInfo = value;
            }
        }

        #endregion


        #region 方法   初始化函数就传入路径
        /// <summary>
        /// 初始化函数 
        /// 传入的是文件数据
        /// </summary>
        public DealingFileBase(FileInfo inputFile)
        {           
            this._fileInfo = inputFile;
            this.FullName = inputFile.FullName;
            this.Name = inputFile.Name;
            this.fileLength = inputFile.Length;
            this.ExtWithoutDot = Path.GetFileNameWithoutExtension(inputFile.Name);
            

            //拿到文件二话不说先解压
            DecompressionFile();
        }


        /// <summary>
        /// 解压原始文件
        /// </summary>
        /// <param name="filePath">原始文件</param>
        /// <param name="savePath">保存路径</param>
        public void DecompressionFile()
        {
            bool res = true;
            try
            {
                //保存路径为： 
                this.ComPressPath = Path.Combine(tool.tool.CreateCompressPath(),Guid.NewGuid().ToString());
                if (CompressionAndDecompression.GetInstance().Decompression(this.FullName, this.ComPressPath) == false)
                {
                    res = false;
                    ILog.log.Error($"解压文件失败：{this.FullName}");
                }
                else
                {
                    res = true;
                    this.IsConpress = true;
                    WorkFolder = Path.Combine(this.ComPressPath, "GMPT"); //解压后得文件夹 ComPressPath
                    ILog.log.Error($"解压文件成功：{this.FullName}");
                }
            }
            catch(Exception ex)
            {
                res = false;
                ILog.log.Error($"文件解压异常：{ex.ToString()}");
            }
        }

        /// <summary>
        /// 加载数据 输出数据流
        /// </summary>
        /// <param name="bstm"></param>
        /// <returns></returns>
        public override bool LoadInputData(out DataStream bstm)
        {
            bool res = false;
            DataStream inputStream = null;
            try
            {
                ///这里的文件夹是解压后的文件夹
                inputStream = new DataStream(this.WorkFolder);
                if (inputStream.Taskinfo.taskid != null)
                {
                    res = true;
                    this.IsDataLoad = true;
                    ILog.log.Info($"加载文件数据成功：{this.FullName}");
                }
            }
            catch (Exception ex)
            {
                res = false;
                ILog.log.Error($"加载文件数据失败：{ex.ToString()}");
            }
            bstm = inputStream;
            return res;
        }


        
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <returns></returns>
        public override bool DeleteFile()
        {
            bool res = true;
            try
            {
                if (!FileInfo_.Exists)
                {
                    return res;
                }
                FileInfo_.Delete();
            }
            catch (Exception ex)
            {
                res = false;
                ILog.log.Error($"移动数据数据失败：{ex.ToString()}");
            }
            return res;
        }
        

        /// <summary>
        /// 解压失败移动去解压失败得文件
        /// 解压成功移动去备份文件夹
        /// </summary>
        /// <param name="desPath"></param>
        /// <returns></returns>
        public override bool MoveFile(string desPath)
        {
            bool res = true;
            try
            {
                if(!FileInfo_.Exists)
                {
                    return res;
                }
                FileInfo_.MoveTo(desPath);
            }
            catch(Exception ex)
            {
                res = false;
                ILog.log.Error($"移动数据数据失败：{ex.ToString()}");
            }
            return res;
        }

        #endregion


    }
}
