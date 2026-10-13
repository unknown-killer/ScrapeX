namespace ScrapeX
{
    partial class ExportForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleLabel = new System.Windows.Forms.Label();
            this.countryCaption = new System.Windows.Forms.Label();
            this.countries = new System.Windows.Forms.CheckedListBox();
            this.allButton = new System.Windows.Forms.Button();
            this.noneButton = new System.Windows.Forms.Button();
            this.protocolCaption = new System.Windows.Forms.Label();
            this.protocol = new System.Windows.Forms.ComboBox();
            this.splitCountries = new System.Windows.Forms.CheckBox();
            this.includeScheme = new System.Windows.Forms.CheckBox();            this.fullDetail = new System.Windows.Forms.CheckBox();
            this.summary = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Location = new System.Drawing.Point(24, 20);
            this.titleLabel.Size = new System.Drawing.Size(550, 32);
            this.titleLabel.Text = "Export verified proxies";
            this.titleLabel.TabIndex = 0;
            this.Controls.Add(this.titleLabel);
            this.countryCaption.Name = "countryCaption";
            this.countryCaption.Location = new System.Drawing.Point(24, 70);
            this.countryCaption.Size = new System.Drawing.Size(230, 22);
            this.countryCaption.Text = "Countries (multiple selection)";
            this.countryCaption.TabIndex = 1;
            this.Controls.Add(this.countryCaption);
            this.countries.Name = "countries";
            this.countries.Location = new System.Drawing.Point(24, 100);
            this.countries.Size = new System.Drawing.Size(320, 220);
            this.countries.Text = "";
            this.countries.TabIndex = 2;
            this.Controls.Add(this.countries);
            this.countries.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.countries.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.allButton.Name = "allButton";
            this.allButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.allButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(65, 79, 98);
            this.allButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            this.allButton.Location = new System.Drawing.Point(24, 332);
            this.allButton.Size = new System.Drawing.Size(152, 32);
            this.allButton.Text = "Select all";
            this.allButton.TabIndex = 3;
            this.Controls.Add(this.allButton);
            this.allButton.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.allButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.noneButton.Name = "noneButton";
            this.noneButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.noneButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(65, 79, 98);
            this.noneButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            this.noneButton.Location = new System.Drawing.Point(192, 332);
            this.noneButton.Size = new System.Drawing.Size(152, 32);
            this.noneButton.Text = "Clear selection";
            this.noneButton.TabIndex = 4;
            this.Controls.Add(this.noneButton);
            this.noneButton.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.noneButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.protocolCaption.Name = "protocolCaption";
            this.protocolCaption.Location = new System.Drawing.Point(370, 70);
            this.protocolCaption.Size = new System.Drawing.Size(210, 22);
            this.protocolCaption.Text = "Protocol";
            this.protocolCaption.TabIndex = 5;
            this.Controls.Add(this.protocolCaption);
            this.protocol.Name = "protocol";
            this.protocol.Location = new System.Drawing.Point(370, 100);
            this.protocol.Size = new System.Drawing.Size(210, 28);
            this.protocol.Text = "";
            this.protocol.TabIndex = 6;
            this.Controls.Add(this.protocol);
            this.protocol.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.protocol.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.splitCountries.Name = "splitCountries";
            this.splitCountries.Location = new System.Drawing.Point(370, 150);
            this.splitCountries.Size = new System.Drawing.Size(215, 50);
            this.splitCountries.Text = "Separate file per country";
            this.splitCountries.TabIndex = 7;
            this.Controls.Add(this.splitCountries);
            this.includeScheme.Name = "includeScheme";
            this.includeScheme.Location = new System.Drawing.Point(370, 210);
            this.includeScheme.Size = new System.Drawing.Size(215, 50);
            this.includeScheme.Text = "Include protocol:// prefix";
            this.includeScheme.TabIndex = 8;
            this.Controls.Add(this.includeScheme);            this.fullDetail.Name = "fullDetail";            this.fullDetail.Location = new System.Drawing.Point(370, 278);            this.fullDetail.Size = new System.Drawing.Size(215, 60);            this.fullDetail.Text = "Full Detail (country, ping, TLS and test details)";            this.fullDetail.TabIndex = 9;            this.fullDetail.CheckedChanged += new System.EventHandler(this.DetailFormatChanged);            this.Controls.Add(this.fullDetail);
            this.summary.Name = "summary";
            this.summary.Location = new System.Drawing.Point(24, 386);
            this.summary.Size = new System.Drawing.Size(550, 46);
            this.summary.Text = "";
            this.summary.TabIndex = 9;
            this.Controls.Add(this.summary);
            this.saveButton.Name = "saveButton";
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(65, 79, 98);
            this.saveButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            this.saveButton.Location = new System.Drawing.Point(370, 446);
            this.saveButton.Size = new System.Drawing.Size(210, 40);
            this.saveButton.Text = "Save export";
            this.saveButton.TabIndex = 10;
            this.Controls.Add(this.saveButton);
            this.saveButton.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.saveButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(65, 79, 98);
            this.cancelButton.Paint += new System.Windows.Forms.PaintEventHandler(this.DisabledButtonPaint);
            this.cancelButton.Location = new System.Drawing.Point(24, 446);
            this.cancelButton.Size = new System.Drawing.Size(130, 40);
            this.cancelButton.Text = "Cancel";
            this.cancelButton.TabIndex = 11;
            this.Controls.Add(this.cancelButton);
            this.cancelButton.BackColor = System.Drawing.Color.FromArgb(35, 48, 65);
            this.cancelButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
            this.countries.CheckOnClick = true;
            this.countries.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CountryChanged);
            this.protocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocol.Items.AddRange(new object[] { "All protocols", "HTTP", "SOCKS4", "SOCKS5" });
            this.protocol.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
            this.includeScheme.Checked = true;
            this.allButton.Click += new System.EventHandler(this.SelectAll);
            this.noneButton.Click += new System.EventHandler(this.SelectNone);
            this.saveButton.Click += new System.EventHandler(this.SaveExport);
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton = this.cancelButton;
            this.BackColor = System.Drawing.Color.FromArgb(16, 24, 37);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ClientSize = new System.Drawing.Size(608, 512);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SCRAPE X — Export";
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label countryCaption;
        private System.Windows.Forms.CheckedListBox countries;
        private System.Windows.Forms.Button allButton;
        private System.Windows.Forms.Button noneButton;
        private System.Windows.Forms.Label protocolCaption;
        private System.Windows.Forms.ComboBox protocol;
        private System.Windows.Forms.CheckBox splitCountries;
        private System.Windows.Forms.CheckBox includeScheme;        private System.Windows.Forms.CheckBox fullDetail;
        private System.Windows.Forms.Label summary;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
