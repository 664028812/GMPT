using InputBase.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputBase._Source
{
    /// <summary>
    /// 表示一个文件夹里面的内容
    /// </summary>
    public abstract class DealDataBase : IIFileBase
    {
        #region  属性

        /// <summary>
        /// 解压后文件保存路径
        /// </summary>
        public abstract string ComPressPath { get; set; }

        /// <summary>
        /// 数据名
        /// </summary>
        public abstract string Name { get; protected set; }

        /// <summary>
        /// 不带后缀的文件名
        /// </summary>
        public abstract string ExtWithoutDot { get; protected set; }

        /// <summary>
        /// 数据全名
        /// </summary>
        public abstract string FullName { get; protected set; }

        /// <summary>
        /// 是否解压
        /// </summary>
        public abstract bool IsConpress { get; protected set; }

        /// <summary>
        /// 是否文件数据被加载
        /// </summary>
        public abstract bool IsDataLoad { get; protected set; }

        /// <summary>
        /// 文件数据
        /// </summary>
        public abstract DataStream bstm { get; protected set; }

        /// <summary>
        /// 最后处理的方法
        /// </summary>
        public EDealMethod dealthmod { get; set; } = EDealMethod.Delete;

        #endregion

        /// <summary>
        /// 任务的路径
        /// </summary>
        public abstract string TaskInfoFilePath { get; protected set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        private FileHandleStatus dealResult;
        public FileHandleStatus DealResult
        {
            get { return dealResult; }
            set
            {
              dealResult = value  ;
                OnDataIn(this);
            }
        } 
        public Action<IIFileBase> OnDataIn { get; set; }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <returns></returns>
        public abstract bool DeleteFile();
        

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="desPath"></param>
        /// <returns></returns>
        public abstract bool MoveFile(string desPath);

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="bstm"></param>
        /// <returns></returns>
        public abstract bool LoadInputData(out DataStream bstm);

        /// <summary>
        /// 接口未实现子类实现那
        /// </summary>
        public void Dispose()
        {
            
        }

        //public abstract bool Delete();

    }
}
