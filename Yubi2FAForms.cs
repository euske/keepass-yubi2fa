// Yubi2FAForms.cs

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Yubi2FA {

public class Yubi2FAInputForm : Form {

    private TextBox otpBox;
    private Button resetButton;
    private Button okButton;
    private Button cancelButton;
    private GroupBox groupBox;
    private TableLayoutPanel tableLayoutPanel;

    public Yubi2FAInputForm() {
        this.otpBox = new TextBox();
        this.resetButton = new Button();
        this.okButton = new Button();
        this.cancelButton = new Button();
        this.groupBox = new GroupBox();
        this.tableLayoutPanel = new TableLayoutPanel();

        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(6F, 12F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.AutoSize = true;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.ClientSize = new Size(240, 90);
        this.Padding = new Padding(3);
        this.Text = "Yubi2FA";
        this.AcceptButton = this.okButton;

        this.otpBox.Dock = DockStyle.Fill;
        this.otpBox.UseSystemPasswordChar = true;
        this.otpBox.TabIndex = 1;
        this.otpBox.TextChanged += otpBox_TextChanged;

        this.resetButton.Dock = DockStyle.Fill;
        this.resetButton.Text = "&Reset...";
        this.resetButton.UseVisualStyleBackColor = true;
        this.resetButton.TabIndex = 2;
        this.resetButton.Click += resetButton_Click;

        this.okButton.Dock = DockStyle.Fill;
        this.okButton.Text = "&OK";
        this.okButton.Enabled = false;
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.DialogResult = DialogResult.OK;
        this.okButton.TabIndex = 3;

        this.cancelButton.Dock = DockStyle.Fill;
        this.cancelButton.Text = "&Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.DialogResult = DialogResult.Cancel;
        this.cancelButton.TabIndex = 4;

        this.groupBox.Dock = DockStyle.Top;
        this.groupBox.Padding = new Padding(8);
        this.groupBox.Size = new Size(240, 52);
        this.groupBox.TabStop = false;
        this.groupBox.Text = "YubiKey OTP";
        this.groupBox.Controls.Add(this.otpBox);
        
        this.tableLayoutPanel.Dock = DockStyle.Bottom;
        this.tableLayoutPanel.Size = new Size(240, 30);
        this.tableLayoutPanel.RowCount = 1;
        this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this.tableLayoutPanel.ColumnCount = 4;
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel.Controls.Add(this.resetButton, 0, 0);
        this.tableLayoutPanel.Controls.Add(this.okButton, 2, 0);
        this.tableLayoutPanel.Controls.Add(this.cancelButton, 3, 0);
        
        this.Controls.Add(this.groupBox);
        this.Controls.Add(this.tableLayoutPanel);
        this.ResumeLayout(false);
    }

    public string OTP {
        get { return this.otpBox.Text; }
    }

    private string _configLine;
    public string ConfigLine {
        get { return _configLine; }
    }

    private void otpBox_TextChanged(object sender, EventArgs args) {
        if (sender is TextBox) {
            try {
                YubiKeyOTPItem.CheckOTP((sender as TextBox).Text);
                this.okButton.Enabled = true;
            } catch (FormatException ) {
                this.okButton.Enabled = false;
            }
        }
    }

    private void resetButton_Click(object sender, EventArgs args) {
        using (Yubi2FAConfigForm form = new Yubi2FAConfigForm()) {
            if (form.ShowDialog() == DialogResult.OK &&
                form.ConfigLine != null) {
                _configLine = form.ConfigLine;
                Close();
            }
        }
    }
}

public class Yubi2FAConfigForm : Form {

    private TextBox configBox;
    private Button importButton;
    private Button helpButton;
    private Button okButton;
    private Button cancelButton;
    private GroupBox groupBox;
    private TableLayoutPanel tableLayoutPanel1;
    private TableLayoutPanel tableLayoutPanel2;

    public Yubi2FAConfigForm() {
        this.configBox = new TextBox();
        this.importButton = new Button();
        this.helpButton = new Button();
        this.okButton = new Button();
        this.cancelButton = new Button();
        this.groupBox = new GroupBox();
        this.tableLayoutPanel1 = new TableLayoutPanel();
        this.tableLayoutPanel2 = new TableLayoutPanel();
        
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(6F, 12F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.AutoSize = true;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.ClientSize = new Size(320, 180);
        this.Padding = new Padding(3);
        this.Text = "Yubi2FA Configuration";
        this.AcceptButton = this.okButton;

        this.configBox.Dock = DockStyle.Fill;
        this.configBox.UseSystemPasswordChar = true;
        this.configBox.Multiline = true;
        this.configBox.TabIndex = 1;
        this.configBox.TextChanged += configBox_TextChanged;
        
        this.importButton.Dock = DockStyle.Right;
        this.importButton.Size = new Size(75, 34);
        this.importButton.Text = "Import...";
        this.importButton.UseVisualStyleBackColor = true;
        this.importButton.TabIndex = 2;
        this.importButton.Click += importButton_Click;

        this.helpButton.Dock = DockStyle.Fill;
        this.helpButton.Text = "&Help";
        this.helpButton.UseVisualStyleBackColor = true;
        this.helpButton.TabIndex = 3;

        this.okButton.Dock = DockStyle.Fill;
        this.okButton.Text = "&OK";
        this.okButton.Enabled = false;
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.DialogResult = DialogResult.OK;
        this.okButton.TabIndex = 4;

        this.cancelButton.Dock = DockStyle.Fill;
        this.cancelButton.Text = "&Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.DialogResult = DialogResult.Cancel;
        this.cancelButton.TabIndex = 5;
        
        this.tableLayoutPanel2.Dock = DockStyle.Fill;
        this.tableLayoutPanel2.ColumnCount = 1;
        this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        this.tableLayoutPanel2.Location = new Point(8, 20);
        this.tableLayoutPanel2.RowCount = 2;
        this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        this.tableLayoutPanel2.Controls.Add(this.configBox, 0, 0);
        this.tableLayoutPanel2.Controls.Add(this.importButton, 0, 1);

        this.groupBox.Dock = DockStyle.Top;
        this.groupBox.Padding = new Padding(8);
        this.groupBox.Size = new Size(320, 128);
        this.groupBox.TabStop = false;
        this.groupBox.Text = "YubiKey configuration_log.csv line";
        this.groupBox.Controls.Add(this.tableLayoutPanel2);
        
        this.tableLayoutPanel1.Dock = DockStyle.Bottom;
        this.tableLayoutPanel1.Size = new Size(320, 30);
        this.tableLayoutPanel1.RowCount = 1;
        this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this.tableLayoutPanel1.ColumnCount = 4;
        this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
        this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        this.tableLayoutPanel1.Controls.Add(this.helpButton, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.okButton, 2, 0);
        this.tableLayoutPanel1.Controls.Add(this.cancelButton, 3, 0);
        
        this.Controls.Add(this.groupBox);
        this.Controls.Add(this.tableLayoutPanel1);
        this.ResumeLayout(false);
    }

    public string ConfigLine {
        get { return this.configBox.Text; }
    }

    private void configBox_TextChanged(object sender, EventArgs args) {
        if (sender is TextBox) {
            try {
                YubiKeyOTPItem.FromLog((sender as TextBox).Text);
                this.okButton.Enabled = true;
            } catch (FormatException ) {
                this.okButton.Enabled = false;
            }
        }
    }

    private void importButton_Click(object sender, EventArgs args) {
        using (OpenFileDialog dialog = new OpenFileDialog()) {
            dialog.Title = "Select configuration_log.csv File";
            dialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.CheckFileExists = true;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK) {
                try {
                    using (Stream stream = dialog.OpenFile()) {
                        using (StreamReader fp = new StreamReader(stream)) {
                            string line = getOTPLastLogLine(fp);
                            if (line != null) {
                                this.configBox.Text = line;
                            }
                        }
                    }
                } catch (IOException ) {
                    // Could not read the log file. - ignored
                }
            }
        }
    }

    private string getOTPLastLogLine(StreamReader fp) {
        string goodLine = null;
        while (true) {
            string line = fp.ReadLine();
            if (line == null) break;
            try {
                YubiKeyOTPItem.FromLog(line);
                goodLine = line;
            } catch (FormatException ) {
            }
        }
        return goodLine;
    }
}

} // Yubi2FA
