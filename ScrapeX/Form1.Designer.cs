namespace ScrapeX
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.retryAlternateBox = new System.Windows.Forms.CheckBox();
            this.socks5CountLabel = new System.Windows.Forms.Label();
            this.socks4CountLabel = new System.Windows.Forms.Label();
            this.httpCountLabel = new System.Windows.Forms.Label();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.brandCaption = new System.Windows.Forms.Label();
            this.sessionCaption = new System.Windows.Forms.Label();
            this.elapsedLabel = new System.Windows.Forms.Label();
            this.minimizeButton = new System.Windows.Forms.Button();
            this.maximizeButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.headerRule = new System.Windows.Forms.Panel();
            this.workspacePanel = new System.Windows.Forms.Panel();
            this.scrapeButton = new System.Windows.Forms.Button();
            this.checkButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.outputButton = new System.Windows.Forms.Button();
            this.statsTable = new System.Windows.Forms.TableLayoutPanel();
            this.collectedCard = new System.Windows.Forms.Panel();
            this.collectedCardTitle = new System.Windows.Forms.Label();
            this.collectedLabel = new System.Windows.Forms.Label();
            this.collectedCardHint = new System.Windows.Forms.Label();
            this.checkedCard = new System.Windows.Forms.Panel();
            this.checkedCardTitle = new System.Windows.Forms.Label();
            this.checkedLabel = new System.Windows.Forms.Label();
            this.checkedCardHint = new System.Windows.Forms.Label();
            this.httpsCard = new System.Windows.Forms.Panel();
            this.httpsCardTitle = new System.Windows.Forms.Label();
            this.goodLabel = new System.Windows.Forms.Label();
            this.httpsCardHint = new System.Windows.Forms.Label();
            this.httpOnlyCard = new System.Windows.Forms.Panel();
            this.httpOnlyCardTitle = new System.Windows.Forms.Label();
            this.httpOnlyLabel = new System.Windows.Forms.Label();
            this.httpOnlyCardHint = new System.Windows.Forms.Label();
            this.failedCard = new System.Windows.Forms.Panel();
            this.failedCardTitle = new System.Windows.Forms.Label();
            this.badLabel = new System.Windows.Forms.Label();
            this.failedCardHint = new System.Windows.Forms.Label();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.httpTargetCaption = new System.Windows.Forms.Label();
            this.targetBox = new System.Windows.Forms.TextBox();
            this.httpsTargetCaption = new System.Windows.Forms.Label();
            this.httpsTargetBox = new System.Windows.Forms.TextBox();
            this.workersCaption = new System.Windows.Forms.Label();
            this.timeoutCaption = new System.Windows.Forms.Label();
            this.workers = new System.Windows.Forms.NumericUpDown();
            this.timeoutBox = new System.Windows.Forms.NumericUpDown();
            this.autoCountryBox = new System.Windows.Forms.CheckBox();
            this.probeHint = new System.Windows.Forms.Label();
            this.filterBox = new System.Windows.Forms.ComboBox();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchCaption = new System.Windows.Forms.Label();
            this.exportButton = new System.Windows.Forms.Button();
            this.exportHttpButton = new System.Windows.Forms.Button();
            this.tabPanel = new System.Windows.Forms.Panel();
            this.resultsTabButton = new System.Windows.Forms.Button();
            this.sourcesTabButton = new System.Windows.Forms.Button();
            this.activityTabButton = new System.Windows.Forms.Button();
            this.pagesPanel = new System.Windows.Forms.Panel();
            this.resultPage = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.flagColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.endpointColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.countryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.protocolColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.httpsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.httpLatencyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.httpsLatencyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.detailColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.countryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourcePage = new System.Windows.Forms.Panel();
            this.sourceCaption = new System.Windows.Forms.Label();
            this.sourcesBox = new System.Windows.Forms.TextBox();
            this.saveSourcesButton = new System.Windows.Forms.Button();
            this.restoreSourcesButton = new System.Windows.Forms.Button();
            this.activityPage = new System.Windows.Forms.Panel();
            this.activityBox = new System.Windows.Forms.TextBox();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.statusLabel = new System.Windows.Forms.Label();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).BeginInit();
            this.workspacePanel.SuspendLayout();
            this.statsTable.SuspendLayout();
            this.collectedCard.SuspendLayout();
            this.checkedCard.SuspendLayout();
            this.httpsCard.SuspendLayout();
            this.httpOnlyCard.SuspendLayout();
            this.failedCard.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.workers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).BeginInit();
            this.tabPanel.SuspendLayout();
            this.pagesPanel.SuspendLayout();
            this.resultPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.gridMenu.SuspendLayout();
            this.sourcePage.SuspendLayout();
            this.activityPage.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // retryAlternateBox
            // 
            this.retryAlternateBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.retryAlternateBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(192)))), ((int)(((byte)(210)))));
            this.retryAlternateBox.Location = new System.Drawing.Point(1010, 174);
            this.retryAlternateBox.Name = "retryAlternateBox";
            this.retryAlternateBox.Size = new System.Drawing.Size(390, 28);
            this.retryAlternateBox.TabIndex = 61;
            this.retryAlternateBox.Text = "Retry alternate targets (slower)";
            // 
            // socks5CountLabel
            // 
            this.socks5CountLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.socks5CountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.socks5CountLabel.Location = new System.Drawing.Point(544, 174);
            this.socks5CountLabel.Name = "socks5CountLabel";
            this.socks5CountLabel.Size = new System.Drawing.Size(248, 28);
            this.socks5CountLabel.TabIndex = 60;
            this.socks5CountLabel.Text = "SOCKS5   0 working";
            // 
            // socks4CountLabel
            // 
            this.socks4CountLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.socks4CountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.socks4CountLabel.Location = new System.Drawing.Point(284, 174);
            this.socks4CountLabel.Name = "socks4CountLabel";
            this.socks4CountLabel.Size = new System.Drawing.Size(248, 28);
            this.socks4CountLabel.TabIndex = 59;
            this.socks4CountLabel.Text = "SOCKS4   0 working";
            // 
            // httpCountLabel
            // 
            this.httpCountLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.httpCountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.httpCountLabel.Location = new System.Drawing.Point(24, 174);
            this.httpCountLabel.Name = "httpCountLabel";
            this.httpCountLabel.Size = new System.Drawing.Size(248, 28);
            this.httpCountLabel.TabIndex = 58;
            this.httpCountLabel.Text = "HTTP   0 working";
            // 
            // headerPanel
            // 
            this.headerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.headerPanel.Controls.Add(this.logoBox);
            this.headerPanel.Controls.Add(this.brandCaption);
            this.headerPanel.Controls.Add(this.sessionCaption);
            this.headerPanel.Controls.Add(this.elapsedLabel);
            this.headerPanel.Controls.Add(this.minimizeButton);
            this.headerPanel.Controls.Add(this.maximizeButton);
            this.headerPanel.Controls.Add(this.closeButton);
            this.headerPanel.Controls.Add(this.headerRule);
            this.headerPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.headerPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1440, 100);
            this.headerPanel.TabIndex = 0;
            this.headerPanel.DoubleClick += new System.EventHandler(this.MaximizeClick);
            this.headerPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleMouseDown);
            // 
            // logoBox
            // 
            this.logoBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.logoBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.logoBox.Image = ((System.Drawing.Image)(resources.GetObject("logoBox.Image")));
            this.logoBox.Location = new System.Drawing.Point(26, 18);
            this.logoBox.Name = "logoBox";
            this.logoBox.Size = new System.Drawing.Size(355, 64);
            this.logoBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoBox.TabIndex = 1;
            this.logoBox.TabStop = false;
            this.logoBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleMouseDown);
            // 
            // brandCaption
            // 
            this.brandCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.brandCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.brandCaption.Location = new System.Drawing.Point(401, 37);
            this.brandCaption.Name = "brandCaption";
            this.brandCaption.Size = new System.Drawing.Size(295, 26);
            this.brandCaption.TabIndex = 2;
            this.brandCaption.Text = "PROXY INTELLIGENCE / SECURE ROUTES";
            this.brandCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleMouseDown);
            // 
            // sessionCaption
            // 
            this.sessionCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sessionCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.sessionCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.sessionCaption.Location = new System.Drawing.Point(1020, 24);
            this.sessionCaption.Name = "sessionCaption";
            this.sessionCaption.Size = new System.Drawing.Size(185, 20);
            this.sessionCaption.TabIndex = 3;
            this.sessionCaption.Text = "SESSION TIME";
            // 
            // elapsedLabel
            // 
            this.elapsedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.elapsedLabel.Font = new System.Drawing.Font("Segoe UI", 21F);
            this.elapsedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.elapsedLabel.Location = new System.Drawing.Point(1020, 44);
            this.elapsedLabel.Name = "elapsedLabel";
            this.elapsedLabel.Size = new System.Drawing.Size(190, 36);
            this.elapsedLabel.TabIndex = 4;
            this.elapsedLabel.Text = "00:00:00";
            // 
            // minimizeButton
            // 
            this.minimizeButton.AccessibleName = "Minimize window";
            this.minimizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minimizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.minimizeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.minimizeButton.FlatAppearance.BorderSize = 0;
            this.minimizeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.minimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minimizeButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.minimizeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.minimizeButton.Image = ((System.Drawing.Image)(resources.GetObject("minimizeButton.Image")));
            this.minimizeButton.Location = new System.Drawing.Point(1288, 22);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(42, 36);
            this.minimizeButton.TabIndex = 5;
            this.minimizeButton.UseVisualStyleBackColor = false;
            this.minimizeButton.Click += new System.EventHandler(this.MinimizeClick);
            // 
            // maximizeButton
            // 
            this.maximizeButton.AccessibleName = "Maximize window";
            this.maximizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maximizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.maximizeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.maximizeButton.FlatAppearance.BorderSize = 0;
            this.maximizeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.maximizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.maximizeButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.maximizeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.maximizeButton.Image = ((System.Drawing.Image)(resources.GetObject("maximizeButton.Image")));
            this.maximizeButton.Location = new System.Drawing.Point(1336, 22);
            this.maximizeButton.Name = "maximizeButton";
            this.maximizeButton.Size = new System.Drawing.Size(42, 36);
            this.maximizeButton.TabIndex = 6;
            this.maximizeButton.UseVisualStyleBackColor = false;
            this.maximizeButton.Click += new System.EventHandler(this.MaximizeClick);
            // 
            // closeButton
            // 
            this.closeButton.AccessibleName = "Close window";
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(45)))), ((int)(((byte)(65)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.closeButton.Image = ((System.Drawing.Image)(resources.GetObject("closeButton.Image")));
            this.closeButton.Location = new System.Drawing.Point(1384, 22);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(42, 36);
            this.closeButton.TabIndex = 7;
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.CloseClick);
            // 
            // headerRule
            // 
            this.headerRule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerRule.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(51)))), ((int)(((byte)(72)))));
            this.headerRule.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.headerRule.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.headerRule.Location = new System.Drawing.Point(24, 98);
            this.headerRule.Name = "headerRule";
            this.headerRule.Size = new System.Drawing.Size(1392, 1);
            this.headerRule.TabIndex = 8;
            // 
            // workspacePanel
            // 
            this.workspacePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.workspacePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.workspacePanel.Controls.Add(this.scrapeButton);
            this.workspacePanel.Controls.Add(this.checkButton);
            this.workspacePanel.Controls.Add(this.importButton);
            this.workspacePanel.Controls.Add(this.stopButton);
            this.workspacePanel.Controls.Add(this.clearButton);
            this.workspacePanel.Controls.Add(this.outputButton);
            this.workspacePanel.Controls.Add(this.statsTable);
            this.workspacePanel.Controls.Add(this.settingsPanel);
            this.workspacePanel.Controls.Add(this.filterBox);
            this.workspacePanel.Controls.Add(this.searchBox);
            this.workspacePanel.Controls.Add(this.searchCaption);
            this.workspacePanel.Controls.Add(this.exportButton);
            this.workspacePanel.Controls.Add(this.exportHttpButton);
            this.workspacePanel.Controls.Add(this.tabPanel);
            this.workspacePanel.Controls.Add(this.pagesPanel);
            this.workspacePanel.Controls.Add(this.httpCountLabel);
            this.workspacePanel.Controls.Add(this.socks4CountLabel);
            this.workspacePanel.Controls.Add(this.socks5CountLabel);
            this.workspacePanel.Controls.Add(this.retryAlternateBox);
            this.workspacePanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.workspacePanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.workspacePanel.Location = new System.Drawing.Point(0, 106);
            this.workspacePanel.Name = "workspacePanel";
            this.workspacePanel.Size = new System.Drawing.Size(1440, 842);
            this.workspacePanel.TabIndex = 9;
            // 
            // scrapeButton
            // 
            this.scrapeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.scrapeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.scrapeButton.FlatAppearance.BorderSize = 0;
            this.scrapeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(239)))), ((int)(((byte)(207)))));
            this.scrapeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scrapeButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.scrapeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.scrapeButton.Location = new System.Drawing.Point(24, 0);
            this.scrapeButton.Name = "scrapeButton";
            this.scrapeButton.Size = new System.Drawing.Size(152, 40);
            this.scrapeButton.TabIndex = 10;
            this.scrapeButton.Text = "Collect proxies";
            this.scrapeButton.UseVisualStyleBackColor = false;
            this.scrapeButton.Click += new System.EventHandler(this.ScrapeClick);
            this.scrapeButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // checkButton
            // 
            this.checkButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.checkButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.checkButton.FlatAppearance.BorderSize = 0;
            this.checkButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(239)))), ((int)(((byte)(207)))));
            this.checkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.checkButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.checkButton.Location = new System.Drawing.Point(188, 0);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(160, 40);
            this.checkButton.TabIndex = 11;
            this.checkButton.Text = "Check HTTP + TLS";
            this.checkButton.UseVisualStyleBackColor = false;
            this.checkButton.Click += new System.EventHandler(this.CheckClick);
            this.checkButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // importButton
            // 
            this.importButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.importButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.importButton.FlatAppearance.BorderSize = 0;
            this.importButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.importButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.importButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.importButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.importButton.Location = new System.Drawing.Point(360, 0);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(126, 40);
            this.importButton.TabIndex = 12;
            this.importButton.Text = "Import list";
            this.importButton.UseVisualStyleBackColor = false;
            this.importButton.Click += new System.EventHandler(this.ImportClick);
            this.importButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // stopButton
            // 
            this.stopButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.stopButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.stopButton.FlatAppearance.BorderSize = 0;
            this.stopButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.stopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stopButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.stopButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.stopButton.Location = new System.Drawing.Point(498, 0);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(102, 40);
            this.stopButton.TabIndex = 13;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = false;
            this.stopButton.Click += new System.EventHandler(this.StopClick);
            this.stopButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // clearButton
            // 
            this.clearButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.clearButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.clearButton.FlatAppearance.BorderSize = 0;
            this.clearButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.clearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.clearButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.clearButton.Location = new System.Drawing.Point(612, 0);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(138, 40);
            this.clearButton.TabIndex = 14;
            this.clearButton.Text = "Clear session";
            this.clearButton.UseVisualStyleBackColor = false;
            this.clearButton.Click += new System.EventHandler(this.ClearClick);
            this.clearButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // outputButton
            // 
            this.outputButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.outputButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputButton.FlatAppearance.BorderSize = 0;
            this.outputButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.outputButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.outputButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.outputButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.outputButton.Location = new System.Drawing.Point(762, 0);
            this.outputButton.Name = "outputButton";
            this.outputButton.Size = new System.Drawing.Size(144, 40);
            this.outputButton.TabIndex = 15;
            this.outputButton.Text = "Results folder";
            this.outputButton.UseVisualStyleBackColor = false;
            this.outputButton.Click += new System.EventHandler(this.OpenOutput);
            // 
            // statsTable
            // 
            this.statsTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statsTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.statsTable.ColumnCount = 5;
            this.statsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.statsTable.Controls.Add(this.collectedCard, 0, 0);
            this.statsTable.Controls.Add(this.checkedCard, 1, 0);
            this.statsTable.Controls.Add(this.httpsCard, 2, 0);
            this.statsTable.Controls.Add(this.httpOnlyCard, 3, 0);
            this.statsTable.Controls.Add(this.failedCard, 4, 0);
            this.statsTable.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.statsTable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.statsTable.Location = new System.Drawing.Point(24, 58);
            this.statsTable.Margin = new System.Windows.Forms.Padding(0);
            this.statsTable.Name = "statsTable";
            this.statsTable.RowCount = 1;
            this.statsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.statsTable.Size = new System.Drawing.Size(1392, 108);
            this.statsTable.TabIndex = 16;
            // 
            // collectedCard
            // 
            this.collectedCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.collectedCard.Controls.Add(this.collectedCardTitle);
            this.collectedCard.Controls.Add(this.collectedLabel);
            this.collectedCard.Controls.Add(this.collectedCardHint);
            this.collectedCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.collectedCard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.collectedCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.collectedCard.Location = new System.Drawing.Point(0, 0);
            this.collectedCard.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.collectedCard.Name = "collectedCard";
            this.collectedCard.Size = new System.Drawing.Size(268, 108);
            this.collectedCard.TabIndex = 17;
            // 
            // collectedCardTitle
            // 
            this.collectedCardTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.collectedCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.collectedCardTitle.Location = new System.Drawing.Point(16, 12);
            this.collectedCardTitle.Name = "collectedCardTitle";
            this.collectedCardTitle.Size = new System.Drawing.Size(200, 20);
            this.collectedCardTitle.TabIndex = 18;
            this.collectedCardTitle.Text = "COLLECTED";
            // 
            // collectedLabel
            // 
            this.collectedLabel.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.collectedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.collectedLabel.Location = new System.Drawing.Point(16, 37);
            this.collectedLabel.Name = "collectedLabel";
            this.collectedLabel.Size = new System.Drawing.Size(207, 34);
            this.collectedLabel.TabIndex = 19;
            this.collectedLabel.Text = "0";
            // 
            // collectedCardHint
            // 
            this.collectedCardHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.collectedCardHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.collectedCardHint.Location = new System.Drawing.Point(16, 81);
            this.collectedCardHint.Name = "collectedCardHint";
            this.collectedCardHint.Size = new System.Drawing.Size(205, 18);
            this.collectedCardHint.TabIndex = 20;
            this.collectedCardHint.Text = "Unique endpoints";
            // 
            // checkedCard
            // 
            this.checkedCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.checkedCard.Controls.Add(this.checkedCardTitle);
            this.checkedCard.Controls.Add(this.checkedLabel);
            this.checkedCard.Controls.Add(this.checkedCardHint);
            this.checkedCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedCard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.checkedCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.checkedCard.Location = new System.Drawing.Point(278, 0);
            this.checkedCard.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.checkedCard.Name = "checkedCard";
            this.checkedCard.Size = new System.Drawing.Size(268, 108);
            this.checkedCard.TabIndex = 21;
            // 
            // checkedCardTitle
            // 
            this.checkedCardTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkedCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.checkedCardTitle.Location = new System.Drawing.Point(16, 12);
            this.checkedCardTitle.Name = "checkedCardTitle";
            this.checkedCardTitle.Size = new System.Drawing.Size(200, 20);
            this.checkedCardTitle.TabIndex = 22;
            this.checkedCardTitle.Text = "CHECKED";
            // 
            // checkedLabel
            // 
            this.checkedLabel.Font = new System.Drawing.Font("Segoe UI", 13F);
            this.checkedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.checkedLabel.Location = new System.Drawing.Point(16, 37);
            this.checkedLabel.Name = "checkedLabel";
            this.checkedLabel.Size = new System.Drawing.Size(207, 34);
            this.checkedLabel.TabIndex = 23;
            this.checkedLabel.Text = "0";
            // 
            // checkedCardHint
            // 
            this.checkedCardHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.checkedCardHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.checkedCardHint.Location = new System.Drawing.Point(16, 81);
            this.checkedCardHint.Name = "checkedCardHint";
            this.checkedCardHint.Size = new System.Drawing.Size(205, 18);
            this.checkedCardHint.TabIndex = 24;
            this.checkedCardHint.Text = "Completed / total";
            // 
            // httpsCard
            // 
            this.httpsCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.httpsCard.Controls.Add(this.httpsCardTitle);
            this.httpsCard.Controls.Add(this.goodLabel);
            this.httpsCard.Controls.Add(this.httpsCardHint);
            this.httpsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.httpsCard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.httpsCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.httpsCard.Location = new System.Drawing.Point(556, 0);
            this.httpsCard.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.httpsCard.Name = "httpsCard";
            this.httpsCard.Size = new System.Drawing.Size(268, 108);
            this.httpsCard.TabIndex = 25;
            // 
            // httpsCardTitle
            // 
            this.httpsCardTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.httpsCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.httpsCardTitle.Location = new System.Drawing.Point(16, 12);
            this.httpsCardTitle.Name = "httpsCardTitle";
            this.httpsCardTitle.Size = new System.Drawing.Size(200, 20);
            this.httpsCardTitle.TabIndex = 26;
            this.httpsCardTitle.Text = "HTTPS VERIFIED";
            // 
            // goodLabel
            // 
            this.goodLabel.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.goodLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.goodLabel.Location = new System.Drawing.Point(16, 37);
            this.goodLabel.Name = "goodLabel";
            this.goodLabel.Size = new System.Drawing.Size(207, 34);
            this.goodLabel.TabIndex = 27;
            this.goodLabel.Text = "0";
            // 
            // httpsCardHint
            // 
            this.httpsCardHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.httpsCardHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.httpsCardHint.Location = new System.Drawing.Point(16, 81);
            this.httpsCardHint.Name = "httpsCardHint";
            this.httpsCardHint.Size = new System.Drawing.Size(205, 18);
            this.httpsCardHint.TabIndex = 28;
            this.httpsCardHint.Text = "Certificate + HTTPS passed";
            // 
            // httpOnlyCard
            // 
            this.httpOnlyCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.httpOnlyCard.Controls.Add(this.httpOnlyCardTitle);
            this.httpOnlyCard.Controls.Add(this.httpOnlyLabel);
            this.httpOnlyCard.Controls.Add(this.httpOnlyCardHint);
            this.httpOnlyCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.httpOnlyCard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.httpOnlyCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.httpOnlyCard.Location = new System.Drawing.Point(834, 0);
            this.httpOnlyCard.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.httpOnlyCard.Name = "httpOnlyCard";
            this.httpOnlyCard.Size = new System.Drawing.Size(268, 108);
            this.httpOnlyCard.TabIndex = 29;
            // 
            // httpOnlyCardTitle
            // 
            this.httpOnlyCardTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.httpOnlyCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.httpOnlyCardTitle.Location = new System.Drawing.Point(16, 12);
            this.httpOnlyCardTitle.Name = "httpOnlyCardTitle";
            this.httpOnlyCardTitle.Size = new System.Drawing.Size(200, 20);
            this.httpOnlyCardTitle.TabIndex = 30;
            this.httpOnlyCardTitle.Text = "HTTP ONLY";
            // 
            // httpOnlyLabel
            // 
            this.httpOnlyLabel.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.httpOnlyLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(99)))));
            this.httpOnlyLabel.Location = new System.Drawing.Point(16, 37);
            this.httpOnlyLabel.Name = "httpOnlyLabel";
            this.httpOnlyLabel.Size = new System.Drawing.Size(207, 34);
            this.httpOnlyLabel.TabIndex = 31;
            this.httpOnlyLabel.Text = "0";
            // 
            // httpOnlyCardHint
            // 
            this.httpOnlyCardHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.httpOnlyCardHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.httpOnlyCardHint.Location = new System.Drawing.Point(16, 81);
            this.httpOnlyCardHint.Name = "httpOnlyCardHint";
            this.httpOnlyCardHint.Size = new System.Drawing.Size(205, 18);
            this.httpOnlyCardHint.TabIndex = 32;
            this.httpOnlyCardHint.Text = "HTTPS failed / isolated";
            // 
            // failedCard
            // 
            this.failedCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.failedCard.Controls.Add(this.failedCardTitle);
            this.failedCard.Controls.Add(this.badLabel);
            this.failedCard.Controls.Add(this.failedCardHint);
            this.failedCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.failedCard.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.failedCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.failedCard.Location = new System.Drawing.Point(1112, 0);
            this.failedCard.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.failedCard.Name = "failedCard";
            this.failedCard.Size = new System.Drawing.Size(270, 108);
            this.failedCard.TabIndex = 33;
            // 
            // failedCardTitle
            // 
            this.failedCardTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.failedCardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.failedCardTitle.Location = new System.Drawing.Point(16, 12);
            this.failedCardTitle.Name = "failedCardTitle";
            this.failedCardTitle.Size = new System.Drawing.Size(200, 20);
            this.failedCardTitle.TabIndex = 34;
            this.failedCardTitle.Text = "FAILED";
            // 
            // badLabel
            // 
            this.badLabel.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.badLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(126)))), ((int)(((byte)(147)))));
            this.badLabel.Location = new System.Drawing.Point(16, 37);
            this.badLabel.Name = "badLabel";
            this.badLabel.Size = new System.Drawing.Size(207, 34);
            this.badLabel.TabIndex = 35;
            this.badLabel.Text = "0";
            // 
            // failedCardHint
            // 
            this.failedCardHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.failedCardHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.failedCardHint.Location = new System.Drawing.Point(16, 81);
            this.failedCardHint.Name = "failedCardHint";
            this.failedCardHint.Size = new System.Drawing.Size(205, 18);
            this.failedCardHint.TabIndex = 36;
            this.failedCardHint.Text = "Neither probe passed";
            // 
            // settingsPanel
            // 
            this.settingsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.settingsPanel.Controls.Add(this.httpTargetCaption);
            this.settingsPanel.Controls.Add(this.targetBox);
            this.settingsPanel.Controls.Add(this.httpsTargetCaption);
            this.settingsPanel.Controls.Add(this.httpsTargetBox);
            this.settingsPanel.Controls.Add(this.workersCaption);
            this.settingsPanel.Controls.Add(this.timeoutCaption);
            this.settingsPanel.Controls.Add(this.workers);
            this.settingsPanel.Controls.Add(this.timeoutBox);
            this.settingsPanel.Controls.Add(this.autoCountryBox);
            this.settingsPanel.Controls.Add(this.probeHint);
            this.settingsPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.settingsPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.settingsPanel.Location = new System.Drawing.Point(24, 216);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(1392, 128);
            this.settingsPanel.TabIndex = 37;
            // 
            // httpTargetCaption
            // 
            this.httpTargetCaption.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.httpTargetCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.httpTargetCaption.Location = new System.Drawing.Point(0, 0);
            this.httpTargetCaption.Name = "httpTargetCaption";
            this.httpTargetCaption.Size = new System.Drawing.Size(230, 22);
            this.httpTargetCaption.TabIndex = 38;
            this.httpTargetCaption.Text = "HTTP test endpoint";
            // 
            // targetBox
            // 
            this.targetBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.targetBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.targetBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.targetBox.Location = new System.Drawing.Point(0, 25);
            this.targetBox.Name = "targetBox";
            this.targetBox.Size = new System.Drawing.Size(928, 25);
            this.targetBox.TabIndex = 39;
            this.targetBox.Text = "http://www.google.com/generate_204";
            // 
            // httpsTargetCaption
            // 
            this.httpsTargetCaption.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.httpsTargetCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.httpsTargetCaption.Location = new System.Drawing.Point(0, 62);
            this.httpsTargetCaption.Name = "httpsTargetCaption";
            this.httpsTargetCaption.Size = new System.Drawing.Size(420, 22);
            this.httpsTargetCaption.TabIndex = 40;
            this.httpsTargetCaption.Text = "HTTPS / TLS test endpoint — always checked";
            // 
            // httpsTargetBox
            // 
            this.httpsTargetBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.httpsTargetBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.httpsTargetBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.httpsTargetBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.httpsTargetBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.httpsTargetBox.Location = new System.Drawing.Point(0, 87);
            this.httpsTargetBox.Name = "httpsTargetBox";
            this.httpsTargetBox.Size = new System.Drawing.Size(928, 25);
            this.httpsTargetBox.TabIndex = 41;
            this.httpsTargetBox.Text = "https://www.google.com/generate_204";
            // 
            // workersCaption
            // 
            this.workersCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.workersCaption.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.workersCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.workersCaption.Location = new System.Drawing.Point(952, 0);
            this.workersCaption.Name = "workersCaption";
            this.workersCaption.Size = new System.Drawing.Size(140, 22);
            this.workersCaption.TabIndex = 42;
            this.workersCaption.Text = "Parallel proxies";
            // 
            // timeoutCaption
            // 
            this.timeoutCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.timeoutCaption.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.timeoutCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.timeoutCaption.Location = new System.Drawing.Point(1136, 0);
            this.timeoutCaption.Name = "timeoutCaption";
            this.timeoutCaption.Size = new System.Drawing.Size(245, 22);
            this.timeoutCaption.TabIndex = 43;
            this.timeoutCaption.Text = "Timeout / stage (ms)";
            // 
            // workers
            // 
            this.workers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.workers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.workers.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.workers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.workers.Location = new System.Drawing.Point(952, 25);
            this.workers.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.workers.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.workers.Name = "workers";
            this.workers.Size = new System.Drawing.Size(164, 25);
            this.workers.TabIndex = 44;
            this.workers.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // timeoutBox
            // 
            this.timeoutBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.timeoutBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.timeoutBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.timeoutBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.timeoutBox.Increment = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.timeoutBox.Location = new System.Drawing.Point(1136, 25);
            this.timeoutBox.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.timeoutBox.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.timeoutBox.Name = "timeoutBox";
            this.timeoutBox.Size = new System.Drawing.Size(245, 25);
            this.timeoutBox.TabIndex = 45;
            this.timeoutBox.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // autoCountryBox
            // 
            this.autoCountryBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.autoCountryBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.autoCountryBox.Checked = true;
            this.autoCountryBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoCountryBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.autoCountryBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.autoCountryBox.Location = new System.Drawing.Point(952, 70);
            this.autoCountryBox.Name = "autoCountryBox";
            this.autoCountryBox.Size = new System.Drawing.Size(430, 24);
            this.autoCountryBox.TabIndex = 46;
            this.autoCountryBox.Text = "Resolve country in background";
            this.autoCountryBox.UseVisualStyleBackColor = false;
            // 
            // probeHint
            // 
            this.probeHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.probeHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.probeHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.probeHint.Location = new System.Drawing.Point(952, 98);
            this.probeHint.Name = "probeHint";
            this.probeHint.Size = new System.Drawing.Size(430, 20);
            this.probeHint.TabIndex = 47;
            this.probeHint.Text = "Shared socket budget • automatic capacity recovery";
            // 
            // filterBox
            // 
            this.filterBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.filterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.filterBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.filterBox.Items.AddRange(new object[] {
            "Working Proxy",
            "HTTPS verified",
            "HTTP only",
            "All",
            "Failed",
            "Http",
            "Socks4",
            "Socks5"});
            this.filterBox.Location = new System.Drawing.Point(24, 362);
            this.filterBox.Name = "filterBox";
            this.filterBox.Size = new System.Drawing.Size(188, 25);
            this.filterBox.TabIndex = 48;
            this.filterBox.SelectedIndexChanged += new System.EventHandler(this.ApplyFilter);
            // 
            // searchBox
            // 
            this.searchBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.searchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.searchBox.Location = new System.Drawing.Point(286, 362);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(240, 25);
            this.searchBox.TabIndex = 49;
            this.searchBox.TextChanged += new System.EventHandler(this.ApplyFilter);
            // 
            // searchCaption
            // 
            this.searchCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.searchCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.searchCaption.Location = new System.Drawing.Point(231, 365);
            this.searchCaption.Name = "searchCaption";
            this.searchCaption.Size = new System.Drawing.Size(52, 24);
            this.searchCaption.TabIndex = 50;
            this.searchCaption.Text = "Find IP";
            // 
            // exportButton
            // 
            this.exportButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.exportButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.exportButton.FlatAppearance.BorderSize = 0;
            this.exportButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.exportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.exportButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.exportButton.Location = new System.Drawing.Point(550, 358);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(163, 36);
            this.exportButton.TabIndex = 51;
            this.exportButton.Text = "Export HTTPS";
            this.exportButton.UseVisualStyleBackColor = false;
            this.exportButton.Click += new System.EventHandler(this.ExportClick);
            // 
            // exportHttpButton
            // 
            this.exportHttpButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.exportHttpButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.exportHttpButton.FlatAppearance.BorderSize = 0;
            this.exportHttpButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.exportHttpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportHttpButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.exportHttpButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.exportHttpButton.Location = new System.Drawing.Point(725, 358);
            this.exportHttpButton.Name = "exportHttpButton";
            this.exportHttpButton.Size = new System.Drawing.Size(180, 36);
            this.exportHttpButton.TabIndex = 52;
            this.exportHttpButton.Text = "Export HTTP only";
            this.exportHttpButton.UseVisualStyleBackColor = false;
            this.exportHttpButton.Click += new System.EventHandler(this.ExportHttpClick);
            // 
            // tabPanel
            // 
            this.tabPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.tabPanel.Controls.Add(this.resultsTabButton);
            this.tabPanel.Controls.Add(this.sourcesTabButton);
            this.tabPanel.Controls.Add(this.activityTabButton);
            this.tabPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.tabPanel.Location = new System.Drawing.Point(24, 407);
            this.tabPanel.Name = "tabPanel";
            this.tabPanel.Size = new System.Drawing.Size(1392, 38);
            this.tabPanel.TabIndex = 53;
            // 
            // resultsTabButton
            // 
            this.resultsTabButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.resultsTabButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.resultsTabButton.FlatAppearance.BorderSize = 0;
            this.resultsTabButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.resultsTabButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resultsTabButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.resultsTabButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.resultsTabButton.Location = new System.Drawing.Point(0, 0);
            this.resultsTabButton.Name = "resultsTabButton";
            this.resultsTabButton.Size = new System.Drawing.Size(136, 34);
            this.resultsTabButton.TabIndex = 54;
            this.resultsTabButton.Text = "Proxy results";
            this.resultsTabButton.UseVisualStyleBackColor = false;
            this.resultsTabButton.Click += new System.EventHandler(this.ResultsTabClick);
            // 
            // sourcesTabButton
            // 
            this.sourcesTabButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.sourcesTabButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.sourcesTabButton.FlatAppearance.BorderSize = 0;
            this.sourcesTabButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.sourcesTabButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sourcesTabButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.sourcesTabButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.sourcesTabButton.Location = new System.Drawing.Point(146, 0);
            this.sourcesTabButton.Name = "sourcesTabButton";
            this.sourcesTabButton.Size = new System.Drawing.Size(136, 34);
            this.sourcesTabButton.TabIndex = 55;
            this.sourcesTabButton.Text = "Sources";
            this.sourcesTabButton.UseVisualStyleBackColor = false;
            this.sourcesTabButton.Click += new System.EventHandler(this.SourcesTabClick);
            // 
            // activityTabButton
            // 
            this.activityTabButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.activityTabButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.activityTabButton.FlatAppearance.BorderSize = 0;
            this.activityTabButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.activityTabButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.activityTabButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.activityTabButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.activityTabButton.Location = new System.Drawing.Point(292, 0);
            this.activityTabButton.Name = "activityTabButton";
            this.activityTabButton.Size = new System.Drawing.Size(136, 34);
            this.activityTabButton.TabIndex = 56;
            this.activityTabButton.Text = "Activity";
            this.activityTabButton.UseVisualStyleBackColor = false;
            this.activityTabButton.Click += new System.EventHandler(this.ActivityTabClick);
            // 
            // pagesPanel
            // 
            this.pagesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pagesPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.pagesPanel.Controls.Add(this.resultPage);
            this.pagesPanel.Controls.Add(this.sourcePage);
            this.pagesPanel.Controls.Add(this.activityPage);
            this.pagesPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pagesPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.pagesPanel.Location = new System.Drawing.Point(24, 452);
            this.pagesPanel.Name = "pagesPanel";
            this.pagesPanel.Size = new System.Drawing.Size(1392, 382);
            this.pagesPanel.TabIndex = 57;
            // 
            // resultPage
            // 
            this.resultPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.resultPage.Controls.Add(this.grid);
            this.resultPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultPage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.resultPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.resultPage.Location = new System.Drawing.Point(0, 0);
            this.resultPage.Name = "resultPage";
            this.resultPage.Size = new System.Drawing.Size(1392, 382);
            this.resultPage.TabIndex = 58;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(41)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(41)))), ((int)(((byte)(60)))));
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grid.ColumnHeadersHeight = 40;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.flagColumn,
            this.endpointColumn,
            this.countryColumn,
            this.protocolColumn,
            this.httpsColumn,
            this.httpLatencyColumn,
            this.httpsLatencyColumn,
            this.detailColumn});
            this.grid.ContextMenuStrip = this.gridMenu;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(69)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grid.DefaultCellStyle = dataGridViewCellStyle2;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(48)))), ((int)(((byte)(66)))));
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 34;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(1392, 382);
            this.grid.TabIndex = 61;
            this.grid.VirtualMode = true;
            this.grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridFormatting);
            this.grid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridMouseDown);
            this.grid.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.CountryToolTip);
            this.grid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.GridValue);
            this.grid.Paint += new System.Windows.Forms.PaintEventHandler(this.GridPaint);
            // 
            // flagColumn
            // 
            this.flagColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.flagColumn.FillWeight = 26F;
            this.flagColumn.HeaderText = "";
            this.flagColumn.MinimumWidth = 36;
            this.flagColumn.Name = "flagColumn";
            this.flagColumn.ReadOnly = true;
            this.flagColumn.Width = 36;
            // 
            // endpointColumn
            // 
            this.endpointColumn.FillWeight = 140F;
            this.endpointColumn.HeaderText = "ENDPOINT";
            this.endpointColumn.Name = "endpointColumn";
            this.endpointColumn.ReadOnly = true;
            this.endpointColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // countryColumn
            // 
            this.countryColumn.FillWeight = 90F;
            this.countryColumn.HeaderText = "COUNTRY";
            this.countryColumn.Name = "countryColumn";
            this.countryColumn.ReadOnly = true;
            this.countryColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // protocolColumn
            // 
            this.protocolColumn.FillWeight = 72F;
            this.protocolColumn.HeaderText = "PROTOCOL";
            this.protocolColumn.Name = "protocolColumn";
            this.protocolColumn.ReadOnly = true;
            this.protocolColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // httpsColumn
            // 
            this.httpsColumn.FillWeight = 145F;
            this.httpsColumn.HeaderText = "HTTPS / SSL";
            this.httpsColumn.Name = "httpsColumn";
            this.httpsColumn.ReadOnly = true;
            this.httpsColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // httpLatencyColumn
            // 
            this.httpLatencyColumn.FillWeight = 65F;
            this.httpLatencyColumn.HeaderText = "Ping (TCP)";
            this.httpLatencyColumn.Name = "httpLatencyColumn";
            this.httpLatencyColumn.ReadOnly = true;
            this.httpLatencyColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // httpsLatencyColumn
            // 
            this.httpsLatencyColumn.FillWeight = 65F;
            this.httpsLatencyColumn.HeaderText = "TLS ms";
            this.httpsLatencyColumn.Name = "httpsLatencyColumn";
            this.httpsLatencyColumn.ReadOnly = true;
            this.httpsLatencyColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // detailColumn
            // 
            this.detailColumn.FillWeight = 190F;
            this.detailColumn.HeaderText = "PROBE DETAILS";
            this.detailColumn.Name = "detailColumn";
            this.detailColumn.ReadOnly = true;
            this.detailColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // gridMenu
            // 
            this.gridMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.countryMenuItem});
            this.gridMenu.Name = "gridMenu";
            this.gridMenu.Size = new System.Drawing.Size(213, 26);
            // 
            // countryMenuItem
            // 
            this.countryMenuItem.Name = "countryMenuItem";
            this.countryMenuItem.Size = new System.Drawing.Size(212, 22);
            this.countryMenuItem.Text = "Look up endpoint country";
            this.countryMenuItem.Click += new System.EventHandler(this.LookupCountry);
            // 
            // sourcePage
            // 
            this.sourcePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.sourcePage.Controls.Add(this.sourceCaption);
            this.sourcePage.Controls.Add(this.sourcesBox);
            this.sourcePage.Controls.Add(this.saveSourcesButton);
            this.sourcePage.Controls.Add(this.restoreSourcesButton);
            this.sourcePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcePage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.sourcePage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.sourcePage.Location = new System.Drawing.Point(0, 0);
            this.sourcePage.Name = "sourcePage";
            this.sourcePage.Size = new System.Drawing.Size(1392, 382);
            this.sourcePage.TabIndex = 59;
            this.sourcePage.Visible = false;
            // 
            // sourceCaption
            // 
            this.sourceCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.sourceCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(158)))), ((int)(((byte)(184)))));
            this.sourceCaption.Location = new System.Drawing.Point(14, 8);
            this.sourceCaption.Name = "sourceCaption";
            this.sourceCaption.Size = new System.Drawing.Size(1080, 24);
            this.sourceCaption.TabIndex = 62;
            this.sourceCaption.Text = "One source URL per line. Save to keep changes. # starts a comment.";
            // 
            // sourcesBox
            // 
            this.sourcesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourcesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.sourcesBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sourcesBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.sourcesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.sourcesBox.Location = new System.Drawing.Point(14, 38);
            this.sourcesBox.MaxLength = 262144;
            this.sourcesBox.Multiline = true;
            this.sourcesBox.Name = "sourcesBox";
            this.sourcesBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sourcesBox.Size = new System.Drawing.Size(1364, 288);
            this.sourcesBox.TabIndex = 63;
            this.sourcesBox.WordWrap = false;
            // 
            // saveSourcesButton
            // 
            this.saveSourcesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.saveSourcesButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.saveSourcesButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveSourcesButton.FlatAppearance.BorderSize = 0;
            this.saveSourcesButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(85)))), ((int)(((byte)(91)))));
            this.saveSourcesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveSourcesButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveSourcesButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.saveSourcesButton.Location = new System.Drawing.Point(14, 338);
            this.saveSourcesButton.Name = "saveSourcesButton";
            this.saveSourcesButton.Size = new System.Drawing.Size(150, 34);
            this.saveSourcesButton.TabIndex = 64;
            this.saveSourcesButton.Text = "Save sources";
            this.saveSourcesButton.UseVisualStyleBackColor = false;
            this.saveSourcesButton.Click += new System.EventHandler(this.SaveSources);
            this.saveSourcesButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // restoreSourcesButton
            // 
            this.restoreSourcesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.restoreSourcesButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.restoreSourcesButton.FlatAppearance.BorderSize = 0;
            this.restoreSourcesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.restoreSourcesButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.restoreSourcesButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.restoreSourcesButton.Location = new System.Drawing.Point(178, 338);
            this.restoreSourcesButton.Name = "restoreSourcesButton";
            this.restoreSourcesButton.Size = new System.Drawing.Size(190, 34);
            this.restoreSourcesButton.TabIndex = 65;
            this.restoreSourcesButton.Text = "Restore bundled sources";
            this.restoreSourcesButton.UseVisualStyleBackColor = false;
            this.restoreSourcesButton.Click += new System.EventHandler(this.RestoreSources);
            this.restoreSourcesButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            // 
            // activityPage
            // 
            this.activityPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.activityPage.Controls.Add(this.activityBox);
            this.activityPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.activityPage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.activityPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.activityPage.Location = new System.Drawing.Point(0, 0);
            this.activityPage.Name = "activityPage";
            this.activityPage.Size = new System.Drawing.Size(1392, 382);
            this.activityPage.TabIndex = 60;
            this.activityPage.Visible = false;
            // 
            // activityBox
            // 
            this.activityBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(29)))), ((int)(((byte)(44)))));
            this.activityBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.activityBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.activityBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.activityBox.Location = new System.Drawing.Point(10, 10);
            this.activityBox.Multiline = true;
            this.activityBox.Name = "activityBox";
            this.activityBox.ReadOnly = true;
            this.activityBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.activityBox.Size = new System.Drawing.Size(1372, 362);
            this.activityBox.TabIndex = 65;
            this.activityBox.WordWrap = false;
            // 
            // footerPanel
            // 
            this.footerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.footerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.footerPanel.Controls.Add(this.statusLabel);
            this.footerPanel.Controls.Add(this.progress);
            this.footerPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.footerPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.footerPanel.Location = new System.Drawing.Point(0, 954);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(1440, 46);
            this.footerPanel.TabIndex = 66;
            // 
            // statusLabel
            // 
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(227)))), ((int)(((byte)(192)))));
            this.statusLabel.Location = new System.Drawing.Point(24, 11);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(560, 24);
            this.statusLabel.TabIndex = 67;
            this.statusLabel.Text = "Ready — HTTPS results are saved separately";
            // 
            // progress
            // 
            this.progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progress.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.progress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.progress.Location = new System.Drawing.Point(650, 15);
            this.progress.Maximum = 1000;
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(766, 12);
            this.progress.TabIndex = 68;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(18)))), ((int)(((byte)(29)))));
            this.ClientSize = new System.Drawing.Size(1440, 1000);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.workspacePanel);
            this.Controls.Add(this.footerPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1120, 780);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scrape X";
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logoBox)).EndInit();
            this.workspacePanel.ResumeLayout(false);
            this.workspacePanel.PerformLayout();
            this.statsTable.ResumeLayout(false);
            this.collectedCard.ResumeLayout(false);
            this.checkedCard.ResumeLayout(false);
            this.httpsCard.ResumeLayout(false);
            this.httpOnlyCard.ResumeLayout(false);
            this.failedCard.ResumeLayout(false);
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.workers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).EndInit();
            this.tabPanel.ResumeLayout(false);
            this.pagesPanel.ResumeLayout(false);
            this.resultPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.gridMenu.ResumeLayout(false);
            this.sourcePage.ResumeLayout(false);
            this.sourcePage.PerformLayout();
            this.activityPage.ResumeLayout(false);
            this.activityPage.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.PictureBox logoBox;
        private System.Windows.Forms.Label brandCaption;
        private System.Windows.Forms.Label sessionCaption;
        private System.Windows.Forms.Label elapsedLabel;
        private System.Windows.Forms.Button minimizeButton;
        private System.Windows.Forms.Button maximizeButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel headerRule;
        private System.Windows.Forms.Panel workspacePanel;
        private System.Windows.Forms.Button scrapeButton;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button outputButton;
        private System.Windows.Forms.Label httpCountLabel;
        private System.Windows.Forms.Label socks4CountLabel;
        private System.Windows.Forms.Label socks5CountLabel;
        private System.Windows.Forms.CheckBox retryAlternateBox;
        private System.Windows.Forms.TableLayoutPanel statsTable;
        private System.Windows.Forms.Panel collectedCard;
        private System.Windows.Forms.Label collectedCardTitle;
        private System.Windows.Forms.Label collectedLabel;
        private System.Windows.Forms.Label collectedCardHint;
        private System.Windows.Forms.Panel checkedCard;
        private System.Windows.Forms.Label checkedCardTitle;
        private System.Windows.Forms.Label checkedLabel;
        private System.Windows.Forms.Label checkedCardHint;
        private System.Windows.Forms.Panel httpsCard;
        private System.Windows.Forms.Label httpsCardTitle;
        private System.Windows.Forms.Label goodLabel;
        private System.Windows.Forms.Label httpsCardHint;
        private System.Windows.Forms.Panel httpOnlyCard;
        private System.Windows.Forms.Label httpOnlyCardTitle;
        private System.Windows.Forms.Label httpOnlyLabel;
        private System.Windows.Forms.Label httpOnlyCardHint;
        private System.Windows.Forms.Panel failedCard;
        private System.Windows.Forms.Label failedCardTitle;
        private System.Windows.Forms.Label badLabel;
        private System.Windows.Forms.Label failedCardHint;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.Label httpTargetCaption;
        private System.Windows.Forms.TextBox targetBox;
        private System.Windows.Forms.Label httpsTargetCaption;
        private System.Windows.Forms.TextBox httpsTargetBox;
        private System.Windows.Forms.Label workersCaption;
        private System.Windows.Forms.Label timeoutCaption;
        private System.Windows.Forms.NumericUpDown workers;
        private System.Windows.Forms.NumericUpDown timeoutBox;
        private System.Windows.Forms.CheckBox autoCountryBox;
        private System.Windows.Forms.Label probeHint;
        private System.Windows.Forms.ComboBox filterBox;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label searchCaption;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button exportHttpButton;
        private System.Windows.Forms.Panel tabPanel;
        private System.Windows.Forms.Button resultsTabButton;
        private System.Windows.Forms.Button sourcesTabButton;
        private System.Windows.Forms.Button activityTabButton;
        private System.Windows.Forms.Panel pagesPanel;
        private System.Windows.Forms.Panel resultPage;
        private System.Windows.Forms.Panel sourcePage;
        private System.Windows.Forms.Panel activityPage;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewImageColumn flagColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn endpointColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn countryColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn protocolColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn httpsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn httpLatencyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn httpsLatencyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn detailColumn;
        private System.Windows.Forms.Label sourceCaption;
        private System.Windows.Forms.TextBox sourcesBox;
        private System.Windows.Forms.Button saveSourcesButton;
        private System.Windows.Forms.Button restoreSourcesButton;
        private System.Windows.Forms.TextBox activityBox;
        private System.Windows.Forms.Panel footerPanel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.ContextMenuStrip gridMenu;
        private System.Windows.Forms.ToolStripMenuItem countryMenuItem;
    }
}
