using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using transportDataParrern;

namespace GUIServer
{
    public partial class MainWindow : Form
    {
        // Partners info table
        private BindingSource _dgvPartnersInfoSource;


        object lock_object = new object();
        public void UpdateInfoLabel(string info)
        {
            lock (lock_object)
            {
                lbl_info.Text = info;
            }
        }


        public void UpdateDgvPartnersInfoSource()
        {
            try
            {
                _dgvPartnersInfoSource.ResetBindings(false);
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Warn, ex.ToString());
            }
        }
        public PartnerInfo GetSelectedPartnerInfo()
        {
            string selectedLogin = dgv_partnersInfo.SelectedRows.Count == 0 ? null : (dgv_partnersInfo.SelectedRows[0].Cells[0].Value ?? "...").ToString();

            if (selectedLogin != null)
            {
                if (partnersData == null || partnersData.ContainsKey(selectedLogin) == false)
                {
                    LogManager.Log(LogType.Info, "Отсутствуют данные `{0}`", selectedLogin);
                }
                else
                {
                    UpdateDgvsPartnerDataSource(selectedLogin);
                }
            }

            return PartnersManager.GetPartnersInfo().Find(x => x.login == selectedLogin);
        }

        // Partners data
        private Dictionary<string, PartnerDataObject> partnersData;

        private BindingSource _dgvSecuritiesSource;
        private BindingSource _dgvDerivativePortfolioSource;
        private BindingSource _dgvDerivativePositionsSource;
        private BindingSource _dgvTradesSource;
        private BindingSource _dgvOrdersSource;
        private BindingSource _dgvStopOrdersSource;

        public void UpdateDgvsPartnerDataSource(string login)
        {
            if (partnersData.ContainsKey(login) == false) return;

            // Init partner data tables
            _dgvSecuritiesSource = new BindingSource(new BindingList<SecuritiesRow>(partnersData[login].securitiesData), null);
            _dgvDerivativePortfolioSource = new BindingSource(new BindingList<DerivativePortfolioRow>(partnersData[login].derivativePortfolioData), null);
            _dgvDerivativePositionsSource = new BindingSource(new BindingList<DerivativePositionsRow>(partnersData[login].derivativePositionsData), null);
            _dgvTradesSource = new BindingSource(new BindingList<TradeData>(partnersData[login].tradesData), null);
            _dgvOrdersSource = new BindingSource(new BindingList<OrderData>(partnersData[login].ordersData), null);
            _dgvStopOrdersSource = new BindingSource(new BindingList<StopOrderData>(partnersData[login].stopOrdersData), null);

            dgv_securities.DataSource = _dgvSecuritiesSource;
            dgv_derivativePortfolio.DataSource = _dgvDerivativePortfolioSource;
            dgv_derivativePositions.DataSource = _dgvDerivativePositionsSource;
            dgv_trades.DataSource = _dgvTradesSource;
            dgv_orders.DataSource = _dgvOrdersSource;
            dgv_stopOrders.DataSource = _dgvStopOrdersSource;

            // Refresh

            _dgvSecuritiesSource.ResetBindings(false);
            _dgvDerivativePortfolioSource.ResetBindings(false);
            _dgvDerivativePositionsSource.ResetBindings(false);
            _dgvTradesSource.ResetBindings(false);
            _dgvOrdersSource.ResetBindings(false);
            _dgvStopOrdersSource.ResetBindings(false);
        }
        public void SetPartnerData(string login, PartnerDataObject newPartnersDataObj)
        {
            partnersData[login] = newPartnersDataObj;
            PartnersManager.UpdatePartnerTradingState(login, newPartnersDataObj.Is_Trading);
        }

        // MainWindow
        public static MainWindow Instance { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
        }
        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Update partners info from file
            PartnersManager.DownloadPartnersInfoFromFile();
            
            // Init partners info table
            _dgvPartnersInfoSource = new BindingSource(new BindingList<PartnerInfo>(PartnersManager.GetPartnersInfo()), null);
            dgv_partnersInfo.DataSource = _dgvPartnersInfoSource;
            dgv_partnersInfo.Columns[0].HeaderText = "Логин";
            dgv_partnersInfo.Columns[1].HeaderText = "В сети";
            dgv_partnersInfo.Columns[2].HeaderText = "Разрешено торговать";
            dgv_partnersInfo.Columns[3].HeaderText = "В торговле";
            dgv_partnersInfo.Columns[4].HeaderText = "Был в сети..";
        }
        private void MainWindow_Closing(object sender, FormClosingEventArgs e)
        {
            if (MyServerManager.isServerOpened && MessageBox.Show("Остановить сервер?", "Закрытие сервера", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            MyServerManager.CloseServer();
            PartnersManager.UploadPartnersInfoToFile();
        }

        // Connection
        private void switchInterfaceOnServerOpen()
        {
            gb_partnerManagerPanel.Enabled = false;

            btn_onoff.Text = "ВЫКЛЮЧИТЬ";
            btn_onoff.BackColor = Color.PaleVioletRed;
        }
        private void switchInterfaceOnServerClose()
        {
            gb_partnerManagerPanel.Enabled = true;

            btn_onoff.Text = "Включить";
            btn_onoff.BackColor = Color.PaleGreen;
        }
        private void btn_onoff_Click(object sender, EventArgs e)
        {
            if (MyServerManager.isServerOpened == false)
            {   // Open
                try
                {
                    if (MessageBox.Show("Запустить сервер?", "Открытие сервера", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        switchInterfaceOnServerOpen();

                        MyServerManager.OpenServer();
                        partnersData = new Dictionary<string, PartnerDataObject>();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogType.Error, "Ошибка старта сервера: {0}", ex.ToString());
                }
            }
            else
            {   // Close
                try
                {
                    if (MessageBox.Show("Остановить сервер?", "Закрытие сервера", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        switchInterfaceOnServerClose();

                        MyServerManager.CloseServer();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogType.Error, "Ошибка закрытия сервера: {0}", ex.ToString());
                }
            }
        }

        // Btn clicks of actions on partners
        private void btn_addPartnerInfo_Click(object sender, EventArgs e)
        {
            // Fetch: new login
            string login = tb_addLogin.Text.Trim();
            if (login == "")
            {
                MessageBox.Show("Введите логин!", "Ошибка добавления клиента", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            
            // Process
            PartnersManager.AddPartnerInfo(new PartnerInfo(login, true));
        }
        private void btn_editPartnerInfo_Click(object sender, EventArgs e)
        {
            // Fetch: selected partner
            PartnerInfo selectedPartnerInfo = GetSelectedPartnerInfo();
            if (selectedPartnerInfo == null)
            {
                MessageBox.Show("Выберите клиента!", "Ошибка изменения клиента", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            // Process
            bool allowTrade = cb_editAllowTrade.Checked;
            ushort strategyNumber = (ushort)nud_editStrategyNumber.Value;
            
            PartnersManager.EditPartnerInfo(selectedPartnerInfo.login, allowTrade, strategyNumber);
        }
        private void btn_removePartnerInfo_Click(object sender, EventArgs e)
        {
            // Fetch: selected partner
            PartnerInfo selectedPartnerInfo = GetSelectedPartnerInfo();
            if (selectedPartnerInfo == null)
            {
                MessageBox.Show("Выберите клиента!", "Ошибка удаления клиента", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            // Process
            PartnersManager.RemovePartnerInfo(selectedPartnerInfo.login);
        }

        // Others
        private void dgv_partnersInfo_Click(object sender, EventArgs e)
        {
            PartnerInfo selectedPartnerInfo = GetSelectedPartnerInfo();
            lbl_selectedPartner.Text = string.Format("Выбранный клиент: `{0}`", selectedPartnerInfo == null ? "--" : selectedPartnerInfo.login);
        }
        private void btn_openLogFile_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", Globals.log_fullFileName);
        }
    }
}