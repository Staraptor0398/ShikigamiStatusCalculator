namespace Gui.Dialog
{
	partial class ShikigamiRecoveryDialog
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
			this.lblDescription = new System.Windows.Forms.Label();
			this.lblRecoveryCandinateCount = new System.Windows.Forms.Label();
			this.dgvRecoveryCandinates = new System.Windows.Forms.DataGridView();
			this.columnRecovery = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.columnRarity = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnAttack = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnHP = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnDefence = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnSelectAll = new System.Windows.Forms.Button();
			this.btnAllClear = new System.Windows.Forms.Button();
			this.btnRecovery = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgvRecoveryCandinates)).BeginInit();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			this.lblDescription.AutoSize = true;
			this.lblDescription.Location = new System.Drawing.Point(12, 9);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(164, 12);
			this.lblDescription.TabIndex = 0;
			this.lblDescription.Text = "復旧する式神を選択してください。";
			// 
			// lblRecoveryCandinateCount
			// 
			this.lblRecoveryCandinateCount.AutoSize = true;
			this.lblRecoveryCandinateCount.Location = new System.Drawing.Point(12, 32);
			this.lblRecoveryCandinateCount.Name = "lblRecoveryCandinateCount";
			this.lblRecoveryCandinateCount.Size = new System.Drawing.Size(77, 12);
			this.lblRecoveryCandinateCount.TabIndex = 1;
			this.lblRecoveryCandinateCount.Text = "復旧候補：0件";
			// 
			// dgvRecoveryCandinates
			// 
			this.dgvRecoveryCandinates.AllowUserToAddRows = false;
			this.dgvRecoveryCandinates.AllowUserToDeleteRows = false;
			this.dgvRecoveryCandinates.AllowUserToResizeColumns = false;
			this.dgvRecoveryCandinates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvRecoveryCandinates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnRecovery,
            this.columnRarity,
            this.columnName,
            this.columnAttack,
            this.columnHP,
            this.columnDefence,
            this.columnSpeed});
			this.dgvRecoveryCandinates.Location = new System.Drawing.Point(12, 54);
			this.dgvRecoveryCandinates.MultiSelect = false;
			this.dgvRecoveryCandinates.Name = "dgvRecoveryCandinates";
			this.dgvRecoveryCandinates.RowHeadersVisible = false;
			this.dgvRecoveryCandinates.RowTemplate.Height = 21;
			this.dgvRecoveryCandinates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvRecoveryCandinates.Size = new System.Drawing.Size(703, 345);
			this.dgvRecoveryCandinates.TabIndex = 2;
			// 
			// columnRecovery
			// 
			this.columnRecovery.HeaderText = "復旧";
			this.columnRecovery.Name = "columnRecovery";
			// 
			// columnRarity
			// 
			this.columnRarity.HeaderText = "レアリティ";
			this.columnRarity.Name = "columnRarity";
			this.columnRarity.ReadOnly = true;
			// 
			// columnName
			// 
			this.columnName.HeaderText = "式神名";
			this.columnName.Name = "columnName";
			this.columnName.ReadOnly = true;
			// 
			// columnAttack
			// 
			this.columnAttack.HeaderText = "攻撃力";
			this.columnAttack.Name = "columnAttack";
			this.columnAttack.ReadOnly = true;
			// 
			// columnHP
			// 
			this.columnHP.HeaderText = "HP";
			this.columnHP.Name = "columnHP";
			this.columnHP.ReadOnly = true;
			// 
			// columnDefence
			// 
			this.columnDefence.HeaderText = "防御力";
			this.columnDefence.Name = "columnDefence";
			this.columnDefence.ReadOnly = true;
			// 
			// columnSpeed
			// 
			this.columnSpeed.HeaderText = "素早さ";
			this.columnSpeed.Name = "columnSpeed";
			this.columnSpeed.ReadOnly = true;
			// 
			// btnSelectAll
			// 
			this.btnSelectAll.Location = new System.Drawing.Point(14, 415);
			this.btnSelectAll.Name = "btnSelectAll";
			this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
			this.btnSelectAll.TabIndex = 3;
			this.btnSelectAll.Text = "全て選択";
			this.btnSelectAll.UseVisualStyleBackColor = true;
			this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
			// 
			// btnAllClear
			// 
			this.btnAllClear.Location = new System.Drawing.Point(101, 415);
			this.btnAllClear.Name = "btnAllClear";
			this.btnAllClear.Size = new System.Drawing.Size(75, 23);
			this.btnAllClear.TabIndex = 4;
			this.btnAllClear.Text = "全て解除";
			this.btnAllClear.UseVisualStyleBackColor = true;
			this.btnAllClear.Click += new System.EventHandler(this.btnAllClear_Click);
			// 
			// btnRecovery
			// 
			this.btnRecovery.Location = new System.Drawing.Point(559, 415);
			this.btnRecovery.Name = "btnRecovery";
			this.btnRecovery.Size = new System.Drawing.Size(75, 23);
			this.btnRecovery.TabIndex = 5;
			this.btnRecovery.Text = "復旧";
			this.btnRecovery.UseVisualStyleBackColor = true;
			this.btnRecovery.Click += new System.EventHandler(this.btnRecovery_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(640, 415);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// ShikigamiRecoveryDialog
			// 
			this.AcceptButton = this.btnRecovery;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(729, 450);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnRecovery);
			this.Controls.Add(this.btnAllClear);
			this.Controls.Add(this.btnSelectAll);
			this.Controls.Add(this.dgvRecoveryCandinates);
			this.Controls.Add(this.lblRecoveryCandinateCount);
			this.Controls.Add(this.lblDescription);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ShikigamiRecoveryDialog";
			this.Text = "式神データ復旧";
			((System.ComponentModel.ISupportInitialize)(this.dgvRecoveryCandinates)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Label lblRecoveryCandinateCount;
		private System.Windows.Forms.DataGridView dgvRecoveryCandinates;
		private System.Windows.Forms.Button btnSelectAll;
		private System.Windows.Forms.Button btnAllClear;
		private System.Windows.Forms.Button btnRecovery;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.DataGridViewCheckBoxColumn columnRecovery;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnRarity;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnAttack;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnHP;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnDefence;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnSpeed;
	}
}
