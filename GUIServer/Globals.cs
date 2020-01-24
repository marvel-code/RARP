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
        public static string[] RU_dayOfWeek => new string[] { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
        public static string[] RU_dayOfWeek_abbriviated => new string[] { "ВС", "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ" };

        public static string analytics_folder { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"../Аналитика"); } }
        public static string reports_folder { get { return Path.Combine(analytics_folder, "Отчёты"); } }
        public static string billing_folder { get { return Path.Combine(analytics_folder, "Биллинг"); } }
        public static string partnersDB_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientInfo.db"); } }
        public static string log_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.log"); } }
        public static string tradesLog_fullFileName { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tradeLog.html"); } }
    }
}
