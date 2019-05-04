using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InputBase.config
{
    public  class InitConfig
    {

        public InitConfig()
        {

        }

        public void Init()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "Config.xml");

            if (!File.Exists(path))
                throw new FileNotFoundException($"文件：{path}：不存在");

            XElement xel = XElement.Load(path);
        
            foreach (XElement root in xel.Elements("root"))
            {
               
               foreach(XElement model in xel.Elements("pathfile"))
                {
                    
                }

                foreach (XElement model in xel.Elements("param"))
                {
                    string Enable = root.Attribute("enable").Value;
                }
            }
        }
    }
}
