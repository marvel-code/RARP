using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using StockSharp.BusinessEntities;

namespace AutoRobot
{
    public partial class SecuritiesWindow
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public SecuritiesWindow()
        {
            this.Securities = new ObservableCollection<Security>();
            InitializeComponent();
        }

        public ObservableCollection<Security> Securities { get; private set; }

        public bool RealClose { get; set; }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var security = (Security)this.SecuritiesDetails.SelectedValue;
            MainWindow.Instance.Security = security;
            MainWindow.Instance.tb_current_security.Text = security.Code;
            this.Hide();
        }

        private void SecuritiesDetails_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            this.button1.IsEnabled = this.SecuritiesDetails.SelectedIndex != -1;
        }

        private void securitiesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
