namespace ScenarioRunner.Form
{
	partial class MainForm
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.lblScenarioFile = new System.Windows.Forms.Label();
			this.txtScenarioPath = new System.Windows.Forms.TextBox();
			this.btnBrowseScenario = new System.Windows.Forms.Button();
			this.btnRun = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.chkWatchMode = new System.Windows.Forms.CheckBox();
			this.chkKeepGuiOpenOnFailure = new System.Windows.Forms.CheckBox();
			this.lblScenario = new System.Windows.Forms.Label();
			this.rtbScenario = new System.Windows.Forms.RichTextBox();
			this.rtbExecutionLog = new System.Windows.Forms.RichTextBox();
			this.lblExecutionLog = new System.Windows.Forms.Label();
			this.btnShikigamiDataMonitor = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblScenarioFile
			// 
			this.lblScenarioFile.AutoSize = true;
			this.lblScenarioFile.Location = new System.Drawing.Point(12, 9);
			this.lblScenarioFile.Name = "lblScenarioFile";
			this.lblScenarioFile.Size = new System.Drawing.Size(72, 12);
			this.lblScenarioFile.TabIndex = 0;
			this.lblScenarioFile.Text = "Scenario File";
			// 
			// txtScenarioPath
			// 
			this.txtScenarioPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtScenarioPath.Location = new System.Drawing.Point(14, 24);
			this.txtScenarioPath.Name = "txtScenarioPath";
			this.txtScenarioPath.ReadOnly = true;
			this.txtScenarioPath.Size = new System.Drawing.Size(877, 19);
			this.txtScenarioPath.TabIndex = 1;
			// 
			// btnBrowseScenario
			// 
			this.btnBrowseScenario.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowseScenario.Location = new System.Drawing.Point(897, 22);
			this.btnBrowseScenario.Name = "btnBrowseScenario";
			this.btnBrowseScenario.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseScenario.TabIndex = 2;
			this.btnBrowseScenario.Text = "参照";
			this.btnBrowseScenario.UseVisualStyleBackColor = true;
			this.btnBrowseScenario.Click += new System.EventHandler(this.btnBrowseScenario_Click);
			// 
			// btnRun
			// 
			this.btnRun.Enabled = false;
			this.btnRun.Location = new System.Drawing.Point(14, 68);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(75, 23);
			this.btnRun.TabIndex = 3;
			this.btnRun.Text = "実行";
			this.btnRun.UseVisualStyleBackColor = true;
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			// 
			// btnStop
			// 
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(95, 68);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 4;
			this.btnStop.Text = "停止";
			this.btnStop.UseVisualStyleBackColor = true;
			// 
			// chkWatchMode
			// 
			this.chkWatchMode.AutoSize = true;
			this.chkWatchMode.Location = new System.Drawing.Point(342, 72);
			this.chkWatchMode.Name = "chkWatchMode";
			this.chkWatchMode.Size = new System.Drawing.Size(86, 16);
			this.chkWatchMode.TabIndex = 5;
			this.chkWatchMode.Text = "Watch Mode";
			this.chkWatchMode.UseVisualStyleBackColor = true;
			// 
			// chkKeepGuiOpenOnFailure
			// 
			this.chkKeepGuiOpenOnFailure.AutoSize = true;
			this.chkKeepGuiOpenOnFailure.Location = new System.Drawing.Point(342, 94);
			this.chkKeepGuiOpenOnFailure.Name = "chkKeepGuiOpenOnFailure";
			this.chkKeepGuiOpenOnFailure.Size = new System.Drawing.Size(156, 16);
			this.chkKeepGuiOpenOnFailure.TabIndex = 6;
			this.chkKeepGuiOpenOnFailure.Text = "失敗時にGui.exeを閉じない";
			this.chkKeepGuiOpenOnFailure.UseVisualStyleBackColor = true;
			// 
			// lblScenario
			// 
			this.lblScenario.AutoSize = true;
			this.lblScenario.Location = new System.Drawing.Point(12, 136);
			this.lblScenario.Name = "lblScenario";
			this.lblScenario.Size = new System.Drawing.Size(49, 12);
			this.lblScenario.TabIndex = 7;
			this.lblScenario.Text = "Scenario";
			// 
			// rtbScenario
			// 
			this.rtbScenario.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbScenario.Location = new System.Drawing.Point(14, 151);
			this.rtbScenario.Name = "rtbScenario";
			this.rtbScenario.ReadOnly = true;
			this.rtbScenario.Size = new System.Drawing.Size(958, 264);
			this.rtbScenario.TabIndex = 8;
			this.rtbScenario.Text = "";
			// 
			// rtbExecutionLog
			// 
			this.rtbExecutionLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbExecutionLog.Location = new System.Drawing.Point(12, 433);
			this.rtbExecutionLog.Name = "rtbExecutionLog";
			this.rtbExecutionLog.ReadOnly = true;
			this.rtbExecutionLog.Size = new System.Drawing.Size(958, 267);
			this.rtbExecutionLog.TabIndex = 9;
			this.rtbExecutionLog.Text = "";
			// 
			// lblExecutionLog
			// 
			this.lblExecutionLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblExecutionLog.AutoSize = true;
			this.lblExecutionLog.Location = new System.Drawing.Point(10, 418);
			this.lblExecutionLog.Name = "lblExecutionLog";
			this.lblExecutionLog.Size = new System.Drawing.Size(77, 12);
			this.lblExecutionLog.TabIndex = 10;
			this.lblExecutionLog.Text = "Execution Log";
			// 
			// btnShikigamiDataMonitor
			// 
			this.btnShikigamiDataMonitor.Location = new System.Drawing.Point(497, 68);
			this.btnShikigamiDataMonitor.Name = "btnShikigamiDataMonitor";
			this.btnShikigamiDataMonitor.Size = new System.Drawing.Size(92, 23);
			this.btnShikigamiDataMonitor.TabIndex = 11;
			this.btnShikigamiDataMonitor.Text = "式神データ監視";
			this.btnShikigamiDataMonitor.UseVisualStyleBackColor = true;
			this.btnShikigamiDataMonitor.Click += new System.EventHandler(this.btnShikigamiDataMonitor_Click);
			// 
			// btnSave
			// 
			this.btnSave.Enabled = false;
			this.btnSave.Location = new System.Drawing.Point(257, 68);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 13;
			this.btnSave.Text = "保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.Enabled = false;
			this.btnEdit.Location = new System.Drawing.Point(176, 68);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(75, 23);
			this.btnEdit.TabIndex = 12;
			this.btnEdit.Text = "編集";
			this.btnEdit.UseVisualStyleBackColor = true;
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 711);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.btnEdit);
			this.Controls.Add(this.btnShikigamiDataMonitor);
			this.Controls.Add(this.lblExecutionLog);
			this.Controls.Add(this.rtbExecutionLog);
			this.Controls.Add(this.rtbScenario);
			this.Controls.Add(this.lblScenario);
			this.Controls.Add(this.chkKeepGuiOpenOnFailure);
			this.Controls.Add(this.chkWatchMode);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnRun);
			this.Controls.Add(this.btnBrowseScenario);
			this.Controls.Add(this.txtScenarioPath);
			this.Controls.Add(this.lblScenarioFile);
			this.MinimumSize = new System.Drawing.Size(700, 600);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ShikigamiApp Scenario Runner";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblScenarioFile;
		private System.Windows.Forms.TextBox txtScenarioPath;
		private System.Windows.Forms.Button btnBrowseScenario;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.CheckBox chkWatchMode;
		private System.Windows.Forms.CheckBox chkKeepGuiOpenOnFailure;
		private System.Windows.Forms.Label lblScenario;
		private System.Windows.Forms.RichTextBox rtbScenario;
		private System.Windows.Forms.RichTextBox rtbExecutionLog;
		private System.Windows.Forms.Label lblExecutionLog;
		private System.Windows.Forms.Button btnShikigamiDataMonitor;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnEdit;
	}
}

