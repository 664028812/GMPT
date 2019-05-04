using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string ss = pubtool.CreateCompressPath();
            if (!Directory.Exists(ss))
            {
                Directory.CreateDirectory(ss);
            }
        }
    }
}
