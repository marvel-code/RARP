using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIServer
{
    public static class Globals
    {
        public static string partnersDB_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientInfo.db"); } }
        public static string log_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.log"); } }
        public static string tradesLog_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tradeLog.html"); } }
    }
}
