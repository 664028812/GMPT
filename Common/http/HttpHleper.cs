using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.http
{
    public class HttpHleper
    {
        public static Regex re = new Regex("^(.+?)=(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="obj_model">json 数据</param>
        /// <param name="dic">请求头</param>
        /// <param name="cookie">cookie</param>
        /// <returns></returns>
        public static string PostMoths(string url, object obj_model, Dictionary<string, string> dic = null, CookieContainer cookie = null)
        {
            string param = JsonConvert.SerializeObject(obj_model);
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            if (dic != null && dic.Count != 0)
            {
                foreach (var item in dic)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            //解析cookie
            if (cookie != null)
            {
                // cookie = new CookieContainer();
                request.CookieContainer = cookie;
            }

            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(param);
            request.ContentLength = payload.Length;
            string strValue = "";
            try
            {
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s;
                s = response.GetResponseStream();
                string StrDate = "";
                StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate;
                }
            }
            catch (Exception e)
            {
                strValue = e.Message;
            }
            return strValue;
        }


        /// <summary>
        /// post 请求获取数据 不带参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="cookie">cookie</param>
        /// <returns></returns>
        public static string PostMoths(string url, CookieContainer cookie = null)
        {
            string result = "";

            System.Net.HttpWebRequest request;

            request = (System.Net.HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";

            request.ContentType = "application/json;charset=UTF-8";
            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }

            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }



        public static string PostMoths(string url, object obj_model, string filePath, bool isCheck, CookieContainer cookie = null)
        {
            string param = JsonConvert.SerializeObject(obj_model);
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            Stream postStream = request.GetRequestStream();
            //开始标志
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(param);
            //request.ContentLength = payload.Length;
            string strValue = "";
            try
            {
                postStream.Write(payload, 0, payload.Length);
                postStream.Write(endBoundaryBytes, 0, itemBoundaryBytes.Length);
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fs.Length];
                fs.Read(bArr, 0, bArr.Length);
                fs.Close();
                postStream.Write(bArr, 0, bArr.Length);
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length); //结束标志
                postStream.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
          
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            string content = sr.ReadToEnd();
            return content;
        }

        /*
        *  url:POST请求地址
        *  postData:json格式的请求报文,例如：{"key1":"value1","key2":"value2"}
        */
        public static string PostUrl(string url, string postData, CookieContainer cookie = null)
        {
            string result = "";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "POST";

            req.ContentType = "application/json";

            if (cookie != null)
                req.CookieContainer = cookie;

            byte[] data = Encoding.UTF8.GetBytes(postData);

            req.ContentLength = data.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);

                reqStream.Close();
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream stream = resp.GetResponseStream();

            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        //----------------------------华丽分割线处理上传文件post-----------------------------------------

        public static string PostExploit(string url, string field, string file, CookieContainer cookie = null)
        {
            string retString = "";
            string boundary = $"----W{DateTime.Now.Ticks}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = $"multipart/form-data; boundary={boundary}";

            //if (!string.IsNullOrEmpty(cookie))
            //    request.Headers.Add("Cookie", cookie);
            if (cookie != null)
                request.CookieContainer = cookie;
            byte[] endBo = Encoding.UTF8.GetBytes($"--{boundary}--");

            using (MemoryStream ms = new MemoryStream())
            {
                if (!string.IsNullOrEmpty(field))
                    WriteTextField(field, boundary, ms);
                if (!string.IsNullOrEmpty(file))
                    WriteFileField(file, boundary, ms);
                if (ms.Length > 0)
                {
                    ms.Write(endBo, 0, endBo.Length);
                    request.ContentLength = ms.Length;
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(request.GetRequestStream());
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }
            return retString;
        }

        private static string WriteTextField(string textField, string boundary, MemoryStream ms)
        {
            string[] strArr = textField.Split('&');
            StringBuilder sb = new StringBuilder();
            string ss = "";
            foreach (string var in strArr)
            {
                Match m = re.Match(var);
                sb.Append("--" + boundary + "\r\n");
                sb.Append($"Content-Disposition: form-data; name=\"{m.Groups[1].Value}\"\r\n\r\n{m.Groups[2].Value}\r\n");
                //ss += "--" + boundary + "\r\n";
                //ss += $"Content-Disposition: form-data; name=\"{m.Groups[1].Value}\"/r/n/r/n{m.Groups[2].Value}/r/n";
            }
            if (sb.Length > 0)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
                ms.Write(buffer, 0, buffer.Length);
            }
            return sb.ToString();
        }

        private static void WriteFileField(string fileField, string boundary, MemoryStream ms)
        {
            string filePath = "";
            string[] strArr = fileField.Split('&');
            StringBuilder sb = new StringBuilder();
            foreach (string var in strArr)
            {
                Match m = re.Match(var);
                filePath = m.Groups[2].Value;
                sb.Append("--" + boundary + "\r\n");
                sb.Append($"Content-Disposition: form-data; name=\"{ m.Groups[1].Value}\"; filename=\"{Path.GetFileName(m.Groups[2].Value) }\"\r\n");
                sb.Append("Content-Type: application/octet-stream\r\n\r\n");

                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
                ms.Write(buffer, 0, buffer.Length);

                //添加文件数据
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(ms);
                }
                buffer = Encoding.UTF8.GetBytes("\r\n");
                ms.Write(buffer, 0, buffer.Length);
            }
        }

    }
}
