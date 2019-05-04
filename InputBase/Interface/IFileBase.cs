using InputBase._Source;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputBase.Interface
{

    /// <summary>
    /// 传输的7z 压缩包直接传到数据处理器，处理器接收后直接解压加载数据
    /// 加载数据后的结构数据直接委托到api请求
    /// </summary>
    public interface IIFileBase:IDisposable
    {
        #region  属性

        //文件处理结果
        FileHandleStatus DealResult { get; set; }

        //设置这个属性时 文件已经完成
        Action<IIFileBase> OnDataIn { get; set; }

        /// <summary>
        /// 数据名 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 不带后缀的文件名
        /// </summary>
        string ExtWithoutDot { get; }

        /// <summary>
        /// 数据全名
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// 是否解压
        /// </summary>
        bool IsConpress { get; }

        /// <summary>
        /// 是否文件数据被加载
        /// </summary>
        bool IsDataLoad { get; }

        /// <summary>
        /// 文件数据
        /// </summary>
        DataStream bstm { get; }

        /// <summary>
        /// 解压后文件保存的路径
        /// </summary>
        string ComPressPath { get; }

        #endregion


        #region 方法

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool MoveFile(string path);


        /// <summary>
        /// 加载数据 输出数据流
        /// </summary>
        /// <param name="bstm"></param>
        /// <returns></returns>
        bool LoadInputData(out DataStream bstm);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <returns></returns>
        bool DeleteFile();


        #endregion 
    }
}
