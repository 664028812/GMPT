using Common._7z;
using Common.log;
using InputBase.Interface;
using Newtonsoft.Json.Linq;
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
    /// 这里正式处理文件信息
    /// </summary>
    public class FileBase
    {
        #region 属性
        /// <summary>
        /// 文件输入输出的路径
        /// </summary>
        protected XmlConfigPath configData { get; private set; }

        protected Mutex muLock = null;
        protected string muName = null;
        //开始处理时间
        public DateTime startTime = DateTime.Now;
        //结束处理时间
        public DateTime emdTime = DateTime.Now;

        //第一次遍历文件夹的任务
        protected Task TaskFileScan { get; set; }

        protected Task TaskDeal { get; set; }

        protected Task TaskDealAfter { get; set; }

        protected Task TaskFinaDataDelete { get; set; }

        protected Task TaskErrorDataDelete { get; set; }

        //解压相关
        private CompressionAndDecompression cd { get; set; }
        /// <summary>
        /// 平台
        /// </summary>
        private string _InputFrom;

        protected string InputFrom
        {
            get
            {
                return _InputFrom;
            }
            set
            {
                _InputFrom = value;
            }
        }

        /// <summary>
        /// 文件的处理信息   
        /// </summary>
        protected DealInfo di { get; private set; }

        protected FileSystemWatcher Fsea { get; set; }

        /// <summary>
        /// 工作文件夹信息
        /// </summary>
        protected DirectoryInfo WorkDirInfo
        {
            get; set;
        }

        #endregion

        #region    队列线程相关
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


        #endregion
        public FileBase(XmlConfigPath xcp, DealInfo di)
        {
            this.di = di;
            this.configData = xcp;
            InitCD();
            InitWorkDirInfo();
            InitEndFileDirInfo();
            InitErrorFileDirInfo();
            InitFileSystemWatcher();
        }

        #region  初始化函数

        public void InitWorkDirInfo()
        {
            if (WorkDirInfo == null)
                WorkDirInfo = new DirectoryInfo(this.configData.InputPath);
        }

        /// <summary>
        /// 初始化完成的文件夹
        /// </summary>
        public void InitEndFileDirInfo()
        {
            string endFileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, di.CompleteDir);
            if (!Directory.Exists(endFileDir))
                Directory.CreateDirectory(endFileDir);
        }

        /// <summary>
        /// 初始化解压相关
        /// </summary>
        public void InitCD()
        {
            //string dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll", "7z.dll");
            cd = CompressionAndDecompression.GetInstance();
        }

        /// <summary>
        /// 初始化存放错误文件的文件夹
        /// </summary>
        public void InitErrorFileDirInfo()
        {
            string errorFileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, di.CompleteDir);
            if (!Directory.Exists(errorFileDir))
                Directory.CreateDirectory(errorFileDir);
        }

        protected void InitFileSystemWatcher()
        {
            Fsea = new FileSystemWatcher(WorkDirInfo.FullName);
            Fsea.IncludeSubdirectories = false;

        }

        #endregion

        #region 开始函数  所有文件得入口

        /// <summary>
        /// 启动函数
        /// </summary>
        /// <param name="onDataIn"></param>
        public void Start(Action<IIFileBase> onDataIn)
        {
            try
            {
                if (Mutex.TryOpenExisting(InputFrom, out muLock))
                {
                    ILog.log.Error($"已经有个项目在监控此目录：{InputFrom}");
                    return;
                }
                //muName = InputFrom.Replace("\\", "");
            }
            catch (Exception ex)
            {
                ILog.log.Error($"创建线程锁失败：{ex}");
            }

            StartOtherInputThtread();
            StartInputBusiness();
        }



        protected void StartInputBusiness()
        {
            if (TaskFileScan == null)
                TaskFileScan = Task.Factory.StartNew(delegate { FileScan(); });

            if (TaskDeal == null)
                TaskDeal = Task.Factory.StartNew(delegate { DealApi(); });


        }
        #endregion

        #region  这里是获取数据的地方

        protected IEnumerable<FileInfo> GetFileLists()
        {
            IEnumerable<FileInfo> enumFile = WorkDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
            foreach (var item in enumFile)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 解压相关
        /// </summary>
        /// <param name="file7z"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public string Decompression(string file7z, string savePath)
        {
            bool flag = false;
            string destDir = null;
            try
            {
                //先是获取文件信息  然后解压   解压的文件时夹是GMPT  获得文件夹后重命名之后 删除
                FileInfo fi2 = new FileInfo(file7z);
                cd.Decompression(file7z, savePath);
                flag = true;
                string sourceDir = Path.Combine(savePath, "GMPT");
                destDir = Path.Combine(savePath, Path.GetFileNameWithoutExtension(fi2.FullName));
                //解压后move一次  不然文件名会不一致
                Directory.Move(sourceDir, destDir);
                ILog.log.Debug($"文件解压：{fi2.Name}文件解压成功为{destDir}");
                fi2.Delete();

            }
            catch (Exception ex)
            {
                ILog.log.Error($"解压文件失败{file7z};错误信息：{ex}");
                flag = false;
            }
            finally
            {
                Thread.Sleep(1000);
            }
            return destDir;
        }

        #endregion


        protected IEnumerable<KeyValuePair<Object, EventArgs>> GetDatas()
        {
            foreach (FileInfo fi in GetFileLists())
            {
                KeyValuePair<object, EventArgs> dataEv = GetDataReceiveHandler(fi);
                if (dataEv.Key == null || dataEv.Value == null)
                {
                    ILog.log.Error($"获取文件里的数据失败：{fi.FullName}");
                    continue;
                }
                yield return dataEv;
            }
        }

        protected KeyValuePair<object, EventArgs> GetDataReceiveHandler(FileInfo fi)
        {
            return new KeyValuePair<object, EventArgs>(Fsea, new FileSystemEventArgs(WatcherChangeTypes.Created, fi.DirectoryName, fi.Name));
        }

        /// <summary>
        /// 这里是读取文件得线程
        /// </summary>
        protected void FileScan()
        {
            do
            {
                try
                {
                    foreach (KeyValuePair<object, EventArgs> eventHandler in GetDatas())
                    {
                        OnDataGreated(eventHandler.Key, eventHandler.Value);

                        while (WaitingQueue.Count > 100)
                        {
                            Thread.Sleep(1000);
                        }
                    }


                }
                catch (Exception ex)
                {
                    ILog.log.Error($"扫描文件出错：{ex.ToString()}");
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            } while (true);
        }

        /// <summary>
        /// 这里是api 处理得线程
        /// </summary>
        protected void DealApi()
        {
            int total = 0;
            while (true)
            {
                while (!WaitingQueue.Any())
                {
                    Thread.Sleep(1000);

                    DealDataBase data = DeQueueWaiting();

                    try
                    {
                        if(data == null)
                        {
                            ILog.log.Error($"获取数据失败");
                            continue;
                        }

                        if (IsOnDealing(data))
                            continue;

                        if (!EnQueueDealing(data))
                            continue;

                        //ILog.log.Debug($"来了一个数据：任务id--{data.Taskinfo.taskid}任务数据{data.Jsdata}准备进行请求api");

                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
        }
        protected void OnDataGreated(object sender, EventArgs e)
        {
            try
            {
                DealDataBase data = LoadData(sender, e);
                if (data == null || IsInWaitingList(data))
                {
                    return;
                }

                lock (dealingQueueLocker)
                {
                    if (IsOnDealing(data))
                    {
                        return;
                    }
                }
                //加入等待队列
                EnQueueWaiting(data);
                //if(!data.IS)
            }
            catch (Exception ex)
            {
                ILog.log.Error($"扫描文件出错：{ex.ToString()}");
            }
        }



        protected DealDataBase LoadData(Object sender, EventArgs arg)
        {
            DealDataBase res = null;
            FileSystemEventArgs e = arg as FileSystemEventArgs;
            if (e == null)
                return res;

            if (e.ChangeType != WatcherChangeTypes.Created)
                return res;

            try
            {
                if (!IsFile(e.FullPath))
                    return res;
                string destDir = Decompression(e.FullPath, this.configData.InputPath);
                DirectoryInfo root = new DirectoryInfo(destDir);
                //FileInfo taskfi = new FileInfo(e.FullPath);
                if (IsInWaitingList(root))
                    return res;
                //这里开始解压 返回文件夹的名

                res = new DealingFileBase();

            }
            catch (Exception ex)
            {
                ILog.log.Error($"{ex.ToString()}");
                res = null;
            }
            return res;
        }

        protected virtual void StartOtherInputThtread() { }







        #region 队列处理相关



        /// <summary>
        /// 确定等待队列是否有这个元素
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected bool IsInWaitingList(DirectoryInfo data)
        {
            return WaitingQueue.Any((x) => { return x.TaskInfoFilePath == data.FullName; });
        }


        protected bool IsFile(string path)
        {
            bool res = false;
            try
            {
                FileInfo fi = new FileInfo(path);
                fi.Refresh();
                res = fi.Exists;
            }
            catch (Exception ex)
            {
                res = false;
            }
            return res;
        }

        /// <summary>
        /// 是不是在处理队列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool IsOnDealing(DealDataBase data)
        {
            lock (dealingQueueLocker)
            {
                return DealingQueue.ContainsKey(data.TaskInfoFilePath);
            }
        }

        protected bool IsInWaitingList(DealDataBase data)
        {
            return WaitingQueue.Any((x) => { return x.TaskInfoFilePath == data.TaskInfoFilePath; });
        }

        protected void EnQueueWaiting(DealDataBase data)
        {
            if (IsInWaitingList(data))
            {
                return;
            }
            WaitingQueue.Enqueue(data);
        }

        protected bool EnQueueDealing(DealDataBase data)
        {
            lock (dealingQueueLocker)
            {
                if (DealingQueue.ContainsKey(data.TaskInfoFilePath))
                    return false;
                DealingQueue.Add(data.TaskInfoFilePath, data);
                return true;
            }
        }

        //是否循环检查
        protected bool IsRedoFirstDeal
        {
            get;
            set;
        }


        #endregion

        #region 后面再说这些东西
        //protected void OnDataGreated(object sender,EventArgs e)
        //{
        //    try
        //    {
        //        DealDataBase data = LoadData(sender, e);
        //        if(data == null || IsInWaitingList(data))
        //        {
        //            return;
        //        }

        //        lock(dealingQueueLocker)
        //        {
        //            if(IsOnDealing(data))
        //            {
        //                return;
        //            }
        //        }
        //        EnQueueWaiting(data);
        //        //if(!data.IS)
        //    }
        //    catch(Exception ex)
        //    {
        //        ILog.log.Error($"扫描文件出错：{ex.ToString()}");
        //    }
        //}

        //protected void FileScan()
        //{
        //    do
        //    {
        //        try
        //        {
        //            foreach(KeyValuePair<object,EventArgs> eventHandler in GetDatas())
        //            {
        //                OnDataGreated(eventHandler.Key, eventHandler.Value);

        //                while (WaitingQueue.Count > 100)
        //                {
        //                    Thread.Sleep(1000);
        //                }
        //            }


        //        }
        //        catch(Exception ex)
        //        {
        //            ILog.log.Error($"扫描文件出错：{ex.ToString()}");
        //        }
        //        finally
        //        {
        //            Thread.Sleep(1000);
        //        }
        //    } while (true);
        //}
        #endregion


        /// <summary>
        /// 这里在等待队列里面拿出数据
        /// </summary>
        /// <returns></returns>
        protected DealDataBase DeQueueWaiting()
        {
            if (!WaitingQueue.Any())
            {
                return null;
            }
            DealDataBase res = null;
            int total = 0;
            while (!WaitingQueue.TryDequeue(out res))
            {
                if(total % (30*100)==0)
                {
                    ILog.log.Error($"从等待队列拿出数据失败");
                }
                Thread.Sleep(1000);
                total += 100;
            }
            return res;
        }

        #region 暂时没用的东西

        //private void DealInput()
        //{
        //    int total = 0;
        //    while(true)
        //    {
        //        try
        //        {
        //            while(!WaitingQueue.Any())
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            //从等待队列取出数据
        //            DealDataBase data = DeQueueWaiting();

        //            if(data == null)
        //            {
        //                continue;
        //            }

        //            if(IsOnDealing(data))
        //            {
        //                continue;
        //            }

        //            total = 0;
        //            int dqCnt = 0;
        //            while(true)
        //            {
        //                lock(dealingQueueLocker)
        //                {
        //                    dqCnt = DealingQueue.Count;
        //                }

        //                if (dqCnt > 100)
        //                {
        //                    if (dealingQueueWaiter.WaitOne(60 * 1000))
        //                        continue;
        //                }
        //                else
        //                    break;

        //            }

        //            if(!EnQueueDealing(data))
        //            {
        //                continue;
        //            }

        //            data.OnComplete = (dt) =>
        //            {
        //                try
        //                {
        //                    if (dt.DealResult != FileHandleStatus.Success)
        //                    {
        //                        EndupDeal(data, EDealMethod.Delete);
        //                        ILog.log.Debug($"处理文件失败");
        //                    }
        //                    else
        //                    {
        //                        EndupDeal(data, EDealMethod.Move);
        //                        ILog.log.Debug($"处理文件好了");
        //                    }

        //                }
        //                catch
        //                {
        //                    ILog.log.Error($"文件处理失败");
        //                }
        //            };

        //        }
        //        catch(Exception ex)
        //        {
        //            ILog.log.Error($"扫描文件和加载文件出错：{ex.ToString()}");
        //        }
        //    }
        //}


        //protected void EndupDeal(DealDataBase data, EDealMethod dealthmod)
        //{
        //    bool flag = false;

        //    try
        //    {
        //        if(data == null)
        //        {
        //            flag = true;
        //            return;
        //        }

        //        data.dealthmod = dealthmod;
        //        switch (dealthmod)
        //        {
        //            case EDealMethod.Delete:
        //                flag = DeleteData(data);
        //                break;
        //            case EDealMethod.Move:
        //                flag = MoveData(data);
        //                break;
        //            default:
        //                break;                       
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        ILog.log.Error($"文件处理出错：{ex.ToString()}");
        //    }
        //}


        // protected abstract bool DeleteData(DealDataBase data);
        //   protected abstract bool MoveData(DealDataBase data);
        #endregion
    }
}
