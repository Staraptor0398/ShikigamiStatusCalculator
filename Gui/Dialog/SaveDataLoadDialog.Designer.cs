namespace Gui.Dialog
{
	partial class SaveDataLoadDialog
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
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnLoad = new System.Windows.Forms.Button();
			this.txtFilePath = new System.Windows.Forms.TextBox();
			this.cmbLoadType = new System.Windows.Forms.ComboBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 12);
			this.label2.TabIndex = 13;
			this.label2.Text = "読み込み元";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 12);
			this.label1.TabIndex = 12;
			this.label1.Text = "読み込み形式";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(723, 74);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(642, 74);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(75, 23);
			this.btnLoad.TabIndex = 10;
			this.btnLoad.Text = "読み込み";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// txtFilePath
			// 
			this.txtFilePath.Location = new System.Drawing.Point(93, 45);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.ReadOnly = true;
			this.txtFilePath.Size = new System.Drawing.Size(624, 19);
			this.txtFilePath.TabIndex = 9;
			// 
			// cmbLoadType
			// 
			this.cmbLoadType.FormattingEnabled = true;
			this.cmbLoadType.Location = new System.Drawing.Point(93, 15);
			this.cmbLoadType.Name = "cmbLoadType";
			this.cmbLoadType.Size = new System.Drawing.Size(140, 20);
			this.cmbLoadType.TabIndex = 8;
			this.cmbLoadType.SelectedIndexChanged += new System.EventHandler(this.cmbLoadType_SelectedIndexChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(723, 45);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.btnBrowse.TabIndex = 7;
			this.btnBrowse.Text = "参照";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// SaveDataLoadDialog
			// 
			this.AcceptButton = this.btnLoad;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(808, 107);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.txtFilePath);
			this.Controls.Add(this.cmbLoadType);
			this.Controls.Add(this.btnBrowse);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveDataLoadDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "読み込み";
			this.Load += new System.EventHandler(this.SaveDataLoadDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.ComboBox cmbLoadType;
		private System.Windows.Forms.Button btnBrowse;
	}
}
