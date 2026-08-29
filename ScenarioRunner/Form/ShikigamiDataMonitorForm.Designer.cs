namespace ScenarioRunner.Form
{
	partial class ShikigamiDataMonitorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.rtbContent = new System.Windows.Forms.RichTextBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.chkAutoRefresh = new System.Windows.Forms.CheckBox();
			this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
			this.lblFilePath = new System.Windows.Forms.Label();
			this.lblCsvContent = new System.Windows.Forms.Label();
			this.pnlHeader = new System.Windows.Forms.Panel();
			this.pnlContent = new System.Windows.Forms.Panel();
			this.pnlControl = new System.Windows.Forms.Panel();
			this.pnlHeader.SuspendLayout();
			this.pnlContent.SuspendLayout();
			this.pnlControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtFilePath
			// 
			this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilePath.Location = new System.Drawing.Point(9, 18);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.ReadOnly = true;
			this.txtFilePath.Size = new System.Drawing.Size(780, 19);
			this.txtFilePath.TabIndex = 0;
			// 
			// rtbContent
			// 
			this.rtbContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbContent.Location = new System.Drawing.Point(7, 17);
			this.rtbContent.Name = "rtbContent";
			this.rtbContent.ReadOnly = true;
			this.rtbContent.Size = new System.Drawing.Size(782, 400);
			this.rtbContent.TabIndex = 1;
			this.rtbContent.Text = "";
			this.rtbContent.WordWrap = false;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefresh.Location = new System.Drawing.Point(7, 6);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(75, 23);
			this.btnRefresh.TabIndex = 2;
			this.btnRefresh.Text = "更新";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// chkAutoRefresh
			// 
			this.chkAutoRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkAutoRefresh.AutoSize = true;
			this.chkAutoRefresh.Checked = true;
			this.chkAutoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkAutoRefresh.Location = new System.Drawing.Point(88, 10);
			this.chkAutoRefresh.Name = "chkAutoRefresh";
			this.chkAutoRefresh.Size = new System.Drawing.Size(72, 16);
			this.chkAutoRefresh.TabIndex = 3;
			this.chkAutoRefresh.Text = "自動更新";
			this.chkAutoRefresh.UseVisualStyleBackColor = true;
			// 
			// tmrRefresh
			// 
			this.tmrRefresh.Enabled = true;
			this.tmrRefresh.Interval = 200;
			this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
			// 
			// lblFilePath
			// 
			this.lblFilePath.AutoSize = true;
			this.lblFilePath.Location = new System.Drawing.Point(8, 3);
			this.lblFilePath.Name = "lblFilePath";
			this.lblFilePath.Size = new System.Drawing.Size(51, 12);
			this.lblFilePath.TabIndex = 4;
			this.lblFilePath.Text = "File Path";
			// 
			// lblCsvContent
			// 
			this.lblCsvContent.AutoSize = true;
			this.lblCsvContent.Location = new System.Drawing.Point(7, 4);
			this.lblCsvContent.Name = "lblCsvContent";
			this.lblCsvContent.Size = new System.Drawing.Size(52, 12);
			this.lblCsvContent.TabIndex = 5;
			this.lblCsvContent.Text = "CSV内容";
			// 
			// pnlHeader
			// 
			this.pnlHeader.Controls.Add(this.lblFilePath);
			this.pnlHeader.Controls.Add(this.txtFilePath);
			this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlHeader.Location = new System.Drawing.Point(0, 0);
			this.pnlHeader.Name = "pnlHeader";
			this.pnlHeader.Size = new System.Drawing.Size(801, 44);
			this.pnlHeader.TabIndex = 6;
			// 
			// pnlContent
			// 
			this.pnlContent.Controls.Add(this.lblCsvContent);
			this.pnlContent.Controls.Add(this.rtbContent);
			this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlContent.Location = new System.Drawing.Point(0, 44);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Size = new System.Drawing.Size(801, 460);
			this.pnlContent.TabIndex = 7;
			// 
			// pnlControl
			// 
			this.pnlControl.Controls.Add(this.chkAutoRefresh);
			this.pnlControl.Controls.Add(this.btnRefresh);
			this.pnlControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlControl.Location = new System.Drawing.Point(0, 467);
			this.pnlControl.Name = "pnlControl";
			this.pnlControl.Size = new System.Drawing.Size(801, 37);
			this.pnlControl.TabIndex = 8;
			// 
			// ShikigamiDataMonitorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(801, 504);
			this.Controls.Add(this.pnlControl);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.pnlHeader);
			this.Name = "ShikigamiDataMonitorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ShikigamiData Monitor";
			this.pnlHeader.ResumeLayout(false);
			this.pnlHeader.PerformLayout();
			this.pnlContent.ResumeLayout(false);
			this.pnlContent.PerformLayout();
			this.pnlControl.ResumeLayout(false);
			this.pnlControl.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.RichTextBox rtbContent;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.CheckBox chkAutoRefresh;
		private System.Windows.Forms.Timer tmrRefresh;
		private System.Windows.Forms.Label lblFilePath;
		private System.Windows.Forms.Label lblCsvContent;
		private System.Windows.Forms.Panel pnlHeader;
		private System.Windows.Forms.Panel pnlContent;
		private System.Windows.Forms.Panel pnlControl;
	}
}
