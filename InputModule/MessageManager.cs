using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputModule
{
    public class MessageManager : MesageInfo
    {
        private string OutFilePath { get; set; }
        public MessageManager(string path)
        {
            this.OutFilePath = path;
        }



        public void WriteFile(string result, TaskFileiInfo tfi)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(result);

            if (!System.IO.Directory.Exists(this.OutFilePath))
                System.IO.Directory.CreateDirectory(this.OutFilePath);

            if (tfi.taskinfo.tasktype.type == "2")
            {
                var DataFile = Path.Combine(this.OutFilePath, Guid.NewGuid().ToString() + ".gmpt_deletetask");
                using (StreamWriter sw = new StreamWriter(DataFile))
                {
                    string errorinfo;
                    string status;                    
                    string datatime = "captime:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
                    

                    if (jo.ContainsKey("status"))
                    {
                        if (jo["status"].ToString() == "success")
                        {
                            status = "status:" + "3";
                            errorinfo = "errorinfo:" + "NULL";
                        }
                        else
                        {
                            status = "status:" + "2";
                            errorinfo = "errorinfo:" + "返回信息失败";
                        }
                        sw.WriteLine(status);
                        sw.WriteLine(errorinfo);
                    }

                    string  taskid = "taskid:" + tfi.taskinfo.taskid;
                    sw.WriteLine(datatime);
                    sw.WriteLine(taskid);
                }
            }
            else
            {
                var DataFile = Path.Combine(this.OutFilePath, Guid.NewGuid().ToString() + ".gmpt_task");
                using (StreamWriter sw = new StreamWriter(DataFile))
                {
                    string errorinfo;
                    string status;
                    if (jo.ContainsKey("status"))
                    {
                        if (jo["status"].ToString() == "success")
                        {
                            status = "status:" + "3";
                            errorinfo = "errorinfo:" + "NULL";
                        }
                        else
                        {
                            status = "status:" + "2";
                            errorinfo = "errorinfo:" + "返回信息失败";
                        }
                        sw.WriteLine(status);
                        sw.WriteLine(errorinfo);
                    }
                    else if (jo.ContainsKey("msg"))
                    {
                        status = "status:" + "2";
                        errorinfo = "errorinfo:" + "用户信息已过期";
                        sw.WriteLine(status);
                        sw.WriteLine(errorinfo);
                    }
                    if (jo.ContainsKey("id"))
                    {
                        string gmptid = null;

                        gmptid = "gmptid:" + jo["id"].ToString();

                        sw.WriteLine(gmptid);
                    }
                    //时间
                    string datatime = "captime:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
                    sw.WriteLine(datatime);
                    //taskid
                    string taskid = "taskid:" + tfi.taskinfo.taskid;
                    sw.WriteLine(taskid);

                    string data = "data:" + tfi.taskinfo.dataid;
                    sw.WriteLine(data);
                    string type = null;
                    switch (tfi.taskinfo.tasktype.table)
                    {
                        case "1":
                            type = "type:" + "gmpt_server";
                            break;
                        case "2":
                            type = "type:" + "gmpt_exploit";
                            break;
                        case "3":
                            type = "type:" + "gmpt_worktime";
                            break;
                    }
                    sw.WriteLine(type);
                    sw.Close();
                }
            }


        }


        public void WriteAccessLog(JToken result)
        {
            string count = "count:" + result["count"].ToString();
            string browserlang = "browserlang:" + result["browserlang"].ToString();
            string browserver = "browserver:" + result["browserver"].ToString();
            string flashtype = "flashtype:" + result["flashtype"].ToString();
            string ip = "ip:" + result["ip"].ToString();
            string fingerprint = "fingerprint:" + result["fingerprint"].ToString();
            string accesstime = "accesstime:" + result["accesstime"].ToString();
            string browserplatform = "browserplatform:" + result["browserplatform"].ToString();
            string osplat = "osplat:" + result["osplat"].ToString();
            string flashversion = "flashversion:" + result["flashversion"].ToString();
            string sid = "sid:" + result["sid"].ToString();
            string useragent = "useragent:" + result["useragent"].ToString();
            string clientip = "clientip:" + result["clientip"].ToString();
            string os = "os:" + result["os"].ToString();
            //string id = "id:" + result["id"].ToString();
            string browser = "browser:" + result["browser"].ToString();
            if (!System.IO.Directory.Exists(this.OutFilePath))
                System.IO.Directory.CreateDirectory(this.OutFilePath);
            var DataFile = Path.Combine(this.OutFilePath, Guid.NewGuid().ToString() + ".gmpt_accesslog");
            using (StreamWriter sw = new StreamWriter(DataFile))
            {
                sw.WriteLine(count);
                sw.WriteLine(browserlang);
                sw.WriteLine(browserver);
                sw.WriteLine(flashtype);
                sw.WriteLine(ip);
                sw.WriteLine(fingerprint);
                sw.WriteLine(accesstime);
                sw.WriteLine(browserplatform);
                sw.WriteLine(osplat);
                sw.WriteLine(flashversion);
                sw.WriteLine(sid);
                sw.WriteLine(useragent);
                sw.WriteLine(clientip);
                sw.WriteLine(os);
                sw.WriteLine(browser);
                sw.Close();
            }
            ILog.log.Error($"文件写入：{DataFile}");
        }
    }
}
