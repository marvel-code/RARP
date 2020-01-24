using System;
using System.IO;

namespace GUIServer
{
    public static class Globals
    {
        public static string CURRENT_DATE_STRING => DateTime.Now.ToString(@"yyyy.MM.dd");
        public static string[] RU_dayOfWeek => new string[] { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
        public static string[] RU_dayOfWeek_abbriviated => new string[] { "ВС", "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ" };

        public static string template_path => Path.Combine(Environment.CurrentDirectory, "ClientDayReportOnServerTemplate", "day_report_template.html");
        public static string analytics_folder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"../Аналитика");
        public static string reports_folder => Path.Combine(analytics_folder, "Отчёты");
        public static string datereport_folder => Path.Combine(analytics_folder, "Отчёты", CURRENT_DATE_STRING);
        public static string datereport_dashboard(string username)
        {
            return Path.Combine(datereport_folder, "Сводка {username}.html");
        }

        public static string datereport_tradesLog_path(string username)
        {
            return Path.Combine(datereport_folder, $"Сделки и индикаторы {username}.html");
        }

        public static string billing_folder => Path.Combine(analytics_folder, "Биллинг");
        public static string billing_file_path(string username)
        {
            return Path.Combine(billing_folder, $"Сводка {username}.csv");
        }
        public static string partnersDB_fullFileName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clientInfo.db");
        public static string log_fullFileName => Path.Combine(datereport_folder, "log.log");
    }
}
