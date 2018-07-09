namespace GUIServer
{
    partial class MainWindow
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgv_partnersInfo = new System.Windows.Forms.DataGridView();
            this.btn_onoff = new System.Windows.Forms.Button();
            this.gb_partnerManagerPanel = new System.Windows.Forms.GroupBox();
            this.lbl_selectedPartner = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_editStrategyNumber = new System.Windows.Forms.NumericUpDown();
            this.cb_editAllowTrade = new System.Windows.Forms.CheckBox();
            this.lbl_addLogin = new System.Windows.Forms.Label();
            this.tb_addLogin = new System.Windows.Forms.TextBox();
            this.btn_removeClientInfo = new System.Windows.Forms.Button();
            this.btn_editClientInfo = new System.Windows.Forms.Button();
            this.btn_addClientInfo = new System.Windows.Forms.Button();
            this.lb_mainLog = new System.Windows.Forms.ListBox();
            this.btn_openLogFile = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.dgv_stopOrders = new System.Windows.Forms.DataGridView();
            this.label6 = new System.Windows.Forms.Label();
            this.dgv_trades = new System.Windows.Forms.DataGridView();
            this.label5 = new System.Windows.Forms.Label();
            this.dgv_orders = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.dgv_derivativePositions = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.dgv_derivativePortfolio = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_securities = new System.Windows.Forms.DataGridView();
            this.lbl_info = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_partnersInfo)).BeginInit();
            this.gb_partnerManagerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_editStrategyNumber)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_stopOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trades)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_orders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_derivativePositions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_derivativePortfolio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_securities)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_partnersInfo
            // 
            this.dgv_partnersInfo.AllowUserToAddRows = false;
            this.dgv_partnersInfo.AllowUserToDeleteRows = false;
            this.dgv_partnersInfo.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_partnersInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_partnersInfo.Location = new System.Drawing.Point(12, 212);
            this.dgv_partnersInfo.Name = "dgv_partnersInfo";
            this.dgv_partnersInfo.ReadOnly = true;
            this.dgv_partnersInfo.Size = new System.Drawing.Size(447, 170);
            this.dgv_partnersInfo.TabIndex = 0;
            this.dgv_partnersInfo.Click += new System.EventHandler(this.dgv_partnersInfo_Click);
            // 
            // btn_onoff
            // 
            this.btn_onoff.BackColor = System.Drawing.Color.PaleGreen;
            this.btn_onoff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_onoff.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_onoff.Location = new System.Drawing.Point(12, 12);
            this.btn_onoff.Name = "btn_onoff";
            this.btn_onoff.Size = new System.Drawing.Size(121, 44);
            this.btn_onoff.TabIndex = 2;
            this.btn_onoff.Text = "Включить";
            this.btn_onoff.UseVisualStyleBackColor = false;
            this.btn_onoff.Click += new System.EventHandler(this.btn_onoff_Click);
            // 
            // gb_partnerManagerPanel
            // 
            this.gb_partnerManagerPanel.Controls.Add(this.lbl_selectedPartner);
            this.gb_partnerManagerPanel.Controls.Add(this.label3);
            this.gb_partnerManagerPanel.Controls.Add(this.nud_editStrategyNumber);
            this.gb_partnerManagerPanel.Controls.Add(this.cb_editAllowTrade);
            this.gb_partnerManagerPanel.Controls.Add(this.lbl_addLogin);
            this.gb_partnerManagerPanel.Controls.Add(this.tb_addLogin);
            this.gb_partnerManagerPanel.Controls.Add(this.btn_removeClientInfo);
            this.gb_partnerManagerPanel.Controls.Add(this.btn_editClientInfo);
            this.gb_partnerManagerPanel.Controls.Add(this.btn_addClientInfo);
            this.gb_partnerManagerPanel.Location = new System.Drawing.Point(12, 62);
            this.gb_partnerManagerPanel.Name = "gb_partnerManagerPanel";
            this.gb_partnerManagerPanel.Size = new System.Drawing.Size(447, 107);
            this.gb_partnerManagerPanel.TabIndex = 3;
            this.gb_partnerManagerPanel.TabStop = false;
            this.gb_partnerManagerPanel.Text = "Управление клиентами";
            // 
            // lbl_selectedPartner
            // 
            this.lbl_selectedPartner.AutoSize = true;
            this.lbl_selectedPartner.Location = new System.Drawing.Point(90, 80);
            this.lbl_selectedPartner.Name = "lbl_selectedPartner";
            this.lbl_selectedPartner.Size = new System.Drawing.Size(122, 13);
            this.lbl_selectedPartner.TabIndex = 14;
            this.lbl_selectedPartner.Text = "Выбранный клиент: `--`";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(91, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Стратегия №";
            // 
            // nud_editStrategyNumber
            // 
            this.nud_editStrategyNumber.Location = new System.Drawing.Point(165, 49);
            this.nud_editStrategyNumber.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nud_editStrategyNumber.Name = "nud_editStrategyNumber";
            this.nud_editStrategyNumber.Size = new System.Drawing.Size(43, 20);
            this.nud_editStrategyNumber.TabIndex = 11;
            // 
            // cb_editAllowTrade
            // 
            this.cb_editAllowTrade.AutoSize = true;
            this.cb_editAllowTrade.Location = new System.Drawing.Point(215, 50);
            this.cb_editAllowTrade.Name = "cb_editAllowTrade";
            this.cb_editAllowTrade.Size = new System.Drawing.Size(133, 17);
            this.cb_editAllowTrade.TabIndex = 10;
            this.cb_editAllowTrade.Text = "Разрешить торговлю";
            this.cb_editAllowTrade.UseVisualStyleBackColor = true;
            // 
            // lbl_addLogin
            // 
            this.lbl_addLogin.AutoSize = true;
            this.lbl_addLogin.Location = new System.Drawing.Point(90, 22);
            this.lbl_addLogin.Name = "lbl_addLogin";
            this.lbl_addLogin.Size = new System.Drawing.Size(41, 13);
            this.lbl_addLogin.TabIndex = 7;
            this.lbl_addLogin.Text = "Логин:";
            // 
            // tb_addLogin
            // 
            this.tb_addLogin.Location = new System.Drawing.Point(135, 19);
            this.tb_addLogin.Name = "tb_addLogin";
            this.tb_addLogin.Size = new System.Drawing.Size(213, 20);
            this.tb_addLogin.TabIndex = 3;
            // 
            // btn_removeClientInfo
            // 
            this.btn_removeClientInfo.Location = new System.Drawing.Point(10, 75);
            this.btn_removeClientInfo.Name = "btn_removeClientInfo";
            this.btn_removeClientInfo.Size = new System.Drawing.Size(75, 23);
            this.btn_removeClientInfo.TabIndex = 2;
            this.btn_removeClientInfo.Text = "Удалить";
            this.btn_removeClientInfo.UseVisualStyleBackColor = true;
            this.btn_removeClientInfo.Click += new System.EventHandler(this.btn_removePartnerInfo_Click);
            // 
            // btn_editClientInfo
            // 
            this.btn_editClientInfo.Location = new System.Drawing.Point(10, 46);
            this.btn_editClientInfo.Name = "btn_editClientInfo";
            this.btn_editClientInfo.Size = new System.Drawing.Size(75, 23);
            this.btn_editClientInfo.TabIndex = 1;
            this.btn_editClientInfo.Text = "Изменить";
            this.btn_editClientInfo.UseVisualStyleBackColor = true;
            this.btn_editClientInfo.Click += new System.EventHandler(this.btn_editPartnerInfo_Click);
            // 
            // btn_addClientInfo
            // 
            this.btn_addClientInfo.Location = new System.Drawing.Point(10, 17);
            this.btn_addClientInfo.Name = "btn_addClientInfo";
            this.btn_addClientInfo.Size = new System.Drawing.Size(75, 23);
            this.btn_addClientInfo.TabIndex = 0;
            this.btn_addClientInfo.Text = "Добавить";
            this.btn_addClientInfo.UseVisualStyleBackColor = true;
            this.btn_addClientInfo.Click += new System.EventHandler(this.btn_addPartnerInfo_Click);
            // 
            // lb_mainLog
            // 
            this.lb_mainLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lb_mainLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_mainLog.FormattingEnabled = true;
            this.lb_mainLog.HorizontalScrollbar = true;
            this.lb_mainLog.ItemHeight = 16;
            this.lb_mainLog.Location = new System.Drawing.Point(10, 389);
            this.lb_mainLog.Name = "lb_mainLog";
            this.lb_mainLog.Size = new System.Drawing.Size(449, 260);
            this.lb_mainLog.TabIndex = 4;
            // 
            // btn_openLogFile
            // 
            this.btn_openLogFile.Location = new System.Drawing.Point(340, 24);
            this.btn_openLogFile.Name = "btn_openLogFile";
            this.btn_openLogFile.Size = new System.Drawing.Size(108, 32);
            this.btn_openLogFile.TabIndex = 15;
            this.btn_openLogFile.Text = "Открыть лог";
            this.btn_openLogFile.UseVisualStyleBackColor = true;
            this.btn_openLogFile.Click += new System.EventHandler(this.btn_openLogFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.dgv_stopOrders);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.dgv_trades);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dgv_orders);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.dgv_derivativePositions);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.dgv_derivativePortfolio);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.dgv_securities);
            this.groupBox1.Location = new System.Drawing.Point(465, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(944, 637);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Информация о клиенте";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 483);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Стоп-заявки";
            // 
            // dgv_stopOrders
            // 
            this.dgv_stopOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_stopOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_stopOrders.Location = new System.Drawing.Point(12, 499);
            this.dgv_stopOrders.Name = "dgv_stopOrders";
            this.dgv_stopOrders.Size = new System.Drawing.Size(929, 132);
            this.dgv_stopOrders.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(569, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Сделки";
            // 
            // dgv_trades
            // 
            this.dgv_trades.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_trades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_trades.Location = new System.Drawing.Point(572, 37);
            this.dgv_trades.Name = "dgv_trades";
            this.dgv_trades.Size = new System.Drawing.Size(353, 111);
            this.dgv_trades.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 159);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Заявки";
            // 
            // dgv_orders
            // 
            this.dgv_orders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_orders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_orders.Location = new System.Drawing.Point(9, 176);
            this.dgv_orders.Name = "dgv_orders";
            this.dgv_orders.Size = new System.Drawing.Size(916, 297);
            this.dgv_orders.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(401, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Позиции по деривативам";
            // 
            // dgv_derivativePositions
            // 
            this.dgv_derivativePositions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_derivativePositions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_derivativePositions.Location = new System.Drawing.Point(401, 37);
            this.dgv_derivativePositions.Name = "dgv_derivativePositions";
            this.dgv_derivativePositions.Size = new System.Drawing.Size(165, 111);
            this.dgv_derivativePositions.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(142, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Портфель по деривативам";
            // 
            // dgv_derivativePortfolio
            // 
            this.dgv_derivativePortfolio.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_derivativePortfolio.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_derivativePortfolio.Location = new System.Drawing.Point(142, 37);
            this.dgv_derivativePortfolio.Name = "dgv_derivativePortfolio";
            this.dgv_derivativePortfolio.Size = new System.Drawing.Size(253, 111);
            this.dgv_derivativePortfolio.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Инструменты";
            // 
            // dgv_securities
            // 
            this.dgv_securities.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_securities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_securities.Location = new System.Drawing.Point(6, 37);
            this.dgv_securities.Name = "dgv_securities";
            this.dgv_securities.Size = new System.Drawing.Size(132, 111);
            this.dgv_securities.TabIndex = 0;
            // 
            // lbl_info
            // 
            this.lbl_info.AutoSize = true;
            this.lbl_info.Location = new System.Drawing.Point(12, 172);
            this.lbl_info.Name = "lbl_info";
            this.lbl_info.Size = new System.Drawing.Size(35, 13);
            this.lbl_info.TabIndex = 17;
            this.lbl_info.Text = "label8";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1421, 673);
            this.Controls.Add(this.lbl_info);
            this.Controls.Add(this.lb_mainLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_openLogFile);
            this.Controls.Add(this.gb_partnerManagerPanel);
            this.Controls.Add(this.btn_onoff);
            this.Controls.Add(this.dgv_partnersInfo);
            this.Name = "MainWindow";
            this.Text = "GUIServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_Closing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_partnersInfo)).EndInit();
            this.gb_partnerManagerPanel.ResumeLayout(false);
            this.gb_partnerManagerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_editStrategyNumber)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_stopOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_trades)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_orders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_derivativePositions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_derivativePortfolio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_securities)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_partnersInfo;
        private System.Windows.Forms.Button btn_onoff;
        private System.Windows.Forms.GroupBox gb_partnerManagerPanel;
        private System.Windows.Forms.Button btn_removeClientInfo;
        private System.Windows.Forms.Button btn_editClientInfo;
        private System.Windows.Forms.Button btn_addClientInfo;
        public System.Windows.Forms.ListBox lb_mainLog;
        private System.Windows.Forms.TextBox tb_addLogin;
        private System.Windows.Forms.Label lbl_addLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_editStrategyNumber;
        private System.Windows.Forms.CheckBox cb_editAllowTrade;
        private System.Windows.Forms.Label lbl_selectedPartner;
        private System.Windows.Forms.Button btn_openLogFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgv_securities;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView dgv_stopOrders;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridView dgv_trades;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgv_orders;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgv_derivativePositions;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgv_derivativePortfolio;
        private System.Windows.Forms.Label lbl_info;
    }
}

