using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace GUIServer
{
    public enum LogType
    {
        Debug, Info, Warn, Error, Fatal
    }

    public static class LogManager
    {
        public static string WrapMessageToLog(LogType logType, string message, params object[] args)
        {
            return DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss |  ") + string.Format("{0}:\t", logType) + string.Format(message, args);
        }

        public static void Log(LogType logType, string message, params object[] args)
        {
            // Window
            ReportToWindow(WrapMessageToLog(logType, message, args));

            // File
            ReportToFile(WrapMessageToLog(logType, message, args));
        }

        public static void ReportToWindow(string message)
        {
            MainWindow.Instance.lb_mainLog.Items.Insert(0, message);
        }
        public static void ReportToFile(string message)
        {
            try
            {
                File.AppendAllText(Globals.log_fullFileName, message + "\r\n");
            }
            catch (Exception ex)
            {
                ReportToWindow(WrapMessageToLog(LogType.Error, "Не удалось логировать в файл: ", ex));
            }
        }
    }
}