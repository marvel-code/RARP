using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
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

using Newtonsoft.Json;

using MessageBox = System.Windows.MessageBox;

using System.Web;

using transportDataParrern;

namespace AutoRobot
{
    public partial class MainWindow
    {
        public readonly SecuritiesWindow sw = new SecuritiesWindow();
        public readonly OptionsWindow ow = new OptionsWindow();
        public MonitorWindow mw = new MonitorWindow();
        public OrdersTable ot = new OrdersTable();

        private Boolean is_Connected = false;
        private Boolean is_DdeExported = false;
        public Boolean is_Test { get; private set; }

        public QuikTrader Trader;
        public Portfolio Portfolio;
        public Security Security;

        public CandleManager candleManager;
        public WorkProcess workProcess;
        
        /// CONNECTION
        // Quik path selection
        private void selectQuikPath()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "QUIK (info.exe)|*.exe";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_quik_path.Text = dlg.FileName;
                connectionCfg.Quik_Path = dlg.FileName;
            }
        }
        // App -> Quik connection
        private void connectQuik()
        {
            if (!is_Connected)
            {
                if (tb_quik_path.Text.IsEmpty())
                    MessageBox.Show(this, "Путь к Quik не выбран");
                else
                {
                    // Создаем шлюз между Quik и роботом
                    Trader = new QuikTrader(tb_quik_path.Text);
                    // Отслеживать только во время торгов
                    Trader.ReConnectionSettings.WorkingTime = Exchange.Rts.WorkingTime;
                    // На случай неполадок соединения с Quik
                    Trader.ConnectionError +=
                        error =>
                            this.GuiSync(() =>
                            {
                                addLogMessage(error.Message);
                            });
                    // Инициализируем механизм переподключения
                    Trader.ReConnectionSettings.ConnectionRestored +=
                        () =>
                            this.GuiAsync(() =>
                            {
                                addLogMessage("Соединение восстановлено");
                            });
                    // Синхронизируем инструменты с Quik в окне выбора инструментов
                    Trader.NewSecurities += securities => this.GuiAsync(() => sw.Securities.AddRange(securities));
                    //
                    Trader.ProcessDataError += ex => addLogMessage(ex.Message);
                    // Синхронизируем данные после коннекта
                    Trader.Connected += () =>
                    {
                        candleManager = new CandleManager(Trader);
                        /*
                        this.GuiAsync(() =>
                        {
                            ExportDde.IsEnabled = ShowSecurities.IsEnabled = true;
                        });
                        */
                    };
                    // Подключаемся
                    try
                    {   
                        Trader.Connect();

                        // Обновляем портфели в ComboBox
                        cbb_portfolios.Trader = Trader;
                        // Обновляем переменные
                        is_Connected = true;
                        // Обновляем интерфейс
                        btn_connect_quik.Content = "Подключено";
                        btn_connect_quik.IsHitTestVisible = false;
                        btn_connect_quik.Focusable = false;
                        btn_connect_quik.IsTabStop = false;
                        btn_connect_quik.Background = new SolidColorBrush(Color.FromArgb(255, (byte)200, (byte)255, (byte)200)); 
                        btn_export_dde.IsEnabled = true;
                        btn_show_securities.IsEnabled = true;
                        cbb_portfolios.IsEnabled = true;
                        // Лог
                        addLogMessage("Успешное подключение к QUIK");
                    }
                    catch (Exception ex)
                    {   // Не удалось подключиться
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                /*
                Trader.Disconnect();

                is_Connected = false;
                ConnectBtn.Content = "Подключиться";
                 */
            }
        }
        // DDE Export
        private void startDdeExport()
        {
            try
            {
                addLogMessage("Экспортирование DDE");
                Trader.StartExport();
                is_DdeExported = true;
            }
            catch (Exception ex)
            {
                addLogMessage("Ошибка экспорта DDE: " + ex.Message);
            }
        }
        private void stopDdeExport()
        {
            try
            {
                addLogMessage("Принудительная остановка экспортирования DDE");
                Trader.StopExport();
                is_DdeExported = false;
            }
            catch (Exception exc)
            {
                addLogMessage(string.Format("Ошибка остановки экспорта DDE: {0}", exc.Message));
            }
        }
        
        /// MENU
        // Start work
        private void startWork()
        {
            workProcess = null; // ПОТОМ СДЕЛАТЬ НОРМАЛЬНО!!!
            if (workProcess == null)
            {
                // Выбран ли портфель и инструмент
                if (Portfolio == null || Security == null)
                {
                    MessageBox.Show(this, "Портфель или инструмент не выбран.");
                    return;
                }
                // Импорт котировок ("Все сделки")
                try
                {
                    Trader.RegisterQuotes(Security);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка получения котировок\n\n" + ex.Message);
                    return;
                }
                // Синхронизация с QUIK
                try
                {
                    workProcess = new WorkProcess(candleManager, Trader, Portfolio, Security);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            TM.init(Trader, Portfolio, Security);

            // Загрузка истории
            //..

            if (workProcess != null && workProcess.ProcessState == ProcessStates.Stopped)
            {
                // Подготавливаем интерфейс для работы
                btn_start_robot.IsEnabled = false;
                //btn_stop_robot.IsEnabled = true;
                btn_start_strategy.IsEnabled = true;
                btn_exit_from_position.IsEnabled = true;
                optionsInterface_Off();
                grid_connection.IsEnabled = false;
                // Запускаем робота
                workProcess.Start();
                // Фокус робота
                Instance.Focus();
            }
            else
                addLogMessage("Ошибка старта: Робот находится в рабочем состоянии.");
            try
            {
            }
            catch (Exception ex)
            {
                addLogMessage("Ошибка старта: " + ex.Message);
                if (workProcess != null)
                    workProcess.Stop();
            }
        }
        // Stop work
        private void stopWork()
        {
            if (TM.is_Position)
            {
                var notice = MessageBox.Show("Вы не вышли из позиции. Выйти?", "Предупреждение", MessageBoxButton.YesNoCancel);
                if (notice == MessageBoxResult.Cancel)
                    return;
                if (notice == MessageBoxResult.Yes)
                    TM.Register.ExitOrder(0, "(стоп робота)");
            }
            // Стратегия
            workProcess.Stop();
            // Интерфейс покоя
            grid_connection.IsEnabled = true;

            btn_start_strategy.IsEnabled = false;
            btn_stop_strategy.IsEnabled = false;

            btn_start_robot.IsEnabled = true;
            btn_stop_robot.IsEnabled = false;
        }
        // Save log
        private void saveLog()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = ("*.html|*.htm");
                //sfd.ShowDialog();

                String Dest_FileName = sfd.FileName;
                if (Dest_FileName == "")
                {
                    // Указываем директорию
                    String Destination_Directory = MyGlobals.Directory_Log;
                    if (!Directory.Exists(Destination_Directory))
                        Directory.CreateDirectory(Destination_Directory);
                    // Характеризуем имя файла
                    Dest_FileName = Destination_Directory + string.Format("{0}.html", DateTime.Now.ToString("yyyy.MM.dd"));
                }
                // Записываем
                using (StreamWriter hdl_write = new StreamWriter(Dest_FileName, true, System.Text.Encoding.UTF8))
                {
                    Boolean is_NewFile = hdl_write.BaseStream.Length == 0;
                    var Spoiler_Type = typeof(System.Windows.Controls.Expander);
                    //if (is_NewFile) hdl_write.WriteLine("<!DOCTYPE html><html><head><style>details{margin:0;}</style></head><body><pre>");
                    if (is_NewFile) hdl_write.WriteLine("<style>p{margin:0;font-weight:bold;}</style><table>");

                    // В обратном порядке (мы вставляем элемент на 0-ую позицию, поэтому самые ранние - последние)
                    lb_log.Items.MoveCurrentToLast();
                    do
                    {
                        var t = lb_log.Items.CurrentItem;
                        hdl_write.Write("<tr><td>");
                        if (t.GetType() == Spoiler_Type)
                        {
                            var _t = t as System.Windows.Controls.Expander;
                            hdl_write.Write(string.Format("<details><summary>{0}</summary><p>{1}</p></details>", _t.Header, _t.Content));
                        }
                        else
                            hdl_write.Write(Convert.ToString(t).Nl2Br());
                        hdl_write.WriteLine("</tr></td>");
                    }
                    while (lb_log.Items.MoveCurrentToPrevious());

                    //if (is_NewFile) hdl_write.WriteLine("</pre></body></html>");
                    hdl_write.Close();
                }
                addLogMessage("Лог сохранён");
            }
            catch (Exception ex)
            {
                addLogSpoiler("Лог не сохранён", ex.Message);
            }
        }
        
        /// TRADE
        // Start trading
        public void startTrading()
        {
            btn_start_strategy.IsEnabled = false;
            btn_stop_strategy.IsEnabled = true;
            btn_exit_from_position.IsEnabled = false;

            grid_trade_configuration.IsEnabled = false;

            workProcess.isTrade = true;

            updateRobotStatus();

            addLogMessage("СТАРТ ТОРГОВЛИ");
        }
        // Stop trading
        public void stopTrading()
        {
            btn_start_strategy.Dispatcher.Invoke(new Action(() => btn_start_strategy.IsEnabled = true));
            btn_stop_strategy.Dispatcher.Invoke(new Action(() => btn_stop_strategy.IsEnabled = false));
            btn_exit_from_position.Dispatcher.Invoke(new Action(() => btn_exit_from_position.IsEnabled = true));

            grid_trade_configuration.Dispatcher.Invoke(new Action(() => grid_trade_configuration.IsEnabled = true));


            workProcess.isTrade = false;
            TM.Register.ExitOrder(999, "Stop trading exit");

            updateRobotStatus();

            addLogMessage("СТОП ТОРГОВЛИ");
        }
        // Save indicators values to file
        public void saveIndicatorsValuesToFile(OrderInfo _OrderInfo)
        {
            return;
            if (workProcess != null)
            {
                var Current_Date = DateTime.Now.Date.ToString(@"yyyy/MM/dd");
                String _FileName = Current_Date;
                // Дирректория сохранения
                String _Directory = MyGlobals.Directory_OrderIndicatorsValues + @"\" ;
                if (!Directory.Exists(_Directory))
                    Directory.CreateDirectory(_Directory);
                // Дожидаемся очереди до значения индикаторов
                Dispatcher.Invoke(new Action(() =>
                {
                    // Создание файла
                    using (StreamWriter sw = new StreamWriter(_Directory + _FileName + ".html", true, System.Text.Encoding.UTF8))
                    {
                        if (sw.BaseStream.Length == 0)
                        {
                            sw.WriteLine("<!DOCTYPE html><html><head><link rel='stylesheet' href='IndicatorsValuesStyle.css'></head><body><table id='Orders'><h1>{0}</h1>", Current_Date);
                            
                            /**
                             * Информация заявки
                             **/

                            //Заголовок таблицы
                            sw.WriteLine("<tr>");
                            for (int i = 0; i < ot.dg_orders_table.Columns.Count; i++)
                            {
                                sw.WriteLine("<td>" + ot.dg_orders_table.Columns[i].Header + "</td>");
                            }
                            sw.WriteLine("</tr>");
                        }

                        //Строка таблицы
                        sw.WriteLine("<tr>");
                        var _Row = _OrderInfo;
                        {
                            sw.WriteLine("<td rowspan='2'>" + _Row.Number + "</td>");
                            sw.WriteLine("<td>" + _Row.Time + "</td>");
                            sw.WriteLine("<td>" + _Row.Order_ID + "</td>");
                            sw.WriteLine("<td>" + _Row.Rule_ID + "</td>");
                            sw.WriteLine("<td>" + _Row.Type + "</td>");
                            sw.WriteLine("<td>" + _Row.Direction + "</td>");
                            sw.WriteLine("<td>" + _Row.Volume + "</td>");
                            sw.WriteLine("<td>" + _Row.Security_Price + "</td>");
                            sw.WriteLine("<td>" + _Row.Order_Price + "</td>");
                            sw.WriteLine("<td>" + _Row.Order_Shift + "</td>");
                            sw.WriteLine("<td>" + _Row.Day_PNL + "</td>");
                            sw.WriteLine("<td>" + _Row.Position_PNL + "</td>");
                            sw.WriteLine("<td>" + _Row.Max_Position_PNL + "</td>");
                            sw.WriteLine("<td>" + _Row.Min_Position_PNL + "</td>");
                            sw.WriteLine("<td>" + _Row.Comment + "</td>");
                        }
                        sw.Close();
                    }
                }));
            }
        }
        
        /// INTERFACE
        // Indicators cfg
        private void saveIndicatorsConfiguration()
        {
            try
            {
                File.WriteAllText(MyGlobals.File_Indicators_Config, JsonConvert.SerializeObject(connectionCfg));
                addLogMessage("Конфигурация соединения cохранена");
            }
            catch (Exception ex)
            {
                addLogMessage("Конфигурация соединения не сохранена: " + ex.Message);
            }
        }
        private void loadIndicatorsConfiguration()
        {
            try
            {
                var _Deserialized = JsonConvert.DeserializeObject<Connection_Configuration>(File.OpenText(MyGlobals.File_Indicators_Config).ReadLine());
                if (_Deserialized == null)
                    throw new Exception("Нулевая конфигурация соединения");
                connectionCfg = _Deserialized;
                addLogMessage("Конфигурация соединения загружена");
            }
            catch (Exception ex)
            {
                addLogSpoiler("Конфигурация соединения не загружена", ex.Message);
            }
        }
        // Connection cfg
        private void saveConnectionConfiguration()
        {
            try
            {
                File.WriteAllText(MyGlobals.File_Connection_Config, JsonConvert.SerializeObject(connectionCfg));
                addLogMessage("Конфигурация соединения cохранена");
            }
            catch (Exception ex)
            {
                addLogMessage("Конфигурация соединения не сохранена: " + ex.Message);
            }
        }
        private void loadConnectionConfiguration()
        {
            try
            {
                var _Deserialized = JsonConvert.DeserializeObject<Connection_Configuration>(File.OpenText(MyGlobals.File_Connection_Config).ReadLine());
                if (_Deserialized == null)
                    throw new Exception("Нулевая конфигурация соединения");
                connectionCfg = _Deserialized;
                addLogMessage("Конфигурация соединения загружена");
            }
            catch (Exception ex)
            {
                addLogSpoiler("Конфигурация соединения не загружена", ex.Message);
            }
        }
        // Trade cfg
        private void saveTradingConfiguration()
        {
            try
            {
                File.WriteAllText(MyGlobals.File_Trade_Config, JsonConvert.SerializeObject(TM.tradeСfg));
                addLogMessage("Конфигурация торговли cохранена");
            }
            catch (Exception ex)
            {
                addLogMessage("Конфигурация торговли не сохранена: " + ex.Message);
            }
        }
        private void loadTradingConfiguration()
        {
            try
            {
                var _Deserialized = JsonConvert.DeserializeObject<Trade_Configuration>(File.OpenText(MyGlobals.File_Trade_Config).ReadLine());
                if (_Deserialized == null)
                    throw new Exception("Нулевая конфигурация торговли");
                TM.tradeСfg = _Deserialized;
                addLogMessage("Конфигурация торговли загружена");
            }
            catch (Exception ex)
            {
                addLogSpoiler("Конфигурация торговли не загружена", ex.Message);
            }
        }
        // On\Off options interface
        private void optionsInterface_On()
        {
            
        }
        private void optionsInterface_Off()
        {

        }
        // Изменяем статус робота
        public void updateRobotStatus()
        {
            mw.GuiAsync(() =>
            {
                switch (workProcess.ProcessState)
                {
                    case ProcessStates.Started:
                        if (workProcess.isWork)
                        {
                            if (workProcess.isTrade)
                            {
                                tb_status.Text = "Торговля";
                                tb_status.Background = Select_Color(MyColors.Cyan);
                            }
                            else
                            {
                                tb_status.Text = "Готов";
                                tb_status.Background = Select_Color(MyColors.Green);
                            }
                        }
                        break;
                    case ProcessStates.Stopping:
                        tb_status.Text = "Останавливаю";
                        tb_status.Background = Select_Color(MyColors.Yellow);
                        break;
                    case ProcessStates.Stopped:
                            addLogMessage("Стоп робота.\n");
                            tb_status.Text = "Остановлен";
                            tb_status.Background = Select_Color(MyColors.Red);
                        break;
                }
            });
        }
        // Изменяем текст TB в очереди
        public void setTextboxText(System.Windows.Controls.TextBox _TextBox, String _Text)
        {
            try
            {
                _TextBox.Dispatcher.Invoke(new Action(() => _TextBox.Text = _Text));
            }
            catch (Exception ex)
            {
                addLogMessage(ex.Message);
            }
        }
        public void setTextboxTextAndBackgroundByValue(System.Windows.Controls.TextBox _TextBox, Decimal _Value)
        {
            try
            {
                // Day PNL
                _TextBox.Dispatcher.Invoke(new Action(() =>
                {
                    // Значение TB
                    _TextBox.Text = _Value.ToString();
                    // Цвет TB
                    if (_Value > 0)
                        _TextBox.Background = Select_Color(MyColors.Green);
                    else if (_Value < 0)
                        _TextBox.Background = Select_Color(MyColors.Red);
                    else
                        _TextBox.Background = Select_Color(MyColors.White);
                }));
            }
            catch { }
        }
        // Добавления сообщения в лог
        public void addLogMessage(String _Message, params object[] obj)
        {
            try
            {
                lb_log.Dispatcher.Invoke(new Action(() =>
                {
                    String log_message = DateTime.Now + " |  " + string.Format(_Message, obj);
                    lb_log.Items.Insert(0, log_message);
                }));
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ошибка добавления сообщения \"" + _Message + "\" в лог:\n\n" + ex.Message);
            }
        }
        public void addLogSpoiler(String _Spoiler_Header_Message, String _Spoiler_Content_Message)
        {
            try
            {
                lb_log.Dispatcher.Invoke(new Action(() =>
                    lb_log.Items.Insert(0,
                        new System.Windows.Controls.Expander()
                        {
                            Header = DateTime.Now + " |  " + _Spoiler_Header_Message,
                            Content = _Spoiler_Content_Message,
                            Background = Select_Color(MyColors.Blue)
                        })
                ));
            } catch (Exception ex) { }
        }
        
        /// OTHERS
        // Мои цвета
        public SolidColorBrush Select_Color(MyColors _Color)
        {
            if (_Color == MyColors.Red)
                return new SolidColorBrush(Color.FromRgb(255, 200, 200));
            if (_Color == MyColors.Yellow)
                return new SolidColorBrush(Color.FromRgb(255, 255, 200));
            if (_Color == MyColors.Green)
                return new SolidColorBrush(Color.FromRgb(200, 255, 200));
            if (_Color == MyColors.Blue)
                return new SolidColorBrush(Color.FromRgb(235, 243, 255));
            if (_Color == MyColors.Cyan)
                return new SolidColorBrush(Color.FromRgb(236, 253, 255));

            return new SolidColorBrush(Colors.White);
        }
    }
    public class Connection_Configuration
    {
        public String Quik_Path { get; set; }
    }
}