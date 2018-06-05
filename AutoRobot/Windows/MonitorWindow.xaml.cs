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
    /// <summary>
    /// Логика взаимодействия для MonitoringWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        public MonitorWindow()
        {
            InitializeComponent();
        }
        // Закрытие окна
        public Boolean is_Closable = false;
        private void On_Monitor_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = !is_Closable;
        }
    }
}
