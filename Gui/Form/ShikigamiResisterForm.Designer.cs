namespace ShikigamiApp
{
	partial class ShikigamiResisterForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbRarity = new System.Windows.Forms.ComboBox();
			this.txtName = new System.Windows.Forms.TextBox();
			this.btnResister = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtEffectResist = new System.Windows.Forms.TextBox();
			this.txtEffectHit = new System.Windows.Forms.TextBox();
			this.txtCritDamage = new System.Windows.Forms.TextBox();
			this.txtCritRate = new System.Windows.Forms.TextBox();
			this.txtSpeed = new System.Windows.Forms.TextBox();
			this.txtDeffense = new System.Windows.Forms.TextBox();
			this.txtHP = new System.Windows.Forms.TextBox();
			this.txtAttck = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(33, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(46, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "レアリティ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(33, 59);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "式神名";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmbRarity
			// 
			this.cmbRarity.FormattingEnabled = true;
			this.cmbRarity.Location = new System.Drawing.Point(114, 18);
			this.cmbRarity.Name = "cmbRarity";
			this.cmbRarity.Size = new System.Drawing.Size(121, 20);
			this.cmbRarity.TabIndex = 2;
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(114, 56);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(121, 19);
			this.txtName.TabIndex = 3;
			// 
			// btnResister
			// 
			this.btnResister.Location = new System.Drawing.Point(47, 272);
			this.btnResister.Name = "btnResister";
			this.btnResister.Size = new System.Drawing.Size(75, 23);
			this.btnResister.TabIndex = 4;
			this.btnResister.Text = "登録";
			this.btnResister.UseVisualStyleBackColor = true;
			this.btnResister.Click += new System.EventHandler(this.btnResister_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(147, 272);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cmbRarity);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.txtName);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(264, 94);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "基本情報";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtEffectResist);
			this.groupBox2.Controls.Add(this.txtEffectHit);
			this.groupBox2.Controls.Add(this.txtCritDamage);
			this.groupBox2.Controls.Add(this.txtCritRate);
			this.groupBox2.Controls.Add(this.txtSpeed);
			this.groupBox2.Controls.Add(this.txtDeffense);
			this.groupBox2.Controls.Add(this.txtHP);
			this.groupBox2.Controls.Add(this.txtAttck);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Location = new System.Drawing.Point(12, 127);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(264, 124);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "ステータス";
			// 
			// txtEffectResist
			// 
			this.txtEffectResist.Location = new System.Drawing.Point(186, 86);
			this.txtEffectResist.Name = "txtEffectResist";
			this.txtEffectResist.Size = new System.Drawing.Size(49, 19);
			this.txtEffectResist.TabIndex = 15;
			// 
			// txtEffectHit
			// 
			this.txtEffectHit.Location = new System.Drawing.Point(186, 65);
			this.txtEffectHit.Name = "txtEffectHit";
			this.txtEffectHit.Size = new System.Drawing.Size(49, 19);
			this.txtEffectHit.TabIndex = 14;
			// 
			// txtCritDamage
			// 
			this.txtCritDamage.Location = new System.Drawing.Point(186, 44);
			this.txtCritDamage.Name = "txtCritDamage";
			this.txtCritDamage.Size = new System.Drawing.Size(49, 19);
			this.txtCritDamage.TabIndex = 13;
			// 
			// txtCritRate
			// 
			this.txtCritRate.Location = new System.Drawing.Point(186, 23);
			this.txtCritRate.Name = "txtCritRate";
			this.txtCritRate.Size = new System.Drawing.Size(49, 19);
			this.txtCritRate.TabIndex = 12;
			// 
			// txtSpeed
			// 
			this.txtSpeed.Location = new System.Drawing.Point(78, 86);
			this.txtSpeed.Name = "txtSpeed";
			this.txtSpeed.Size = new System.Drawing.Size(49, 19);
			this.txtSpeed.TabIndex = 11;
			// 
			// txtDeffense
			// 
			this.txtDeffense.Location = new System.Drawing.Point(78, 65);
			this.txtDeffense.Name = "txtDeffense";
			this.txtDeffense.Size = new System.Drawing.Size(49, 19);
			this.txtDeffense.TabIndex = 10;
			// 
			// txtHP
			// 
			this.txtHP.Location = new System.Drawing.Point(78, 44);
			this.txtHP.Name = "txtHP";
			this.txtHP.Size = new System.Drawing.Size(49, 19);
			this.txtHP.TabIndex = 9;
			// 
			// txtAttck
			// 
			this.txtAttck.Location = new System.Drawing.Point(78, 23);
			this.txtAttck.Name = "txtAttck";
			this.txtAttck.Size = new System.Drawing.Size(49, 19);
			this.txtAttck.TabIndex = 8;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(133, 89);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(53, 12);
			this.label7.TabIndex = 7;
			this.label7.Text = "効果抵抗";
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(133, 68);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(53, 12);
			this.label8.TabIndex = 6;
			this.label8.Text = "効果命中";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(133, 47);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(54, 12);
			this.label9.TabIndex = 5;
			this.label9.Text = "会心DMG";
			this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(133, 26);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(41, 12);
			this.label10.TabIndex = 4;
			this.label10.Text = "会心率";
			this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(33, 90);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(37, 12);
			this.label6.TabIndex = 3;
			this.label6.Text = "素早さ";
			this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(33, 68);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(41, 12);
			this.label5.TabIndex = 2;
			this.label5.Text = "防御力";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(33, 47);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(20, 12);
			this.label4.TabIndex = 1;
			this.label4.Text = "HP";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(31, 26);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 12);
			this.label3.TabIndex = 0;
			this.label3.Text = "攻撃力";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ShikigamiResisterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(288, 312);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnResister);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "ShikigamiResisterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "式神登録";
			this.Load += new System.EventHandler(this.ShikigamiResisterForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbRarity;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Button btnResister;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtSpeed;
		private System.Windows.Forms.TextBox txtDeffense;
		private System.Windows.Forms.TextBox txtHP;
		private System.Windows.Forms.TextBox txtAttck;
		private System.Windows.Forms.TextBox txtEffectResist;
		private System.Windows.Forms.TextBox txtEffectHit;
		private System.Windows.Forms.TextBox txtCritDamage;
		private System.Windows.Forms.TextBox txtCritRate;
	}
}