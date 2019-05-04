using InputModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMPT
{
    class Program
    {
        static void Main(string[] args)
        {
            //完成的文件夹
            string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            //错误的文件夹
            string errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error");
            if (!Directory.Exists(errorPath))
                Directory.CreateDirectory(errorPath);
            //配置文件夹
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);
            string configxml =Path.Combine( AppDomain.CurrentDomain.BaseDirectory,"config", "ConfigXml.xml");

            InputManager im = new InputManager(tempPath, errorPath, configxml);
            im.Start();
            Console.ReadKey();
        }
    }
}
