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
using System.Windows.Media;

using Ecng.Collections;
using Ecng.Common;
using Ecng.Xaml;

namespace AutoRobot
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static Connection_Configuration connectionCfg { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            addLogMessage(string.Format("Вас приветствует {0}. Удачной торговли!", MyGlobals.Robot_Name));
            
            // Загрузка конфигурации соединения
            connectionCfg = new Connection_Configuration();
            loadConnectionConfiguration();
            tb_quik_path.Text = connectionCfg.Quik_Path; // проблемы с привязкой
            grid_connection.DataContext = connectionCfg;
            // Загрузка конфигурации торговли
            TM.tradeСfg = new Trade_Configuration();
            loadTradingConfiguration();
            grid_trade_configuration.DataContext = TM.tradeСfg;
        }

        // Закрытие робота
        private void app_Closing(object sender, CancelEventArgs e)
        {
            if (workProcess != null)
            {
                workProcess.MyDoDispose();
            }

            saveConnectionConfiguration();
            saveTradingConfiguration();
            saveLog();

            if (is_DdeExported)
            {
                try
                {
                    Trader.StopExport();
                }
                catch { }
            }

            ow.is_Closable = true;
            mw.is_Closable = true;
            ot.is_Closable = true;

            sw.Close();
            ow.Close();
            mw.Close();
            ot.Close();
        }

        // "..."
        private void btn_quik_path_Click(object sender, RoutedEventArgs e)
        {
            selectQuikPath();
        }
        // "Инструменты"
        private void btn_show_securities_Click(object sender, RoutedEventArgs e)
        {
            sw.Show();
        }
        // "Экспорт DDE"
        private void btn_export_dde_Click(object sender, RoutedEventArgs e)
        {
            startDdeExport();
            
            btn_export_dde.Content = "Экспортировано";
            btn_export_dde.IsHitTestVisible = false;
            btn_export_dde.Focusable = false;
            btn_export_dde.IsTabStop = false;
            btn_export_dde.Background = new SolidColorBrush(Color.FromArgb(255, (byte)200, (byte)255, (byte)200));

            Instance.Focus();
        }
        // "Подключиться"
        private void btn_connect_quik_Click(object sender, RoutedEventArgs e)
        {
            connectQuik();
        }
        // "СТАРТ"
        private void btn_start_robot_Click(object sender, RoutedEventArgs e)
        {
            startWork();
        }
        // "СТОП"
        private void btn_stop_robot_Click(object sender, RoutedEventArgs e)
        {
            stopWork();
        }
        // "Вход"
        private void btn_start_strategy_Click(object sender, RoutedEventArgs e)
        {
            startTrading();
        }
        // "ВЫХОД"
        private void btn_stop_strategy_Click(object sender, RoutedEventArgs e)
        {
            if (TM.is_Position)
            {
                var notice = System.Windows.MessageBox.Show("Вы не вышли из позиции. Выйти?", "Предупреждение", MessageBoxButton.YesNoCancel);
                if (notice == MessageBoxResult.Cancel)
                    return;
                if (notice == MessageBoxResult.Yes)
                    TM.Register.ExitOrder(0, "(стоп торговли)");
            }
            stopTrading();
        }
        // Обновить портфель, после его выбора в списке
        private void cbb_portfolios_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Portfolio = cbb_portfolios.SelectedPortfolio;
        }
        // "Режим тестирования"
        private void cb_test_mode_Checked(object sender, RoutedEventArgs e)
        {
            is_Test = cb_test_mode.IsChecked.Value;
        }
        // "Мониторинг"
        private void btn_monitoring_Click(object sender, RoutedEventArgs e)
        {
            mw.Show();
        }
        // "Выйти из позиции"
        private void btn_exit_from_position_Click(object sender, RoutedEventArgs e)
        {
            TM.Register.ExitOrder(0, "(по кнопке)");
        }
        // "Сохр. лог"
        private void btn_save_log_Click(object sender, RoutedEventArgs e)
        {
            saveLog();
        }
        // "Параметры"
        private void btn_options_Click(object sender, RoutedEventArgs e)
        {
            ow.Show();
        }
        // "Сохранить настройки"
        private void btn_refresh_test_pnl_Click(object sender, RoutedEventArgs e)
        {
            saveConnectionConfiguration();
            saveTradingConfiguration();
        }
        // "Таблица"
        private void btn_orders_table_Click(object sender, RoutedEventArgs e)
        {
            ot.Show();
        }
        // "Аналитика"
        private void btn_open_analytics_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", MyGlobals.Directory_Analytics);
        }
    }
}