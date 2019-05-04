using Common.http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputModule
{
    public class HttpManager
    {
        #region 需要的字段
        private TempAndErrorfile te { get; set; }
        private ConfigXml XmlData { get; set; }
        //api类
        private WebAPI wa = null;
        //cook
        private CookieContainer DCookieContainer = null;
        private Cookie ck = null;
        private MessageManager mm = null;

        #endregion
        public HttpManager(TempAndErrorfile te, ConfigXml XmlData)
        {
            this.te = te;
            this.XmlData = XmlData;
            Init();
        }

        private void Init()
        {
            wa = new WebAPI
            {
                addApi = $"http://{this.XmlData.Domain}/table/new/",
                deleteApi = $"http://{ this.XmlData.Domain}/table/id/delete/",
                updateApi = $"http://{ this.XmlData.Domain}/table/id/update/",
                selectApi = $"http://{ this.XmlData.Domain }/table/list/",
            };
            if (mm == null)
                mm = new MessageManager(this.XmlData.OutFilePath);
            GetCookie();
        }

        public void GetCookie()
        {
            try
            {
                object s = new
                {
                    username = this.XmlData.Name,
                    password = this.XmlData.Password

                };
                var raspone = HttpHleper.PostMoths($"http://{this.XmlData.Domain}/login_api/", s, null, null);
                JObject json = JObject.Parse(raspone);
                string token = json["user_token"].ToString();
                DCookieContainer = new CookieContainer();
                ck = new Cookie("user_token", token);
                ck.Domain = this.XmlData.Ip;
                DCookieContainer.Add(ck);
            }
            catch (Exception ex)
            {
                ILog.log.Error($"错误信息：{ex}====获取token失败");
            }
        }


        public void Start()
        {
            RealDeal();
            DealAccessLog();
        }

        private void ReadLog()
        {
            wa.selectApi = wa.selectApi.Replace("table", "accesslog");
            string MaxID = null;
            while (true)
            {
                object o = new
                {
                    id__gt = MaxID
                };
                var res = HttpHleper.PostMoths(wa.selectApi, o, null, DCookieContainer);
                JObject js = JObject.Parse(res);
                if (js["status"].ToString() == "error" && js["msg"].ToString() == "没有查询到对应的数据")
                {                   
                    ILog.log.Debug($"数据查完了，没有多余的数据");
                    Thread.Sleep(6000);
                }
                else
                {
                    string count = js["data_count"].ToString();///总的记录条数
                    JArray jlist = JArray.Parse(js["data"].ToString());
                    MaxID = jlist[jlist.Count - 1]["id"].ToString();
                    foreach (var item in jlist)
                    {
                        mm.WriteAccessLog(item);
                    }
                }
            }
        }

        public void DealAccessLog()
        {
            Task DealAccesslog = Task.Factory.StartNew(delegate { ReadLog(); });
        }

        public void RealDeal()
        {
            System.Threading.CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task task = Task.Factory.StartNew(X => WorkThread((CancellationToken)X), tokenSource.Token, tokenSource.Token);
        }

        public void WorkThread(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    //增加漏洞和更新漏洞（更新漏洞是全部更新）
                    AddPostApi(StaticDataObject.AddData);
                    DeletePostApi(StaticDataObject.DeleteData);
                    UpdatePostApi(StaticDataObject.UpdateData);

                }
                catch (Exception ex)
                {
                    ILog.log.Error($"错误信息：{ex}====工作线程中处理出现了问题");
                }
            }
        }

        #region   处理API 返回信息的函数
        public bool ResultIsSuccess(string result)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
            bool cheeck = false;
            if (!jo.ContainsKey("msg")) //返回成功
            {
                if (jo["status"].ToString() == "success")
                {
                    cheeck = true;
                }
            }
            else if (jo["msg"].ToString() == "用户信息已过期")//返回用户信息过期
            {
                GetCookie();
                cheeck = false;
            }
            else   //返回失败其他的错误信息
            {

                cheeck = false;
            }
            return cheeck;
        }

        /// <summary>
        /// 读取api返回的信息 处理后进行第二次的上传文件 
        /// 此函数用于增加漏洞
        /// </summary>
        public string ReadResultDeal(string result, string filePath, TaskFileiInfo taskid)
        {
            var res = "";

            try
            {
                JObject job = (JObject)JsonConvert.DeserializeObject(result);
                //重要的得到id 和文件名
                string status = job["status"].ToString();
                if (status == "success")
                {
                    ILog.log.Debug($"上传第一次数据返回的信息：{job}");
                    string id = job["data"]["id"].ToString();
                    string filename = job["data"]["evilfile"].ToString().Trim('/');//evilfile   "file_a22e624b-6b38-4b8a-8c66-cba4d95c747a.7z"
                    filename = filename.Replace("_", "__");
                    string data = string.Format("id={0}&evilfile={1}", id, filename);
                    string aa = filename.Split('_')[2].ToString() + "=";
                    filePath = string.Format("{0}" + filePath, aa);
                    string updateApi = wa.updateApi;
                    string url = updateApi.Replace("table", "exploit");
                    string newUrl = url.Replace("id", id);
                    res = HttpHleper.PostExploit(newUrl, data, filePath, DCookieContainer);

                }
                else
                { 
                    ILog.log.Debug($"上传文件之前的发送json给api返回的是失败的状态：{result}");
                    taskid.DealResult = ApiStatus.fail;
                }
            }
            catch (Exception ex)
            {
                ILog.log.Error($"错误信息：{ex}====ReadResultDeal中处理出现了问题");
                taskid.DealResult = ApiStatus.fail;
            }
            return res;
        }

     
        public TaskFileiInfo IsSuccess(string result, TaskFileiInfo key, TaskFileiInfo tfi)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
            if (!jo.ContainsKey("msg")) //返回成功
            {
                if (jo["status"].ToString() == "success")
                {
                    key = tfi;
                }
            }
            else if (jo["msg"].ToString() == "用户信息已过期")//返回用户信息过期
            {
                GetCookie();//用户信息过期不用删除
            }
            else
            {
                key = tfi;
            }
            mm.WriteFile(result, tfi);
            return key;
        }

        private void DeleteFile(TaskFileiInfo key, Dictionary<TaskFileiInfo, JObject> JsonData)
        {
            if(XmlData.IsDelete == true)//要删除文件
            {
                switch (key.DealResult)
                {
                    case ApiStatus.success:
                        Directory.Delete(key.TaskInfofilepath, true);
                        JsonData.Remove(key);
                        break;
                    case ApiStatus.fail:
                        Directory.Move(key.TaskInfofilepath, this.te.errorfile);
                        JsonData.Remove(key);
                        break;
                    case ApiStatus.unknown:

                        break;
                    default:
                        break;
                }
            }
            else//不删除完成的文件
            {
                switch (key.DealResult)
                {
                    case ApiStatus.success:
                        //Directory.Move(key.TaskInfofilepath, this.te.tempfile);
                        JsonData.Remove(key);
                        break;
                    case ApiStatus.fail:
                        Directory.Move(key.TaskInfofilepath, this.te.errorfile);
                        JsonData.Remove(key);
                        break;
                    case ApiStatus.unknown:

                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
//----------------------------------------------------------------------------
        #region  api请求相关的函数
        public void AddPostApi(Dictionary<TaskFileiInfo, JObject> JsonData)
        {
            if (JsonData.Count != 0)
            {
                string addApi = null;
                TaskFileiInfo key = null;
                string result = null;
                foreach (var item in JsonData)
                {
                    addApi = wa.addApi;
                    switch (item.Key.taskinfo.tasktype.table)
                    {
                        case "1":
                            addApi = addApi.Replace("table", "server");
                            result = HttpHleper.PostMoths(addApi, item.Value, null, DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{addApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{addApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                              
                            break;
                        case "2":
                            addApi = addApi.Replace("table", "exploit");
                            ILog.log.Trace($"===urls地址======================={addApi}=============================");
                            result = HttpHleper.PostUrl(addApi, item.Value.ToString(), DCookieContainer);
                            
                            if (ResultIsSuccess(result) == true)
                            {
                                
                                if (item.Key.filepath.filepath != null)
                                {                                    
                                    result = ReadResultDeal(result, item.Key.filepath.filepath, item.Key);
                                    if(ResultIsSuccess(result) == true)
                                    {
                                        ILog.log.Debug($"{addApi}请求成功返回的信息{result}");
                                        item.Key.DealResult = ApiStatus.success;
                                    }
                                    else
                                    {
                                        ILog.log.Debug($"{addApi}请求失败返回的信息{result}");
                                        item.Key.DealResult = ApiStatus.fail;
                                    }
                                }
                            }
                            else
                            {
                                ILog.log.Debug($"{addApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            
                            break;
                        case "3":
                            addApi = addApi.Replace("table", "worktime");
                            result = HttpHleper.PostUrl(addApi, item.Value.ToString(), DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{addApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{addApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        default:
                            break;
                    }

                    if (result != null)
                    {
                        //这里返回key 的信息无论成功是否都删除
                        key = IsSuccess(result, key, item.Key);
                    }
                }
                if (null != key)
                {
                    DeleteFile(key, JsonData);
                }
            }
        }


        public void DeletePostApi(Dictionary<TaskFileiInfo, JObject> JsonData)
        {
            if (JsonData.Count != 0)
            {
                string deleteApi = null;
                TaskFileiInfo key = null;
                string result = null;
                string id = null;
                foreach (var item in JsonData)
                {
                    deleteApi = wa.deleteApi;
                    id = item.Value["id"].ToString();
                    switch (item.Key.taskinfo.tasktype.table)
                    {
                        case "1":
                            deleteApi = deleteApi.Replace("table", "server");
                            deleteApi = deleteApi.Replace("id", id);
                            result = HttpHleper.PostMoths(deleteApi, DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{deleteApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{deleteApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        case "2":
                            deleteApi = deleteApi.Replace("table", "exploit");
                            deleteApi = deleteApi.Replace("id", id);
                            result = HttpHleper.PostMoths(deleteApi, DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{deleteApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{deleteApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        case "3":
                            deleteApi = deleteApi.Replace("table", "worktime");
                            deleteApi = deleteApi.Replace("id", id);
                            result = HttpHleper.PostMoths(deleteApi, DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{deleteApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{deleteApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        default:
                            break;
                    }
                    if (result != null)
                    {                       
                        key = IsSuccess(result, key, item.Key);
                    }
                }

                if (key != null)
                {
                    DeleteFile(key, JsonData);
                }
            }
        }


        public void UpdatePostApi(Dictionary<TaskFileiInfo, JObject> JsonData)
        {
            if (JsonData.Count != 0)
            {
                TaskFileiInfo key = null;
                string updateApi = null;
                string id = null;
                string result = null;
                foreach (var item in JsonData)
                {               
                    id = item.Value["id"].ToString();
                    updateApi = wa.updateApi;

                    switch (item.Key.taskinfo.tasktype.table)
                    {
                        case "1":
                            updateApi = updateApi.Replace("table", "server");
                            updateApi = updateApi.Replace("id", id);
                            result = HttpHleper.PostUrl(updateApi, item.Value.ToString(), DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{updateApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{updateApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        case "2":
                            updateApi = updateApi.Replace("table", "exploit");
                            updateApi = updateApi.Replace("id", id);
                            result = HttpHleper.PostUrl(updateApi, item.Value.ToString(), DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                if (item.Key.filepath.filepath != null)
                                {
                                    result = ReadResultDeal(result, item.Key.filepath.filepath, item.Key);
                                    if (ResultIsSuccess(result) == true)
                                    {
                                        ILog.log.Debug($"{updateApi}请求成功返回的信息{result}");
                                        item.Key.DealResult = ApiStatus.success;
                                    }
                                    else
                                    {
                                        ILog.log.Debug($"{updateApi}请求失败返回的信息{result}");
                                        item.Key.DealResult = ApiStatus.fail;
                                    }
                                }
                            }
                            else
                            {
                                ILog.log.Debug($"{updateApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        case "3":
                            updateApi = updateApi.Replace("table", "worktime");
                            updateApi = updateApi.Replace("id", id);
                            result = HttpHleper.PostUrl(updateApi, item.Value.ToString(), DCookieContainer);
                            if (ResultIsSuccess(result) == true)
                            {
                                ILog.log.Debug($"{updateApi}请求成功返回的信息{result}");
                                item.Key.DealResult = ApiStatus.success;
                            }
                            else
                            {
                                ILog.log.Debug($"{updateApi}请求失败返回的信息{result}");
                                item.Key.DealResult = ApiStatus.fail;
                            }
                            break;
                        default:
                            break;
                    }


                    if (result != null)
                    {
                        key = IsSuccess(result, key, item.Key);
                    }
                }
                if (key != null)
                {
                    DeleteFile(key, JsonData);
                }

            }
        }

        #endregion
    }
}
