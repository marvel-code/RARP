using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;



namespace stocksharp.ServiceContracts
{
    class Log
    {
        public static void addLog(GUIServer.LogType logType, string message, params string[] args)
        {
            GUIServer.LogManager.Log(logType, message, args);

            return;
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "s#Logs.log"), DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss |  ") + string.Format(message, args) + "\r\n");
            }
            catch (Exception ex)
            {
                //ReportToWindow(WrapMessageToLog(LogType.Error, "Не удалось логировать в файл: ", ex));
            }
        }
    }
}
