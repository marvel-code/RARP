using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRobot
{
    public class MyGlobals
    {
        public static String Robot_Name = "AutoRobot";

        public static String Directory_Robot { get { return Directory.GetCurrentDirectory() + "\\"; } }
        public static String Directory_Analytics { get { return Directory_Robot + "Analytics\\"; } }
        public static String Directory_OrderIndicatorsValues { get { return Directory_Analytics + "OrdersIndicatorsValues\\"; } }
        public static String Directory_Log { get { return Directory_Analytics + "log\\"; } }
        public static String Directory_Configuration { get { return Path.GetTempPath() + "\\"; } }

        public static String File_Indicators_Config { get { return Directory_Configuration + "indicators.cfg"; } }
        public static String File_Connection_Config { get { return Directory_Configuration + "connection.cfg"; } }
        public static String File_Trade_Config { get { return Directory_Configuration + "trade.cfg"; } }
    }
}
