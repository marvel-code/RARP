using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;
using System.Xml;

using Ecng.Collections;
using Ecng.Common;
using Ecng.Xaml;

using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Quik;
using System.Collections.ObjectModel;

namespace AutoRobot
{
    public class TM // TradeManager
    {
        private static MainWindow mw { get { return MainWindow.Instance; } }
        private static OrdersTable ot { get { return OrdersTable.Instance; } }
        // Test Trades
        private static List<MyTrade> testTrades = new List<MyTrade>();
        // Orders info
        public static ObservableCollection<OrderInfo> ordersInfoArray;
        // Configuration
        public static Trade_Configuration tradeСfg;

        // INIT
        public static void init(QuikTrader _QuikTrader, Portfolio _Portfolio, Security _Security)
        {
            MySecurity = _Security;
            MyTrader = _QuikTrader;
            MyPortfolio = _Portfolio;

            MyTrader.MarketTimeOffset = DateTime.Now - MyTrader.MarketTime;

            Orders_Enter = new List<Order>();
            Orders_Exit = new List<Order>();
            Orders_Stop = new List<Order>();
            Orders_Failed = new List<Order>();
            if (!tradeСfg.is_New_Session)
            {
                Orders_Enter.Add(MyDayOrders.LastOrDefault(or => or.Comment == string.Format("{0} {1}", Robot_Trade_Name, OrderType.Enter)));
                if (Orders_Enter[0] == null)
                    Orders_Enter = new List<Order>();

                Orders_Exit.Add(MyDayOrders.LastOrDefault(or => or.Comment == string.Format("{0} {1}", Robot_Trade_Name, OrderType.Exit) || or.Comment == string.Format("{0} {1}", Robot_Trade_Name, OrderType.Stop) && or.StopCondition == null));
                if (Orders_Exit[0] == null || last_ExitOrder.Time < last_EnterOrder.Time)
                    Orders_Exit = new List<Order>();

                Orders_Stop.Add(MyDayOrders.LastOrDefault(or => or.Comment == string.Format("{0} {1}", Robot_Trade_Name, OrderType.Stop) && or.StopCondition != null));
                if (Orders_Stop[0] == null)
                    Orders_Stop = new List<Order>();
            }
        }
        public static void loadTestTrades(List<MyTrade> _Test_Trades)
        {
            testTrades = _Test_Trades;
        }
        public static void updateValues() { }

        // Connection
        public static Security MySecurity { get; private set; }
        public static QuikTrader MyTrader { get; private set; }
        public static Portfolio MyPortfolio { get; private set; }
        public static DateTime terminalDateTime
        {
            get
            {
                try
                {
                    return MyTrader.Terminal.ServerTime.Value;
                }
                catch
                {
                    return DateTime.Now;
                }
            }
        }
        // Orders
        public static Order last_EnterOrder { get { return Orders_Enter.Count == 0 ? null : Orders_Enter.Last(); } }
        public static Order last_ExitOrder { get { return Orders_Exit.Count == 0 ? null : Orders_Exit.Last(); } }
        public static Order last_StopOrder { get { return Orders_Stop.Count == 0 ? null : Orders_Stop.Last(); } }
        public static Order last_FailedOrder { get { return Orders_Failed.Count == 0 ? null : Orders_Failed.Last(); } }
        public static List<Order> MyAllOrders { get { return MyTrader == null ? null : MyTrader.Orders.Where(or => !MyTrader.StopOrders.Contains(or)).ToList(); } }
        public static List<Order> MyDayOrders { get { return MyTrader == null ? null : MyTrader.Orders.Where(or => or.Time.Day == terminalDateTime.Day).ToList(); } }
        public static List<Order> Orders_Enter { get; private set; }
        public static List<Order> Orders_Exit { get; private set; }
        public static List<Order> Orders_Stop { get; private set; }
        public static List<Order> Orders_Failed { get; private set; }
        // Common
        public static Decimal last_Rule_ID { get; private set; }
        public static String Robot_Trade_Name { get { return "AR"; } }
        public static Decimal Current_Price { get { return MySecurity == null ? 0 : MySecurity.LastTrade.Price; } }
        public static Decimal Quik_Position { get { return MyTrader == null || MyTrader.GetPosition(MyPortfolio, MySecurity).IsNull() ? 0 : MyTrader.GetPosition(MyPortfolio, MySecurity).CurrentValue; } }
        public static Decimal Current_Position
        {
            get
            {
                if (Orders_Enter.Count == 0)
                    return 0;

                Decimal result = 0;
                
                result += last_EnterOrder.Direction == OrderDirections.Buy ? last_EnterOrder.Volume - last_EnterOrder.Balance : last_EnterOrder.Balance - last_EnterOrder.Volume;

                if (Orders_Exit.Count != 0 && last_ExitOrder.Time > last_EnterOrder.Time)
                    result += last_ExitOrder.Direction == OrderDirections.Buy ? last_ExitOrder.Volume - last_ExitOrder.Balance : last_ExitOrder.Balance - last_ExitOrder.Volume;

                return result;
            }
        }
        public static Decimal Day_PNL
        {
            get
            {
                //return 0;   // Конфликт, если существует заявка, одновременно закрывающая текущую позицию и открывающая новую.
                            // Текущий вариант работает только с строго открывающими и закрывающими свою позицию заявками.


                // INIT
                Decimal Result = 0;
                Decimal Trade_Position = 0;
                // Массивы
                var _MyOrders_Array = MyDayOrders.Where(or => or.Time.Day == terminalDateTime.Day); // Мои заявки текущей стратегии
                // Перебираем заявки
                foreach (var or in _MyOrders_Array)
                {
                    // Добавляем среднюю цену сделок заявки
                    if (or.Direction == OrderDirections.Buy)
                    {
                        Result -= Get_Average_Trades_Price(or);
                        Trade_Position -= Get_Sum_Trades_Volume(or);
                    }
                    else
                    {
                        Result += Get_Average_Trades_Price(or);
                        Trade_Position += Get_Sum_Trades_Volume(or);
                    }
                }
                // Если находимся в позиции
                if (Trade_Position > 0)
                    Result -= Current_Price;
                else if (Trade_Position < 0)
                    Result += Current_Price;
                // Если PNL ненормальный какой-то
                if (Result.Abs() > 10000)
                    Add_Log_Message("Зафиксирован невалидный PNL: " + Result);

                return Result;
            }
        }
        public static Decimal Session_PNL
        {
            get
            {
                // INIT
                Decimal Result = 0;
                Decimal Trade_Position = 0;
                // Массивы
                var _MyOrders_Array = Orders_Enter.Concat(Orders_Exit).ToList(); // Мои заявки текущей стратегии
                // Перебираем заявки
                foreach (var or in _MyOrders_Array)
                {
                    // Добавляем среднюю цену сделок заявки
                    if (or.Direction == OrderDirections.Buy)
                    {
                        Result -= Get_Average_Trades_Price(or);
                        Trade_Position -= Get_Sum_Trades_Volume(or);
                    }
                    else
                    {
                        Result += Get_Average_Trades_Price(or);
                        Trade_Position += Get_Sum_Trades_Volume(or);
                    }
                }
                // Если находимся в позиции
                if (Trade_Position > 0)
                    Result -= Current_Price;
                else if (Trade_Position < 0)
                    Result += Current_Price;
                // Если PNL ненормальный какой-то
                if (Result.Abs() > 10000)
                    Add_Log_Message("Зафиксирован невалидный PNL: " + Result);

                return Result;
            }
        }
        public static Decimal _Position_PNL { get; private set; }
        public static Decimal Position_PNL
        {
            get
            {
                decimal Result = 0;

                if (Current_Position != 0)
                {
                    if (!tradeСfg.is_Test)
                    {
                        var _last_EnterOrder = last_EnterOrder;
                        // Взяли последнюю входую заявку
                        if (_last_EnterOrder != null)
                        {
                            if (_last_EnterOrder.Direction == OrderDirections.Buy)
                                Result = Current_Price - Get_Average_Trades_Price(_last_EnterOrder);
                            else
                                Result = Get_Average_Trades_Price(_last_EnterOrder) - Current_Price;
                        }
                    }
                    else
                    {
                        // Берём последнюю заявку
                        var or = testTrades[testTrades.Count - 1];
                        // Считаем
                        Result = Current_Price - or.Order.Price;
                        if (or.Order.Direction == OrderDirections.Sell)
                            Result *= -1;
                    }
                }
                else if (Min_Position_PNL != 0 || Max_Position_PNL != 0)
                    Min_Position_PNL = Max_Position_PNL = 0;

                if (Result.Abs() > 10000)
                {
                    Add_Log_Message("Зафиксирован невалидный PnL: " + Result);
                    mw.stopTrading();
                }
                else
                {
                    if (Result > Max_Position_PNL)
                        Max_Position_PNL = Result;
                    else if (Result < Min_Position_PNL)
                        Min_Position_PNL = Result;
                }

                _Position_PNL = Result;
                return Result;
            }
        }
        public static Decimal _Max_Position_PNL { get; private set; }
        public static Decimal Max_Position_PNL { get; private set; }
        public static Decimal _Min_Position_PNL { get; private set; }
        public static Decimal Min_Position_PNL { get; private set; }
        public static Decimal ActiveExitOrdersBalance
        {
            get
            {
                decimal Balance = 0;
                foreach (var Order in Orders_Exit)
                    if (Order.State == OrderStates.Active)
                        if (Order.Direction == OrderDirections.Buy)
                        {
                            Balance += Order.Balance;
                        }
                        else
                        {
                            Balance -= Order.Balance;
                        }
                return Balance;
            }
        }
        public static Decimal Exceptions_Count { get; private set; }
        public static void resetExceptionsCount() { Exceptions_Count = 0; }
        public static Decimal Max_Exceptions_Count { get; private set; }
        public static Boolean is_Position { get { return Orders_Enter.Count == 0 ? false : Orders_Exit.Count == 0 ? true : last_EnterOrder.Time > last_ExitOrder.Time; } }

        public struct Register
        {
            public static Order FailedOrder(Order _FailedOrder, OrderType _OrderType)
            {
                Order New_Order;
                // Снова регистрируем заявку на бирже
                if (_OrderType != OrderType.Stop)
                    New_Order = Order(0, _FailedOrder.Balance, _FailedOrder.Direction, _OrderType, "[REREG]");
                else
                    New_Order = StopOrder(_FailedOrder.Volume, _FailedOrder.Direction, (QuikStopConditionTypes)_FailedOrder.StopCondition.Parameters["Type"], "Стоп-заявка [REREG]");
                // Переносим неудачную заявку в массив неудачников
                List<Order> Orders_Array = null;
                if (_OrderType == OrderType.Enter)
                    Orders_Array = Orders_Enter;
                else if (_OrderType == OrderType.Exit)
                    Orders_Array = Orders_Exit;
                else if (_OrderType == OrderType.Stop)
                    Orders_Array = Orders_Stop;

                Orders_Failed.Add(Orders_Array[Orders_Array.Count - 1]);
                Orders_Array[Orders_Array.Count - 1] = New_Order;
                Exceptions_Count++;

                return New_Order;
            }

            public static Order Order(Decimal _Rule_ID, Decimal _Order_Volume, OrderDirections _Order_Direction, OrderType _OrderType, String _Comment = "")
            {
                // Создаём заявку
                Order New_Order = new Order
                {
                    Trader = MyTrader,
                    Portfolio = MyPortfolio,
                    Security = MySecurity,

                    Price = _Order_Direction == OrderDirections.Buy ? Current_Price + tradeСfg.Order_Shift : Current_Price - tradeСfg.Order_Shift,
                    Direction = _Order_Direction,
                    Volume = _Order_Volume,

                    Comment = string.Format("{0} {1}", Robot_Trade_Name, _OrderType)
                };
                // Регистрируем тип заявки
                if (_OrderType == OrderType.Enter)
                {
                    Orders_Enter.Add(New_Order);
                    _Min_Position_PNL = _Max_Position_PNL = 0;
                }
                else
                {
                    Orders_Exit.Add(New_Order);
                    _Comment += " — " + _Position_PNL + "pnl";
                }
                // Регистрируем заявку на бирже
                RegisterOrder(New_Order);
                // Регистрация информации о заявке
                Register_Order_Info(_Rule_ID, _OrderType, _Comment);
                // Лог
                Add_Log_Spoiler(string.Format("{0} {1} {2}", _Order_Direction.ToString().ToUpper(), _OrderType.ToString(), _Comment), string.Format("ID правила: {0}\nОбъем: {1}\nТекущая цена: {2}\nСдвиг: {3}", _Rule_ID, _Order_Volume, Current_Price, tradeСfg.Order_Shift));

                return New_Order;
            }
            public static Order ExitOrder(Decimal _Rule_ID, String _Comment = "")
            {
                _Min_Position_PNL = Min_Position_PNL;
                _Max_Position_PNL = Max_Position_PNL;

                if (last_ExitOrder != null && last_EnterOrder.Time < last_ExitOrder.Time && last_ExitOrder.State == OrderStates.Active) // Если позиция уже закрывается
                {
                    Add_Log_Spoiler(string.Format("Позиция уже закрывается! (попытка: {0})", _Comment), "ID: " + last_ExitOrder.Id);
                    return null;
                }
                else
                {
                    if (Current_Position == 0)
                    {
                        Add_Log_Spoiler(string.Format("Ошибка закрытия позиции."), "Вы не в позиции.");
                        return null;
                    }

                    // Отменяем активные заявки
                    if (last_EnterOrder != null && last_EnterOrder.State == OrderStates.Active)
                        MyTrader.CancelOrder(last_EnterOrder);
                    if (last_StopOrder != null && last_StopOrder.State == OrderStates.Active)
                        MyTrader.CancelOrder(last_StopOrder);
                    // Регистрируем
                    return Order(_Rule_ID, Math.Abs(Current_Position), Current_Position < 0 ? OrderDirections.Buy : OrderDirections.Sell, OrderType.Exit, _Comment);
                }
            }
            public static Order StopOrder(Decimal _StopOrder_Volume, OrderDirections _StopOrder_Direction, QuikStopConditionTypes _QuikStopConditionTypes, String _Comment = "")
            {
                // Регистрируем заявку
                Order New_StopOrder = new Order
                {
                    Trader = MyTrader,
                    Portfolio = MyPortfolio,
                    Security = MySecurity,

                    // Цена дочерней заявки после срабатывания Стоп-лимит
                    Price = _StopOrder_Direction == OrderDirections.Buy ? Current_Price + tradeСfg.Order_StopLoss + tradeСfg.Order_Shift : Current_Price - tradeСfg.Order_StopLoss - tradeСfg.Order_Shift,
                    Direction = _StopOrder_Direction,
                    Volume = _StopOrder_Volume,

                    Comment = string.Format("{0} {1}", Robot_Trade_Name, OrderType.Stop),

                    Type = OrderTypes.Conditional,
                    StopCondition = new QuikStopCondition
                    {
                        Type = _QuikStopConditionTypes,
                        // Цена срабатывания Тейк-профит
                        StopPrice = _StopOrder_Direction == OrderDirections.Buy ? Current_Price - tradeСfg.Order_TakeProfit : Current_Price + tradeСfg.Order_TakeProfit,
                        // Цена срабатывания Стоп-лимит
                        StopLimitPrice = _StopOrder_Direction == OrderDirections.Buy ? Current_Price + tradeСfg.Order_StopLoss : Current_Price - tradeСfg.Order_StopLoss,
                        Offset = tradeСfg.Order_Offset,
                        // Отступ после срабатывания ТЕЙК-ПРОФИТ (только)
                        Spread = tradeСfg.Order_Shift
                    },
                };
                RegisterOrder(New_StopOrder);
                Orders_Stop.Add(New_StopOrder);
                // Лог
                Add_Log_Spoiler(string.Format("{0} {1} {2}", _StopOrder_Direction.ToString().ToUpper(), OrderType.Stop.ToString(), _Comment), string.Format("{0} стоп-заявка\nОбъем: {1}\nТекущая цена: {2}\nПрофит: {3}\nЛосс: {4}\nСдвиг: {5}\nОтступ: {6}", _StopOrder_Direction.ToString().ToUpper(), _StopOrder_Volume, Current_Price, tradeСfg.Order_TakeProfit, tradeСfg.Order_StopLoss, tradeСfg.Order_Shift, tradeСfg.Order_Offset));

                return New_StopOrder;
            }
        }

        public struct Cancel
        {
            public static void Last_EnterOrder(String _Spoiler_Header_Message = "")
            {
                _Spoiler_Header_Message = "Отмена последней входной заявки " + _Spoiler_Header_Message;
                if (last_EnterOrder.State == OrderStates.Active)
                {
                    Add_Log_Spoiler(_Spoiler_Header_Message, string.Format("Баланс: {0}\nПортфель: {1}\nИнструмент: {2}\nID: {3}", last_EnterOrder.Balance, MyPortfolio.Name, MySecurity.Name, last_EnterOrder.Id));
                    MyTrader.CancelOrder(last_EnterOrder);
                }
                else
                {
                    Add_Log_Spoiler(_Spoiler_Header_Message, "Заявка не активна");
                }
            }

            public static void Orders(String _Comment = "", Boolean _NoticeIfNoActive = false)
            {
                int CanceledOrders_Count = 0;
                List<long> CanceledOrders_Array = new List<long>();
                // Отменяем заявки
                var _MyOrders = MyAllOrders;
                foreach (var Order in _MyOrders)
                    if (Order.State == OrderStates.Active)
                    {
                        MyTrader.CancelOrder(Order);

                        CanceledOrders_Array.Add(Order.Id);
                        CanceledOrders_Count++;
                    }
                // Получаем список отменных заявок
                String CanceledOrders_List = String.Join(", ", CanceledOrders_Array);
                // Лог
                _Comment = "Отмена активных заявок " + _Comment;
                if (CanceledOrders_Count != 0)
                    Add_Log_Spoiler(_Comment, string.Format("Количество: {0}\nПортфель: {1}\nИнструмент: {2}\nID: {3}", CanceledOrders_Count, MyPortfolio.Name, MySecurity.Name, CanceledOrders_List));
                else if (_NoticeIfNoActive)
                    Add_Log_Spoiler(_Comment, "Активных заявок не обнаружено");
            }
            public static void EnterOrders(String _Comment = "", Boolean _NoticeIfNoActive = false)
            {
                int CanceledOrders_Count = 0;
                List<long> CanceledOrders_Array = new List<long>();
                // Отменяем заявки
                foreach (var Order in Orders_Enter)
                    if (Order.State == OrderStates.Active)
                    {
                        MyTrader.CancelOrder(Order);

                        CanceledOrders_Array.Add(Order.Id);
                        CanceledOrders_Count++;
                    }
                // Получаем список отменных заявок
                String CanceledOrders_List = String.Join(", ", CanceledOrders_Array);
                // Лог
                _Comment = "Отмена активных входных заявок " + _Comment;
                if (CanceledOrders_Count != 0)
                    Add_Log_Spoiler(_Comment, string.Format("Количество: {0}\nПортфель: {1}\nИнструмент: {2}\nID: {3}", CanceledOrders_Count, MyPortfolio.Name, MySecurity.ShortName, CanceledOrders_List));
                else if (_NoticeIfNoActive)
                    Add_Log_Spoiler(_Comment, "Активных входных заявок не обнаружено");
            }
            public static void ExitOrders(String _Comment = "", Boolean _NoticeIfNoActive = false)
            {
                int CanceledOrders_Count = 0;
                List<long> CanceledOrders_Array = new List<long>();
                // Отменяем заявки
                foreach (var Order in Orders_Exit)
                    if (Order.State == OrderStates.Active)
                    {
                        MyTrader.CancelOrder(Order);

                        CanceledOrders_Array.Add(Order.Id);
                        CanceledOrders_Count++;
                    }
                // Получаем список отменных заявок
                string CanceledOrders_List = String.Join(", ", CanceledOrders_Array);
                // Лог
                _Comment = "Отмена активных выходных заявок " + _Comment;
                if (CanceledOrders_Count != 0)
                    Add_Log_Spoiler(_Comment, string.Format("Количество: {0}\nПортфель: {1}\nИнструмент: {2}\nID: {3}", CanceledOrders_Count, MyPortfolio.Name, MySecurity.ShortName, CanceledOrders_List));
                else if(_NoticeIfNoActive)
                    Add_Log_Spoiler(_Comment, "Активных выходных заявок не обнаружено");
            }
            public static void StopOrders(String _Comment = "", Boolean _NoticeIfNoActive = false)
            {
                int CanceledOrders_Count = 0;
                List<long> CanceledOrders_Array = new List<long>();
                // Отменяем заявки
                foreach (var StopOrder in MyTrader.StopOrders)
                    if (StopOrder.State == OrderStates.Active)
                    {
                        MyTrader.CancelOrder(StopOrder);

                        CanceledOrders_Array.Add(StopOrder.Id);
                        CanceledOrders_Count++;
                    }
                // Получаем список отменных заявок
                string CanceledOrders_List = String.Join(", ", CanceledOrders_Array);
                // Лог
                _Comment = "Отмена активных стоп-заявок " + _Comment;
                if (CanceledOrders_Count != 0)
                    Add_Log_Spoiler(_Comment, string.Format("Количество: {0}\nПортфель: {1}\nИнструмент: {2}\nID: {3}", CanceledOrders_Count, MyPortfolio.Name, MySecurity.ShortName, CanceledOrders_List));
                else if (_NoticeIfNoActive)
                    Add_Log_Spoiler(_Comment, "Активных стоп-заявок не обнаружено");
            }
        }
        // Регистрация заявки
        public static Boolean RegisterOrder(Order _Order)
        {
            try
            {
                MyTrader.RegisterOrder(_Order);
                return true;
            }
            catch (Exception ex)
            {
                Add_Log_Message("Ошибка регистрации заявки: " + ex.Message);
                return false;
            }
        }
        
        /// ORDERS
        // Проверка выхода по стоп-заявке
        public static Boolean is_Exit_From_Stop_Order()
        {
            if (last_StopOrder != null && last_StopOrder.DerivedOrder != null && (last_ExitOrder == null || last_ExitOrder.Time < last_EnterOrder.Time))
            {
                Order _New_ExitOrder = last_StopOrder.DerivedOrder;
                Orders_Exit.Add(_New_ExitOrder);

                String Comment = "(стоп-выход)";
                // Регистрация информации о заявке
                Register_Order_Info(0, OrderType.Exit, Comment);
                // Лог
                Add_Log_Spoiler(string.Format("{0} {1} {2}", _New_ExitOrder.Direction.ToString().ToUpper(), OrderType.Exit, Comment), string.Format("{0} заявка\nОбъем: {1}\nТекущая цена: {2}\nСдвиг: {3}", _New_ExitOrder.Direction.ToString().ToUpper(), _New_ExitOrder.Volume, Current_Price, tradeСfg.Order_Shift));
                return true;
            }

            return false;
        }
        // Средняя цена сделок по заявке
        private static Decimal Get_Average_Trades_Price(Order _Order)
        {
            return Get_Order_Trades(_Order).Sum(t => t.Trade.Price * t.Trade.Volume / Get_Sum_Trades_Volume(_Order));
        }
        // Суммарный объем сделок по заявке
        private static Decimal Get_Sum_Trades_Volume(Order _Order)
        {
            return Get_Order_Trades(_Order).Sum(t => t.Trade.Volume);
        }
        // Массив сделок по заявке
        private static List<MyTrade> Get_Order_Trades(Order _Order)
        {
            return MyTrader.MyTrades.Where(t => t.Order.Id == _Order.Id).ToList();
        }
        
        ///  INFO
        // Таблица + значения индикаторов
        private static void Register_Order_Info(Decimal _Rule_ID, OrderType _OrderType, String _Comment)
        {
            last_Rule_ID = _Rule_ID;

            Order New_Order = null;
            if (_OrderType == OrderType.Enter)
                New_Order = Orders_Enter.Last();
            else if (_OrderType == OrderType.Exit)
                New_Order = Orders_Exit.Last();

            if (New_Order == null)
                return;

            ot.GuiSync(() =>
                ot.Dispatcher.Invoke(new Action(() =>
                {
                    while (New_Order.Id == 0 || New_Order.Time.Year != terminalDateTime.Year)
                        continue;
                    var _OrderInfo = new OrderInfo(_Rule_ID, New_Order, _OrderType, _Comment);
                    // Значения индикаторов
                    mw.saveIndicatorsValuesToFile(_OrderInfo);
                    // Таблица
                    ordersInfoArray.Add(_OrderInfo);
                }))
            );
        }
        public static void Add_Log_Message(String _Message)
        {
            mw.addLogMessage(_Message);
        }
        private static void Add_Log_Spoiler(String _Spoiler_Header_Message, String _Spoiler_Content_Message)
        {
            mw.addLogSpoiler(_Spoiler_Header_Message, _Spoiler_Content_Message);
        }
    }
    public class Trade_Configuration
    {
        public Boolean is_Test { get; set; }
        public Boolean is_New_Session { get; set; }
        public int Max_Exceptions_Count { get; set; }

        public int Order_Volume { get; set; }
        public int Order_Shift { get; set; }
        public int Order_TakeProfit { get; set; }
        public int Order_StopLoss { get; set; }
        public int Order_Offset { get; set; }

        public int Max_Day_Profit { get; set; }
        public int Max_Day_Loss { get; set; }
    }
    public class OrderInfo
    {
        public OrderInfo(Decimal _Rule_ID, Order _Order, OrderType _Order_Type, String _Comment = "")
        {
            Number = TM.ordersInfoArray.Count + 1;
            Time = _Order.Time;
            Order_ID = _Order.Id;
            Rule_ID = _Rule_ID;
            Type = _Order_Type;
            Direction = _Order.Direction;
            Volume = _Order.Volume;
            Security_Price = TM.Current_Price;
            Order_Price = _Order.Price;
            Order_Shift = TM.tradeСfg.Order_Shift;
            Day_PNL = TM.Session_PNL;
            Position_PNL = TM._Position_PNL;
            Max_Position_PNL = TM._Max_Position_PNL;
            Min_Position_PNL = TM._Min_Position_PNL;
            Comment = _Comment;
        }
        public int Number { get; set; }
        public DateTime Time { get; private set; }
        public Decimal Order_ID { get; private set; }
        public Decimal Rule_ID { get; private set; }
        public OrderType Type { get; private set; }
        public OrderDirections Direction { get; private set; }
        public Decimal Volume { get; private set; }
        public Decimal Security_Price { get; private set; }
        public Decimal Order_Price { get; private set; }
        public Decimal Order_Shift { get; private set; }
        public Decimal Day_PNL { get; private set; }
        public Decimal Position_PNL { get; private set; }
        public Decimal Max_Position_PNL { get; private set; }
        public Decimal Min_Position_PNL { get; private set; }
        public String Comment { get; private set; }
    }
}