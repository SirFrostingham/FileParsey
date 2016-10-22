using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace FileParsey
{
    class Log
    {
        public static Logger Instance { get; private set; }
        static Log()
        {
            Instance = LogManager.GetLogger("logfile");
        }
    }
}
