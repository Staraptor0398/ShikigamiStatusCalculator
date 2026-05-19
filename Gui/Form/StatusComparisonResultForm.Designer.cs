namespace ShikigamiApp
{
	partial class StatusComparisonResultForm
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
			this.lblBaseSnapshotTitle = new System.Windows.Forms.Label();
			this.lblBaseSnapshotName = new System.Windows.Forms.Label();
			this.lblTargetSnapshotTitle = new System.Windows.Forms.Label();
			this.lblTargetSnapshotName = new System.Windows.Forms.Label();
			this.dgvComparisonResult = new System.Windows.Forms.DataGridView();
			this.btnClose = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgvComparisonResult)).BeginInit();
			this.SuspendLayout();
			// 
			// lblBaseSnapshotTitle
			// 
			this.lblBaseSnapshotTitle.AutoSize = true;
			this.lblBaseSnapshotTitle.Location = new System.Drawing.Point(9, 12);
			this.lblBaseSnapshotTitle.Name = "lblBaseSnapshotTitle";
			this.lblBaseSnapshotTitle.Size = new System.Drawing.Size(101, 12);
			this.lblBaseSnapshotTitle.TabIndex = 0;
			this.lblBaseSnapshotTitle.Text = "基準スナップショット：";
			// 
			// lblBaseSnapshotName
			// 
			this.lblBaseSnapshotName.AutoSize = true;
			this.lblBaseSnapshotName.Location = new System.Drawing.Point(181, 12);
			this.lblBaseSnapshotName.Name = "lblBaseSnapshotName";
			this.lblBaseSnapshotName.Size = new System.Drawing.Size(106, 12);
			this.lblBaseSnapshotName.TabIndex = 1;
			this.lblBaseSnapshotName.Text = "xxxxx.snapshot.json";
			// 
			// lblTargetSnapshotTitle
			// 
			this.lblTargetSnapshotTitle.AutoSize = true;
			this.lblTargetSnapshotTitle.Location = new System.Drawing.Point(11, 44);
			this.lblTargetSnapshotTitle.Name = "lblTargetSnapshotTitle";
			this.lblTargetSnapshotTitle.Size = new System.Drawing.Size(113, 12);
			this.lblTargetSnapshotTitle.TabIndex = 2;
			this.lblTargetSnapshotTitle.Text = "比較先スナップショット：";
			// 
			// lblTargetSnapshotName
			// 
			this.lblTargetSnapshotName.AutoSize = true;
			this.lblTargetSnapshotName.Location = new System.Drawing.Point(181, 44);
			this.lblTargetSnapshotName.Name = "lblTargetSnapshotName";
			this.lblTargetSnapshotName.Size = new System.Drawing.Size(106, 12);
			this.lblTargetSnapshotName.TabIndex = 3;
			this.lblTargetSnapshotName.Text = "yyyyy.snapshot.json";
			// 
			// dgvComparisonResult
			// 
			this.dgvComparisonResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvComparisonResult.Location = new System.Drawing.Point(14, 72);
			this.dgvComparisonResult.Name = "dgvComparisonResult";
			this.dgvComparisonResult.RowTemplate.Height = 21;
			this.dgvComparisonResult.Size = new System.Drawing.Size(289, 190);
			this.dgvComparisonResult.TabIndex = 4;
			// 
			// btnClose
			// 
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(121, 277);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(75, 23);
			this.btnClose.TabIndex = 5;
			this.btnClose.Text = "閉じる";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// StatusComparisonResultForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(316, 312);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.dgvComparisonResult);
			this.Controls.Add(this.lblTargetSnapshotName);
			this.Controls.Add(this.lblTargetSnapshotTitle);
			this.Controls.Add(this.lblBaseSnapshotName);
			this.Controls.Add(this.lblBaseSnapshotTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StatusComparisonResultForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "比較結果";
			this.Load += new System.EventHandler(this.StatusComparisonResultForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgvComparisonResult)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblBaseSnapshotTitle;
		private System.Windows.Forms.Label lblBaseSnapshotName;
		private System.Windows.Forms.Label lblTargetSnapshotTitle;
		private System.Windows.Forms.Label lblTargetSnapshotName;
		private System.Windows.Forms.DataGridView dgvComparisonResult;
		private System.Windows.Forms.Button btnClose;
	}
}
