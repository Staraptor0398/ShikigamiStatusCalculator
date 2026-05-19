namespace Gui.Dialog
{
	partial class SnapshotCompareFileSelectDialog
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
			this.btnBrowseBaseSnapshot = new System.Windows.Forms.Button();
			this.btnBrowseTargetSnapshot = new System.Windows.Forms.Button();
			this.btnCompare = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtBaseSnapshotFilePath = new System.Windows.Forms.TextBox();
			this.txtTargetSnapshotFilePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnBrowseBaseSnapshot
			// 
			this.btnBrowseBaseSnapshot.Location = new System.Drawing.Point(713, 6);
			this.btnBrowseBaseSnapshot.Name = "btnBrowseBaseSnapshot";
			this.btnBrowseBaseSnapshot.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseBaseSnapshot.TabIndex = 0;
			this.btnBrowseBaseSnapshot.Text = "参照";
			this.btnBrowseBaseSnapshot.UseVisualStyleBackColor = true;
			this.btnBrowseBaseSnapshot.Click += new System.EventHandler(this.btnBrowseBaseSnapshot_Click);
			// 
			// btnBrowseTargetSnapshot
			// 
			this.btnBrowseTargetSnapshot.Location = new System.Drawing.Point(713, 38);
			this.btnBrowseTargetSnapshot.Name = "btnBrowseTargetSnapshot";
			this.btnBrowseTargetSnapshot.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseTargetSnapshot.TabIndex = 1;
			this.btnBrowseTargetSnapshot.Text = "参照";
			this.btnBrowseTargetSnapshot.UseVisualStyleBackColor = true;
			this.btnBrowseTargetSnapshot.Click += new System.EventHandler(this.btnBrowseTargetSnapshot_Click);
			// 
			// btnCompare
			// 
			this.btnCompare.Location = new System.Drawing.Point(632, 67);
			this.btnCompare.Name = "btnCompare";
			this.btnCompare.Size = new System.Drawing.Size(75, 23);
			this.btnCompare.TabIndex = 2;
			this.btnCompare.Text = "比較";
			this.btnCompare.UseVisualStyleBackColor = true;
			this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(713, 67);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// txtBaseSnapshotFilePath
			// 
			this.txtBaseSnapshotFilePath.Location = new System.Drawing.Point(125, 6);
			this.txtBaseSnapshotFilePath.Name = "txtBaseSnapshotFilePath";
			this.txtBaseSnapshotFilePath.ReadOnly = true;
			this.txtBaseSnapshotFilePath.Size = new System.Drawing.Size(582, 19);
			this.txtBaseSnapshotFilePath.TabIndex = 4;
			// 
			// txtTargetSnapshotFilePath
			// 
			this.txtTargetSnapshotFilePath.Location = new System.Drawing.Point(125, 38);
			this.txtTargetSnapshotFilePath.Name = "txtTargetSnapshotFilePath";
			this.txtTargetSnapshotFilePath.ReadOnly = true;
			this.txtTargetSnapshotFilePath.Size = new System.Drawing.Size(582, 19);
			this.txtTargetSnapshotFilePath.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 12);
			this.label1.TabIndex = 6;
			this.label1.Text = "基準スナップショット";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(107, 12);
			this.label2.TabIndex = 7;
			this.label2.Text = "比較先スナップショット";
			// 
			// SnapshotCompareFileSelectDialog
			// 
			this.AcceptButton = this.btnCompare;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(800, 103);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtTargetSnapshotFilePath);
			this.Controls.Add(this.txtBaseSnapshotFilePath);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnCompare);
			this.Controls.Add(this.btnBrowseTargetSnapshot);
			this.Controls.Add(this.btnBrowseBaseSnapshot);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SnapshotCompareFileSelectDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "比較するスナップショット選択";
			this.Load += new System.EventHandler(this.SnapshotCompareFileSelectDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnBrowseBaseSnapshot;
		private System.Windows.Forms.Button btnBrowseTargetSnapshot;
		private System.Windows.Forms.Button btnCompare;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtBaseSnapshotFilePath;
		private System.Windows.Forms.TextBox txtTargetSnapshotFilePath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}