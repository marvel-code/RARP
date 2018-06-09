using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoRobot
{
    public partial class OrdersTable : Window
    {
        public static OrdersTable Instance { get; private set; }
        public OrdersTable()
        {
            InitializeComponent();

            Instance = this;
            TM.ordersInfoArray = new System.Collections.ObjectModel.ObservableCollection<OrderInfo>();
            dg_orders_table.ItemsSource = TM.ordersInfoArray;
        }
        public Boolean is_Closable = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = !is_Closable;
        }

        private void dg_orders_table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
