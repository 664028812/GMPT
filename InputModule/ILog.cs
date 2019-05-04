using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputModule
{
    public  class ILog
    {
       public static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
    }
}
