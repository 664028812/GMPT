using Common.log;
using InputBase.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputBase._Source
{
    /// <summary>
    /// 输入模块的基类
    /// </summary>
    public  class InputFile
    {
        public DirectoryInfo _workDirInfo;

        public  DirectoryInfo WorkDirInfo
        {
            get;
            private   set; 
        }

        public string CompleteDir { get; set; }
        public string ErrorDir { get; set; }

        public string BakDir { get; set; }

        protected Mutex muLock = null;

        protected string muName = null;
        //第一次遍历文件夹的任务
        protected Task TaskFileScan { get; set; }

        protected Task TaskDeal { get; set; }

        protected Task TaskDealAfter { get; set; }

        protected Task TaskFinaDataDelete { get; set; }

        protected Task TaskErrorDataDelete { get; set; }


        //等待队列
        /// <summary>
        /// 等待队列
        /// </summary>
        protected ConcurrentQueue<DealDataBase> WaitingQueue
        {
            get;
        } = new ConcurrentQueue<DealDataBase>();

        //线程同步的
        private AutoResetEvent dealingQueueWaiter = new AutoResetEvent(false);

        //处理的锁
        protected readonly object dealingQueueLocker = new object();

        //处理队列
        private Dictionary<string, IIFileBase> DealingQueue
        {
            get;
        } = new Dictionary<string, IIFileBase>();

        public InputFile(XmlConfigPath xcp) 
        {
            DirectoryInfo DirInfo = new DirectoryInfo(xcp.InputPath);
            this.WorkDirInfo = DirInfo;
            InitCompleteDir();
            InitErrorDir();
            InitBakDir();

        }

        protected void InitCompleteDir()
        {
            this.CompleteDir = Path.Combine(tool.tool.CreateTemp());
            try
            {
                if (!Directory.Exists(CompleteDir))
                    Directory.CreateDirectory(CompleteDir);
            }
            catch (Exception ex)
            {
                ILog.log.Error($"文件夹初始化失败：{ex.ToString()}");
            }
        }

        protected void InitErrorDir()
        {
            this.ErrorDir = Path.Combine(tool.tool.CreateError());
            try
            {
                if (!Directory.Exists(ErrorDir))
                    Directory.CreateDirectory(ErrorDir);
            }
            catch (Exception ex)
            {
                ILog.log.Error($"文件夹初始化失败：{ex.ToString()}");
            }
        }

        protected void InitBakDir()
        {
            this.BakDir = Path.Combine(tool.tool.CreateAsd());
            try
            {
                if (!Directory.Exists(ErrorDir))
                    Directory.CreateDirectory(ErrorDir);
            }
            catch (Exception ex)
            {
                ILog.log.Error($"文件夹初始化失败：{ex.ToString()}");
            }
        }

       
        protected  bool MoveData(DealDataBase data)
        {
            bool res = false;
            string targPath = null;
            switch (data.DealResult)
            {
                case FileHandleStatus.Success:
                    targPath = CompleteDir;
                    break;
                case FileHandleStatus.Fail:
                    targPath = ErrorDir;
                    break;
                default:
                    break;
            }
            try
            {
                if (data.MoveFile(targPath))
                {
                    res = true;
                    
                }
            }
            catch(Exception ex)
            {
                ILog.log.Error($"数据移动失败：{ex.ToString()}");
            }

            return res;
        }

        /// <summary>
        /// 这里删除的是文件夹
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected  bool DeleteData(DealDataBase data)
        {
            bool flag = false;
            return flag;
        }

    

       
        protected IEnumerable<FileInfo> GetFileList()
        {
            IEnumerable<FileInfo> enumFile = WorkDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
            foreach (var fi in enumFile)
            {
                if (fi.Extension != ".7z")
                {
                    continue;
                }
                yield return fi;
            }
        }
    }
}
