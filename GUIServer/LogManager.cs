﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotLiquid;
using transportDataParrern;

namespace GUIServer
{
    public enum LogType
    {
        Debug, Info, Warn, Error, Fatal
    }

    public static class LogManager
    {
        static LogManager()
        {
            if (!File.Exists(Globals.datelog_path))
            {
                Directory.CreateDirectory(Globals.datereport_folder);
                Directory.CreateDirectory(Globals.billing_folder);
                File.Create(Globals.datelog_path).Close();
            }
        }

        private static string CURRENT_DATE_STRING => Globals.CURRENT_DATE_STRING;

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
                File.AppendAllText(Globals.datelog_path, message + "\r\n");
            }
            catch (Exception ex)
            {
                ReportToWindow(WrapMessageToLog(LogType.Error, "Не удалось логировать в файл: " + ex));
            }
        }

        // HTML report

        public class TradeData : Drop
        {
            public long id { get; set; }
            public string dateTime { get; set; }
            public string direction { get; set; }
            public decimal price { get; set; }
            public int volume { get; set; }
            public long orderNumber { get; set; }

            public TradeData(transportDataParrern.TradeData tradeData)
            {
                id = tradeData.id;
                dateTime = tradeData.dateTime;
                direction = tradeData.direction;
                price = tradeData.price;
                volume = tradeData.volume;
                orderNumber = tradeData.orderNumber;
            }
        }
        public class OrderData : Drop
        {
            public long id { get; set; }
            public long derivedOrderId { get; set; }
            public string dateTime { get; set; }
            public string secCode { get; set; }
            public decimal price { get; set; }
            public int volume { get; set; }
            public int balance { get; set; }
            public string side { get; set; }
            public string state { get; set; }
            public string comment { get; set; }

            public OrderData(transportDataParrern.OrderData orderData)
            {
                id = orderData.id;
                derivedOrderId = orderData.derivedOrderId;
                dateTime = orderData.dateTime;
                secCode = orderData.secCode;
                price = orderData.price;
                volume = orderData.volume;
                balance = orderData.balance;
                side = orderData.side;
                state = orderData.state;
                comment = orderData.comment;
            }
        }
        public class StopOrderData : Drop
        {
            public long id { get; set; }
            public string dateTime { get; set; }
            public string secCode { get; set; }
            public decimal price { get; set; }
            public decimal stopPrice { get; set; }
            public int volume { get; set; }
            public int balance { get; set; }
            public string side { get; set; }
            public string type { get; set; }
            public string state { get; set; }
            public string comment { get; set; }

            public StopOrderData(transportDataParrern.StopOrderData stopOrderData)
            {
                id = stopOrderData.id;
                dateTime = stopOrderData.dateTime;
                secCode = stopOrderData.secCode;
                price = stopOrderData.price;
                stopPrice = stopOrderData.stopPrice;
                volume = stopOrderData.volume;
                balance = stopOrderData.balance;
                side = stopOrderData.side;
                type = stopOrderData.type;
                state = stopOrderData.state;
                comment = stopOrderData.comment;
            }
        }
        private class PositionData : Drop
        {
            public bool isClosed { get; set; }
            public int PositionPNL { get; set; }
            public int DayPNL { get; set; }
            public int EntryId { get; set; }
            public int ExitId { get; set; }
            public string EntryTime { get; set; }
            public string ExitTime { get; set; }
            public int EntryPrice { get; set; }
            public int ExitPrice { get; set; }
            public string Direction { get; set; }
        }
        private class CommentInfo
        {
            public bool is_valid;
            public string RobotName;
            public string Direction;
            public int PosNumber;
            public int rule_id;

            public CommentInfo(string comment)
            {
                processComment(comment);
            }

            public void processComment(string comment)
            {
                string[] parts = comment.Split(' ', '#');
                if (parts.Length == 4)
                {
                    try
                    {
                        RobotName = parts[0];
                        Direction = parts[1];
                        PosNumber = int.Parse(parts[2]);
                        rule_id = int.Parse(parts[3]);
                        is_valid = true;
                    }
                    catch (Exception ex)
                    {
                        is_valid = false;
                        Log(LogType.Warn, "Comment {0} parse expcetion: {1}", comment, ex);
                    }
                }
            }
        }
        private class BalanceChartPoint : Drop
        {
            public string Time { get; set; }
            public int Balance { get; set; }
        }
        private class AmountChartCandle : Drop
        {
            public string Date { get; set; }
            public int Open { get; set; }
            public int Close { get; set; }
            public int Diff { get; set; }
            public int High { get; set; }
            public int Low { get; set; }
            public int OpenTail { get; set; }
            public int CloseTail { get; set; }
        }
        private class BillingRecord
        {
            public bool isValid;

            public string DayOfWeek;
            public string Date;
            public string BeginTime;
            public string EndTime;
            public int BeginAmount;
            public int EndAmount;
            public int DiffAmount;

            public BillingRecord(string record)
            {
                string[] parts = record.Split(',');
                if (parts.Length != 7)
                {
                    throw new Exception("Некорректная запись: " + record);
                }
                DayOfWeek = parts[0];
                Date = parts[1];
                BeginTime = parts[2];
                EndTime = parts[3];
                isValid = int.TryParse(parts[4], out BeginAmount)
                            && int.TryParse(parts[5], out EndAmount)
                            && int.TryParse(parts[6], out DiffAmount);
            }

            public string GetRecordString => $"{DayOfWeek},{Date},{BeginTime},{EndTime},{BeginAmount},{EndAmount},{DiffAmount}";

            public void UpdateDiffAmount()
            {
                DiffAmount = EndAmount - BeginAmount;
            }
        }
        public static void RenderHtmlReport(string filename, PartnerDataObject pd, string username)
        {
            decimal? GetOrderTradesAvrPrice(long order_id)
            {
                IEnumerable<transportDataParrern.TradeData> trades = pd.tradesData.Where(t => t.orderNumber == order_id);
                if (trades.Count() == 0)
                {
                    return null;
                }

                return trades.Sum(t => t.price * t.volume) / trades.Sum(t => t.volume);
            };
            AmountChartCandle GetAmountCandle()
            {
                string file_path = Globals.billing_file_path(username);
                string last_record = File.ReadAllLines(file_path, Encoding.Default).Last();
                BillingRecord br = new BillingRecord(last_record);
                if (br.isValid)
                {
                    string date = br.Date;
                    int amount_open = br.BeginAmount;
                    int amount_close = br.EndAmount;
                    int amount_high = Math.Max(amount_open, amount_close);
                    int amount_low = Math.Min(amount_open, amount_close);
                    return new AmountChartCandle
                    {
                        Date = date,
                        Open = amount_open,
                        Close = amount_close,
                        Diff = amount_close - amount_open,
                        High = amount_high,
                        Low = amount_low,
                        OpenTail = amount_close > amount_open ? amount_low : amount_high,
                        CloseTail = amount_close > amount_open ? amount_high : amount_low,
                    };
                }
                else
                {
                    return new AmountChartCandle(); // empty
                }
            }

            // Data
            List<PositionData> PositionsData = new List<PositionData>();
            OrderData last_order = null;
            decimal day_pnl = 0;
            for (int i = 0; i < pd.ordersEnter.Count; ++i)
            {
                OrderData enter_order = new OrderData(pd.ordersEnter[i]);
                OrderData exit_order = i < pd.ordersExit.Count ? new OrderData(pd.ordersExit[i]) : null;
                CommentInfo enterCI = new CommentInfo(enter_order.comment);

                PositionData pos_data = new PositionData
                {
                    Direction = enter_order.side,
                    EntryId = enterCI.rule_id,
                    EntryTime = enter_order.dateTime.Split(' ')[1]
                };
                decimal? enter_trades_price = GetOrderTradesAvrPrice(enter_order.id);
                decimal enter_avr_price = enter_trades_price != null ? enter_trades_price.Value : pd.Current_Price;
                pos_data.EntryPrice = (int)enter_avr_price;
                decimal position_pnl;
                if (exit_order != null)
                {
                    pos_data.isClosed = true;
                    CommentInfo exitCI = new CommentInfo(exit_order.comment);
                    pos_data.ExitId = exitCI.rule_id;
                    pos_data.ExitTime = exit_order.dateTime.Split(' ')[1];
                    decimal? exit_trades_price = GetOrderTradesAvrPrice(exit_order.id);
                    decimal exit_avr_price = exit_trades_price != null ? exit_trades_price.Value : pd.Current_Price;
                    pos_data.ExitPrice = (int)exit_avr_price;
                    position_pnl = pos_data.Direction == "Buy" ? exit_avr_price - enter_avr_price : enter_avr_price - exit_avr_price;
                    day_pnl += position_pnl;
                }
                else
                {
                    position_pnl = pos_data.Direction == "Buy" ? pd.Current_Price - enter_avr_price : enter_avr_price - pd.Current_Price;
                    day_pnl += position_pnl;
                }
                pos_data.PositionPNL = (int)position_pnl;
                pos_data.DayPNL = (int)day_pnl;

                PositionsData.Add(pos_data);
            }
            List<BalanceChartPoint> balanceChartPoints = new List<BalanceChartPoint>
            {
                new BalanceChartPoint { Time = "10:00:00", Balance = 0 }
            };
            foreach (PositionData p in PositionsData)
            {
                if (p.isClosed)
                {
                    balanceChartPoints.Add(new BalanceChartPoint { Time = p.EntryTime, Balance = (int)p.DayPNL });
                }
            }

            // Log
            string template_path = Globals.template_path;
            string template_string = File.ReadAllText(template_path);
            Template tempate = Template.Parse(template_string);
            string render = tempate.Render(Hash.FromAnonymousObject(new
            {
                date = CURRENT_DATE_STRING,
                dayOfWeek_title = Globals.RU_dayOfWeek[(int)DateTime.Now.DayOfWeek],
                balanceChartPoints,
                positions_data = PositionsData,
                orders_data = pd.ordersData.Select(x => new OrderData(x)),
                stoporders_data = pd.stopOrdersData.Select(x => new StopOrderData(x)),
                trades_data = pd.tradesData.Select(x => new TradeData(x)),
                amount_data = GetAmountCandle()
            }));
            string logfile_directory_path = Globals.datereport_folder;
            Directory.CreateDirectory(logfile_directory_path);
            string logfile_path = Globals.datereport_dashboard(username);
            File.WriteAllText(logfile_path, render);
        }

        public static void UpdateBillingData(PartnerDataObject pd, string _currentUser)
        {
            Directory.CreateDirectory(Globals.billing_folder);
            string file_path = Globals.billing_file_path(_currentUser);
            int begin_amount = (int)pd.derivativePortfolioData[0].beginAmount;
            int diff_amount = (int)pd.derivativePortfolioData[0].variationMargin;
            int current_amount = begin_amount + diff_amount;
            string date_string = DateTime.Now.ToString("yyyy.MM.dd");
            string time_string = DateTime.Now.ToString("HH:mm:ss");
            string dayOfWeak_string = Globals.RU_dayOfWeek_abbriviated[(int)DateTime.Now.DayOfWeek];
            try
            {
                if (File.Exists(file_path))
                {
                    IEnumerable<string> old_file_lines = File.ReadLines(file_path, Encoding.Default);
                    string last_record = old_file_lines.Last();
                    BillingRecord br = new BillingRecord(last_record);
                    if (br.isValid && br.Date == date_string)
                    {
                        // Overwrite record
                        br.EndAmount = current_amount;
                        br.EndTime = time_string;
                        br.UpdateDiffAmount();
                        string[] new_file_lines = old_file_lines.ToArray();
                        new_file_lines[new_file_lines.Length - 1] = br.GetRecordString;
                        File.WriteAllLines(file_path, new_file_lines, Encoding.Default);
                    }
                    else
                    {
                        // Add record
                        string[] file_lines = new string[] { $"{dayOfWeak_string},{date_string},{time_string},{time_string},{begin_amount},{current_amount},{diff_amount}" };
                        File.AppendAllLines(file_path, file_lines, Encoding.Default);
                    }
                }
                else
                {
                    // Create file
                    string[] file_lines = new string[]
                    {
                        _currentUser,
                        "День недели,Дата,Время начала торговли,Время конца торговли,Входящий остаток торговли,Исходящий остаток торговли,Изменение остатка",
                    };
                    File.WriteAllLines(file_path, file_lines, Encoding.Default);
                }
            }
            catch (Exception ex)
            {
                Log(LogType.Error, "Файл недоступен для записи: " + ex);
            }
        }
    }
}