using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using transportDataParrern;
using DotLiquid;
using StockSharp.BusinessEntities;

namespace GUIServer
{
    public enum LogType
    {
        Debug, Info, Warn, Error, Fatal
    }

    public static class LogManager
    {
        static string CURRENT_DATE_STRING => DateTime.Now.ToString(@"yyyy.MM.dd");

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

        // HTML report
        
        class PositionData
        {
            public decimal? PositionPNL;
            public decimal DayPNL;
            public int EntryId;
            public int? ExitId;
            public TimeSpan EntryTime;
            public TimeSpan? ExitTime;
            public string Direction;
        }
        class BalanceChartPoint : Drop
        {
            public string Time { get; set; }
            public int Balance { get; set; }
        }
        public static void RenderHtmlReport(string filename, PartnerDataObject pd)
        {
            // Data
            decimal? GetOrderTradesAvrPrice(long order_id)
            {
                IEnumerable<TradeData> trades = pd.tradesData.Where(t => t.orderNumber == order_id);
                if (trades.Count() == 0)
                    return null;
                return trades.Sum(t => t.price * t.volume) / trades.Sum(t => t.volume);
            };
            int GetOrderRuleId(OrderData order) => int.Parse(order.comment.Split('|').Last());
            List<PositionData> PositionsData = new List<PositionData>();
            OrderData last_order = null;
            decimal day_pnl = 0;
            for (int i = 0; i < pd.ordersEnter.Count || i < pd.ordersExit.Count; ++i)
            {
            }
            List<BalanceChartPoint> balanceChartPoints = new List<BalanceChartPoint>();
            balanceChartPoints.Add(new BalanceChartPoint { Time = TimeSpan.FromHours(10).ToString(), Balance = (int)pd.derivativePortfolioData[0].beginAmount });
            balanceChartPoints.Add(new BalanceChartPoint { Time = TimeSpan.FromHours(11).ToString(), Balance = (int)pd.derivativePortfolioData[0].beginAmount + 200 });

            // Log
            string template_path = Path.Combine(Environment.CurrentDirectory, "ClientDayReportOnServerTemplate/day_report_template.html");
            string template_string = File.ReadAllText(template_path);
            Template tempate = Template.Parse(template_string);
            string render = tempate.Render(Hash.FromAnonymousObject(new {
                date = CURRENT_DATE_STRING,
                balanceChartPoints = balanceChartPoints,
                positions_data = PositionsData,
                orders_data = pd.ordersData,
                stoporders_data = pd.stopOrdersData,
                trades_data = pd.tradesData
            }));
            string logfile_directory_path = Path.Combine(Environment.CurrentDirectory, "Отчёты", CURRENT_DATE_STRING);
            Directory.CreateDirectory(logfile_directory_path);
            string logfile_path = Path.Combine(logfile_directory_path, $"{filename}.html");
            File.WriteAllText(logfile_path, render);
        }
    }
}