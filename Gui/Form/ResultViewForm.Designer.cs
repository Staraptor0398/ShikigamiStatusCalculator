namespace ShikigamiApp
{
	partial class ResultViewForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtMitamaStatus = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtFinalStatus = new System.Windows.Forms.TextBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtMitamaStatus);
			this.groupBox1.Location = new System.Drawing.Point(12, 21);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(322, 253);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "御魂のみのステータス";
			// 
			// txtMitamaStatus
			// 
			this.txtMitamaStatus.Font = new System.Drawing.Font("ＭＳ ゴシック", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.txtMitamaStatus.Location = new System.Drawing.Point(21, 21);
			this.txtMitamaStatus.Multiline = true;
			this.txtMitamaStatus.Name = "txtMitamaStatus";
			this.txtMitamaStatus.ReadOnly = true;
			this.txtMitamaStatus.Size = new System.Drawing.Size(279, 215);
			this.txtMitamaStatus.TabIndex = 0;
			this.txtMitamaStatus.TabStop = false;
			this.txtMitamaStatus.WordWrap = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtFinalStatus);
			this.groupBox2.Location = new System.Drawing.Point(13, 280);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(321, 205);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "最終ステータス";
			// 
			// txtFinalStatus
			// 
			this.txtFinalStatus.Font = new System.Drawing.Font("ＭＳ ゴシック", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.txtFinalStatus.Location = new System.Drawing.Point(21, 21);
			this.txtFinalStatus.Multiline = true;
			this.txtFinalStatus.Name = "txtFinalStatus";
			this.txtFinalStatus.ReadOnly = true;
			this.txtFinalStatus.Size = new System.Drawing.Size(279, 161);
			this.txtFinalStatus.TabIndex = 3;
			this.txtFinalStatus.TabStop = false;
			this.txtFinalStatus.WordWrap = false;
			// 
			// btnClose
			// 
			this.btnClose.Location = new System.Drawing.Point(123, 505);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(100, 30);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "閉じる";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// ResultViewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 550);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "ResultViewForm";
			this.Text = "結果表示";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtMitamaStatus;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.TextBox txtFinalStatus;
	}
}