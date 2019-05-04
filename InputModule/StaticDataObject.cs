using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputModule
{
    public class StaticDataObject
    {
        //新设计 增加 删除 更新 查询分开
        public static Dictionary<TaskFileiInfo, JObject> AddData = new Dictionary<TaskFileiInfo, JObject>();
        public static Dictionary<TaskFileiInfo, JObject> DeleteData = new Dictionary<TaskFileiInfo, JObject>();
        public static Dictionary<TaskFileiInfo, JObject> UpdateData = new Dictionary<TaskFileiInfo, JObject>();
        public static Dictionary<TaskFileiInfo, JObject> SelectData = new Dictionary<TaskFileiInfo, JObject>();
    }
}
