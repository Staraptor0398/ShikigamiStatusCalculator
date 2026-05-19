using System.Windows.Forms;
using System.Drawing;

namespace ShikigamiApp
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
			this.lblShikigami = new System.Windows.Forms.Label();
			this.cmbShikigami = new System.Windows.Forms.ComboBox();
			this.txtBaseStats = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbMainStat1 = new System.Windows.Forms.ComboBox();
			this.txtMainVal1 = new System.Windows.Forms.TextBox();
			this.txtMainVal2 = new System.Windows.Forms.TextBox();
			this.cmbMainStat2 = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtMainVal3 = new System.Windows.Forms.TextBox();
			this.cmbMainStat3 = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtMainVal4 = new System.Windows.Forms.TextBox();
			this.cmbMainStat4 = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtMainVal5 = new System.Windows.Forms.TextBox();
			this.cmbMainStat5 = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtMainVal6 = new System.Windows.Forms.TextBox();
			this.cmbMainStat6 = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cmbSubStat11 = new System.Windows.Forms.ComboBox();
			this.txtSubVal11 = new System.Windows.Forms.TextBox();
			this.txtSubVal21 = new System.Windows.Forms.TextBox();
			this.cmbSubStat21 = new System.Windows.Forms.ComboBox();
			this.txtSubVal31 = new System.Windows.Forms.TextBox();
			this.cmbSubStat31 = new System.Windows.Forms.ComboBox();
			this.txtSubVal41 = new System.Windows.Forms.TextBox();
			this.cmbSubStat41 = new System.Windows.Forms.ComboBox();
			this.txtSubVal42 = new System.Windows.Forms.TextBox();
			this.cmbSubStat42 = new System.Windows.Forms.ComboBox();
			this.txtSubVal32 = new System.Windows.Forms.TextBox();
			this.cmbSubStat32 = new System.Windows.Forms.ComboBox();
			this.txtSubVal22 = new System.Windows.Forms.TextBox();
			this.cmbSubStat22 = new System.Windows.Forms.ComboBox();
			this.txtSubVal12 = new System.Windows.Forms.TextBox();
			this.cmbSubStat12 = new System.Windows.Forms.ComboBox();
			this.txtSubVal43 = new System.Windows.Forms.TextBox();
			this.cmbSubStat43 = new System.Windows.Forms.ComboBox();
			this.txtSubVal33 = new System.Windows.Forms.TextBox();
			this.cmbSubStat33 = new System.Windows.Forms.ComboBox();
			this.txtSubVal23 = new System.Windows.Forms.TextBox();
			this.cmbSubStat23 = new System.Windows.Forms.ComboBox();
			this.txtSubVal13 = new System.Windows.Forms.TextBox();
			this.cmbSubStat13 = new System.Windows.Forms.ComboBox();
			this.txtSubVal44 = new System.Windows.Forms.TextBox();
			this.cmbSubStat44 = new System.Windows.Forms.ComboBox();
			this.txtSubVal34 = new System.Windows.Forms.TextBox();
			this.cmbSubStat34 = new System.Windows.Forms.ComboBox();
			this.txtSubVal24 = new System.Windows.Forms.TextBox();
			this.cmbSubStat24 = new System.Windows.Forms.ComboBox();
			this.txtSubVal14 = new System.Windows.Forms.TextBox();
			this.cmbSubStat14 = new System.Windows.Forms.ComboBox();
			this.txtSubVal45 = new System.Windows.Forms.TextBox();
			this.cmbSubStat45 = new System.Windows.Forms.ComboBox();
			this.txtSubVal35 = new System.Windows.Forms.TextBox();
			this.cmbSubStat35 = new System.Windows.Forms.ComboBox();
			this.txtSubVal25 = new System.Windows.Forms.TextBox();
			this.cmbSubStat25 = new System.Windows.Forms.ComboBox();
			this.txtSubVal15 = new System.Windows.Forms.TextBox();
			this.cmbSubStat15 = new System.Windows.Forms.ComboBox();
			this.txtSubVal46 = new System.Windows.Forms.TextBox();
			this.cmbSubStat46 = new System.Windows.Forms.ComboBox();
			this.txtSubVal36 = new System.Windows.Forms.TextBox();
			this.cmbSubStat36 = new System.Windows.Forms.ComboBox();
			this.txtSubVal26 = new System.Windows.Forms.TextBox();
			this.cmbSubStat26 = new System.Windows.Forms.ComboBox();
			this.txtSubVal16 = new System.Windows.Forms.TextBox();
			this.cmbSubStat16 = new System.Windows.Forms.ComboBox();
			this.cmbSetBonus1 = new System.Windows.Forms.ComboBox();
			this.cmbSetBonus2 = new System.Windows.Forms.ComboBox();
			this.cmbSetBonus3 = new System.Windows.Forms.ComboBox();
			this.cmbUnique1 = new System.Windows.Forms.ComboBox();
			this.cmbUnique2 = new System.Windows.Forms.ComboBox();
			this.cmbUnique3 = new System.Windows.Forms.ComboBox();
			this.cmbUnique4 = new System.Windows.Forms.ComboBox();
			this.cmbUnique5 = new System.Windows.Forms.ComboBox();
			this.cmbUnique6 = new System.Windows.Forms.ComboBox();
			this.txtMitamaOnly = new System.Windows.Forms.TextBox();
			this.txtFinalStats = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.btnCalc = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnLoad = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.btnReLoad = new System.Windows.Forms.Button();
			this.btnAddShikigami = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.btnResultView = new System.Windows.Forms.Button();
			this.btnEditShikigami = new System.Windows.Forms.Button();
			this.btnCompareResult = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblShikigami
			// 
			this.lblShikigami.AutoSize = true;
			this.lblShikigami.Location = new System.Drawing.Point(28, 14);
			this.lblShikigami.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblShikigami.Name = "lblShikigami";
			this.lblShikigami.Size = new System.Drawing.Size(53, 12);
			this.lblShikigami.TabIndex = 0;
			this.lblShikigami.Text = "式神選択";
			// 
			// cmbShikigami
			// 
			this.cmbShikigami.FormattingEnabled = true;
			this.cmbShikigami.Location = new System.Drawing.Point(31, 29);
			this.cmbShikigami.Margin = new System.Windows.Forms.Padding(2);
			this.cmbShikigami.Name = "cmbShikigami";
			this.cmbShikigami.Size = new System.Drawing.Size(92, 20);
			this.cmbShikigami.TabIndex = 1;
			this.cmbShikigami.SelectedIndexChanged += new System.EventHandler(this.cmbShikigami_SelectedIndexChanged);
			// 
			// txtBaseStats
			// 
			this.txtBaseStats.Location = new System.Drawing.Point(132, 29);
			this.txtBaseStats.Margin = new System.Windows.Forms.Padding(2);
			this.txtBaseStats.Multiline = true;
			this.txtBaseStats.Name = "txtBaseStats";
			this.txtBaseStats.ReadOnly = true;
			this.txtBaseStats.Size = new System.Drawing.Size(923, 18);
			this.txtBaseStats.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 86);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "壱";
			// 
			// cmbMainStat1
			// 
			this.cmbMainStat1.FormattingEnabled = true;
			this.cmbMainStat1.Location = new System.Drawing.Point(31, 83);
			this.cmbMainStat1.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat1.Name = "cmbMainStat1";
			this.cmbMainStat1.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat1.TabIndex = 4;
			this.cmbMainStat1.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat1_SelectedIndexChanged);
			// 
			// txtMainVal1
			// 
			this.txtMainVal1.Location = new System.Drawing.Point(131, 83);
			this.txtMainVal1.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal1.Name = "txtMainVal1";
			this.txtMainVal1.ReadOnly = true;
			this.txtMainVal1.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal1.TabIndex = 5;
			// 
			// txtMainVal2
			// 
			this.txtMainVal2.Location = new System.Drawing.Point(131, 106);
			this.txtMainVal2.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal2.Name = "txtMainVal2";
			this.txtMainVal2.ReadOnly = true;
			this.txtMainVal2.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal2.TabIndex = 8;
			// 
			// cmbMainStat2
			// 
			this.cmbMainStat2.FormattingEnabled = true;
			this.cmbMainStat2.Location = new System.Drawing.Point(31, 106);
			this.cmbMainStat2.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat2.Name = "cmbMainStat2";
			this.cmbMainStat2.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat2.TabIndex = 7;
			this.cmbMainStat2.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat2_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 108);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 12);
			this.label2.TabIndex = 6;
			this.label2.Text = "弐";
			// 
			// txtMainVal3
			// 
			this.txtMainVal3.Location = new System.Drawing.Point(131, 128);
			this.txtMainVal3.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal3.Name = "txtMainVal3";
			this.txtMainVal3.ReadOnly = true;
			this.txtMainVal3.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal3.TabIndex = 11;
			// 
			// cmbMainStat3
			// 
			this.cmbMainStat3.FormattingEnabled = true;
			this.cmbMainStat3.Location = new System.Drawing.Point(31, 128);
			this.cmbMainStat3.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat3.Name = "cmbMainStat3";
			this.cmbMainStat3.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat3.TabIndex = 10;
			this.cmbMainStat3.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat3_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 131);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(17, 12);
			this.label3.TabIndex = 9;
			this.label3.Text = "参";
			// 
			// txtMainVal4
			// 
			this.txtMainVal4.Location = new System.Drawing.Point(131, 151);
			this.txtMainVal4.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal4.Name = "txtMainVal4";
			this.txtMainVal4.ReadOnly = true;
			this.txtMainVal4.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal4.TabIndex = 14;
			// 
			// cmbMainStat4
			// 
			this.cmbMainStat4.FormattingEnabled = true;
			this.cmbMainStat4.Location = new System.Drawing.Point(31, 151);
			this.cmbMainStat4.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat4.Name = "cmbMainStat4";
			this.cmbMainStat4.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat4.TabIndex = 13;
			this.cmbMainStat4.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat4_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 154);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(17, 12);
			this.label4.TabIndex = 12;
			this.label4.Text = "肆";
			// 
			// txtMainVal5
			// 
			this.txtMainVal5.Location = new System.Drawing.Point(131, 174);
			this.txtMainVal5.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal5.Name = "txtMainVal5";
			this.txtMainVal5.ReadOnly = true;
			this.txtMainVal5.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal5.TabIndex = 17;
			// 
			// cmbMainStat5
			// 
			this.cmbMainStat5.FormattingEnabled = true;
			this.cmbMainStat5.Location = new System.Drawing.Point(31, 173);
			this.cmbMainStat5.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat5.Name = "cmbMainStat5";
			this.cmbMainStat5.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat5.TabIndex = 16;
			this.cmbMainStat5.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat5_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 176);
			this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 12);
			this.label5.TabIndex = 15;
			this.label5.Text = "伍";
			// 
			// txtMainVal6
			// 
			this.txtMainVal6.Location = new System.Drawing.Point(131, 196);
			this.txtMainVal6.Margin = new System.Windows.Forms.Padding(2);
			this.txtMainVal6.Name = "txtMainVal6";
			this.txtMainVal6.ReadOnly = true;
			this.txtMainVal6.Size = new System.Drawing.Size(92, 19);
			this.txtMainVal6.TabIndex = 20;
			// 
			// cmbMainStat6
			// 
			this.cmbMainStat6.FormattingEnabled = true;
			this.cmbMainStat6.Location = new System.Drawing.Point(31, 196);
			this.cmbMainStat6.Margin = new System.Windows.Forms.Padding(2);
			this.cmbMainStat6.Name = "cmbMainStat6";
			this.cmbMainStat6.Size = new System.Drawing.Size(92, 20);
			this.cmbMainStat6.TabIndex = 19;
			this.cmbMainStat6.SelectedIndexChanged += new System.EventHandler(this.cmbMainStat6_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(10, 199);
			this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(17, 12);
			this.label6.TabIndex = 18;
			this.label6.Text = "陸";
			// 
			// cmbSubStat11
			// 
			this.cmbSubStat11.FormattingEnabled = true;
			this.cmbSubStat11.Location = new System.Drawing.Point(226, 83);
			this.cmbSubStat11.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat11.Name = "cmbSubStat11";
			this.cmbSubStat11.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat11.TabIndex = 21;
			// 
			// txtSubVal11
			// 
			this.txtSubVal11.Location = new System.Drawing.Point(321, 83);
			this.txtSubVal11.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal11.Name = "txtSubVal11";
			this.txtSubVal11.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal11.TabIndex = 22;
			// 
			// txtSubVal21
			// 
			this.txtSubVal21.Location = new System.Drawing.Point(512, 83);
			this.txtSubVal21.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal21.Name = "txtSubVal21";
			this.txtSubVal21.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal21.TabIndex = 24;
			// 
			// cmbSubStat21
			// 
			this.cmbSubStat21.FormattingEnabled = true;
			this.cmbSubStat21.Location = new System.Drawing.Point(417, 83);
			this.cmbSubStat21.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat21.Name = "cmbSubStat21";
			this.cmbSubStat21.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat21.TabIndex = 23;
			// 
			// txtSubVal31
			// 
			this.txtSubVal31.Location = new System.Drawing.Point(703, 83);
			this.txtSubVal31.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal31.Name = "txtSubVal31";
			this.txtSubVal31.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal31.TabIndex = 26;
			// 
			// cmbSubStat31
			// 
			this.cmbSubStat31.FormattingEnabled = true;
			this.cmbSubStat31.Location = new System.Drawing.Point(607, 83);
			this.cmbSubStat31.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat31.Name = "cmbSubStat31";
			this.cmbSubStat31.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat31.TabIndex = 25;
			// 
			// txtSubVal41
			// 
			this.txtSubVal41.Location = new System.Drawing.Point(893, 83);
			this.txtSubVal41.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal41.Name = "txtSubVal41";
			this.txtSubVal41.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal41.TabIndex = 28;
			// 
			// cmbSubStat41
			// 
			this.cmbSubStat41.FormattingEnabled = true;
			this.cmbSubStat41.Location = new System.Drawing.Point(798, 83);
			this.cmbSubStat41.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat41.Name = "cmbSubStat41";
			this.cmbSubStat41.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat41.TabIndex = 27;
			// 
			// txtSubVal42
			// 
			this.txtSubVal42.Location = new System.Drawing.Point(893, 106);
			this.txtSubVal42.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal42.Name = "txtSubVal42";
			this.txtSubVal42.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal42.TabIndex = 36;
			// 
			// cmbSubStat42
			// 
			this.cmbSubStat42.FormattingEnabled = true;
			this.cmbSubStat42.Location = new System.Drawing.Point(798, 106);
			this.cmbSubStat42.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat42.Name = "cmbSubStat42";
			this.cmbSubStat42.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat42.TabIndex = 35;
			// 
			// txtSubVal32
			// 
			this.txtSubVal32.Location = new System.Drawing.Point(703, 106);
			this.txtSubVal32.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal32.Name = "txtSubVal32";
			this.txtSubVal32.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal32.TabIndex = 34;
			// 
			// cmbSubStat32
			// 
			this.cmbSubStat32.FormattingEnabled = true;
			this.cmbSubStat32.Location = new System.Drawing.Point(607, 106);
			this.cmbSubStat32.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat32.Name = "cmbSubStat32";
			this.cmbSubStat32.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat32.TabIndex = 33;
			// 
			// txtSubVal22
			// 
			this.txtSubVal22.Location = new System.Drawing.Point(512, 106);
			this.txtSubVal22.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal22.Name = "txtSubVal22";
			this.txtSubVal22.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal22.TabIndex = 32;
			// 
			// cmbSubStat22
			// 
			this.cmbSubStat22.FormattingEnabled = true;
			this.cmbSubStat22.Location = new System.Drawing.Point(417, 106);
			this.cmbSubStat22.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat22.Name = "cmbSubStat22";
			this.cmbSubStat22.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat22.TabIndex = 31;
			// 
			// txtSubVal12
			// 
			this.txtSubVal12.Location = new System.Drawing.Point(321, 106);
			this.txtSubVal12.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal12.Name = "txtSubVal12";
			this.txtSubVal12.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal12.TabIndex = 30;
			// 
			// cmbSubStat12
			// 
			this.cmbSubStat12.FormattingEnabled = true;
			this.cmbSubStat12.Location = new System.Drawing.Point(226, 106);
			this.cmbSubStat12.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat12.Name = "cmbSubStat12";
			this.cmbSubStat12.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat12.TabIndex = 29;
			// 
			// txtSubVal43
			// 
			this.txtSubVal43.Location = new System.Drawing.Point(893, 128);
			this.txtSubVal43.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal43.Name = "txtSubVal43";
			this.txtSubVal43.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal43.TabIndex = 44;
			// 
			// cmbSubStat43
			// 
			this.cmbSubStat43.FormattingEnabled = true;
			this.cmbSubStat43.Location = new System.Drawing.Point(798, 128);
			this.cmbSubStat43.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat43.Name = "cmbSubStat43";
			this.cmbSubStat43.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat43.TabIndex = 43;
			// 
			// txtSubVal33
			// 
			this.txtSubVal33.Location = new System.Drawing.Point(703, 128);
			this.txtSubVal33.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal33.Name = "txtSubVal33";
			this.txtSubVal33.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal33.TabIndex = 42;
			// 
			// cmbSubStat33
			// 
			this.cmbSubStat33.FormattingEnabled = true;
			this.cmbSubStat33.Location = new System.Drawing.Point(607, 128);
			this.cmbSubStat33.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat33.Name = "cmbSubStat33";
			this.cmbSubStat33.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat33.TabIndex = 41;
			// 
			// txtSubVal23
			// 
			this.txtSubVal23.Location = new System.Drawing.Point(512, 128);
			this.txtSubVal23.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal23.Name = "txtSubVal23";
			this.txtSubVal23.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal23.TabIndex = 40;
			// 
			// cmbSubStat23
			// 
			this.cmbSubStat23.FormattingEnabled = true;
			this.cmbSubStat23.Location = new System.Drawing.Point(417, 128);
			this.cmbSubStat23.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat23.Name = "cmbSubStat23";
			this.cmbSubStat23.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat23.TabIndex = 39;
			// 
			// txtSubVal13
			// 
			this.txtSubVal13.Location = new System.Drawing.Point(321, 128);
			this.txtSubVal13.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal13.Name = "txtSubVal13";
			this.txtSubVal13.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal13.TabIndex = 38;
			// 
			// cmbSubStat13
			// 
			this.cmbSubStat13.FormattingEnabled = true;
			this.cmbSubStat13.Location = new System.Drawing.Point(226, 128);
			this.cmbSubStat13.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat13.Name = "cmbSubStat13";
			this.cmbSubStat13.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat13.TabIndex = 37;
			// 
			// txtSubVal44
			// 
			this.txtSubVal44.Location = new System.Drawing.Point(893, 151);
			this.txtSubVal44.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal44.Name = "txtSubVal44";
			this.txtSubVal44.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal44.TabIndex = 52;
			// 
			// cmbSubStat44
			// 
			this.cmbSubStat44.FormattingEnabled = true;
			this.cmbSubStat44.Location = new System.Drawing.Point(798, 151);
			this.cmbSubStat44.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat44.Name = "cmbSubStat44";
			this.cmbSubStat44.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat44.TabIndex = 51;
			// 
			// txtSubVal34
			// 
			this.txtSubVal34.Location = new System.Drawing.Point(703, 151);
			this.txtSubVal34.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal34.Name = "txtSubVal34";
			this.txtSubVal34.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal34.TabIndex = 50;
			// 
			// cmbSubStat34
			// 
			this.cmbSubStat34.FormattingEnabled = true;
			this.cmbSubStat34.Location = new System.Drawing.Point(607, 151);
			this.cmbSubStat34.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat34.Name = "cmbSubStat34";
			this.cmbSubStat34.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat34.TabIndex = 49;
			// 
			// txtSubVal24
			// 
			this.txtSubVal24.Location = new System.Drawing.Point(512, 151);
			this.txtSubVal24.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal24.Name = "txtSubVal24";
			this.txtSubVal24.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal24.TabIndex = 48;
			// 
			// cmbSubStat24
			// 
			this.cmbSubStat24.FormattingEnabled = true;
			this.cmbSubStat24.Location = new System.Drawing.Point(417, 151);
			this.cmbSubStat24.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat24.Name = "cmbSubStat24";
			this.cmbSubStat24.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat24.TabIndex = 47;
			// 
			// txtSubVal14
			// 
			this.txtSubVal14.Location = new System.Drawing.Point(321, 151);
			this.txtSubVal14.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal14.Name = "txtSubVal14";
			this.txtSubVal14.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal14.TabIndex = 46;
			// 
			// cmbSubStat14
			// 
			this.cmbSubStat14.FormattingEnabled = true;
			this.cmbSubStat14.Location = new System.Drawing.Point(226, 151);
			this.cmbSubStat14.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat14.Name = "cmbSubStat14";
			this.cmbSubStat14.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat14.TabIndex = 45;
			// 
			// txtSubVal45
			// 
			this.txtSubVal45.Location = new System.Drawing.Point(893, 174);
			this.txtSubVal45.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal45.Name = "txtSubVal45";
			this.txtSubVal45.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal45.TabIndex = 60;
			// 
			// cmbSubStat45
			// 
			this.cmbSubStat45.FormattingEnabled = true;
			this.cmbSubStat45.Location = new System.Drawing.Point(798, 174);
			this.cmbSubStat45.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat45.Name = "cmbSubStat45";
			this.cmbSubStat45.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat45.TabIndex = 59;
			// 
			// txtSubVal35
			// 
			this.txtSubVal35.Location = new System.Drawing.Point(703, 174);
			this.txtSubVal35.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal35.Name = "txtSubVal35";
			this.txtSubVal35.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal35.TabIndex = 58;
			// 
			// cmbSubStat35
			// 
			this.cmbSubStat35.FormattingEnabled = true;
			this.cmbSubStat35.Location = new System.Drawing.Point(607, 174);
			this.cmbSubStat35.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat35.Name = "cmbSubStat35";
			this.cmbSubStat35.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat35.TabIndex = 57;
			// 
			// txtSubVal25
			// 
			this.txtSubVal25.Location = new System.Drawing.Point(512, 174);
			this.txtSubVal25.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal25.Name = "txtSubVal25";
			this.txtSubVal25.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal25.TabIndex = 56;
			// 
			// cmbSubStat25
			// 
			this.cmbSubStat25.FormattingEnabled = true;
			this.cmbSubStat25.Location = new System.Drawing.Point(417, 174);
			this.cmbSubStat25.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat25.Name = "cmbSubStat25";
			this.cmbSubStat25.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat25.TabIndex = 55;
			// 
			// txtSubVal15
			// 
			this.txtSubVal15.Location = new System.Drawing.Point(321, 174);
			this.txtSubVal15.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal15.Name = "txtSubVal15";
			this.txtSubVal15.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal15.TabIndex = 54;
			// 
			// cmbSubStat15
			// 
			this.cmbSubStat15.FormattingEnabled = true;
			this.cmbSubStat15.Location = new System.Drawing.Point(226, 174);
			this.cmbSubStat15.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat15.Name = "cmbSubStat15";
			this.cmbSubStat15.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat15.TabIndex = 53;
			// 
			// txtSubVal46
			// 
			this.txtSubVal46.Location = new System.Drawing.Point(893, 195);
			this.txtSubVal46.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal46.Name = "txtSubVal46";
			this.txtSubVal46.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal46.TabIndex = 68;
			// 
			// cmbSubStat46
			// 
			this.cmbSubStat46.FormattingEnabled = true;
			this.cmbSubStat46.Location = new System.Drawing.Point(798, 195);
			this.cmbSubStat46.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat46.Name = "cmbSubStat46";
			this.cmbSubStat46.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat46.TabIndex = 67;
			// 
			// txtSubVal36
			// 
			this.txtSubVal36.Location = new System.Drawing.Point(703, 195);
			this.txtSubVal36.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal36.Name = "txtSubVal36";
			this.txtSubVal36.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal36.TabIndex = 66;
			// 
			// cmbSubStat36
			// 
			this.cmbSubStat36.FormattingEnabled = true;
			this.cmbSubStat36.Location = new System.Drawing.Point(607, 195);
			this.cmbSubStat36.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat36.Name = "cmbSubStat36";
			this.cmbSubStat36.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat36.TabIndex = 65;
			// 
			// txtSubVal26
			// 
			this.txtSubVal26.Location = new System.Drawing.Point(512, 195);
			this.txtSubVal26.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal26.Name = "txtSubVal26";
			this.txtSubVal26.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal26.TabIndex = 64;
			// 
			// cmbSubStat26
			// 
			this.cmbSubStat26.FormattingEnabled = true;
			this.cmbSubStat26.Location = new System.Drawing.Point(417, 195);
			this.cmbSubStat26.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat26.Name = "cmbSubStat26";
			this.cmbSubStat26.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat26.TabIndex = 63;
			// 
			// txtSubVal16
			// 
			this.txtSubVal16.Location = new System.Drawing.Point(321, 195);
			this.txtSubVal16.Margin = new System.Windows.Forms.Padding(2);
			this.txtSubVal16.Name = "txtSubVal16";
			this.txtSubVal16.Size = new System.Drawing.Size(92, 19);
			this.txtSubVal16.TabIndex = 62;
			// 
			// cmbSubStat16
			// 
			this.cmbSubStat16.FormattingEnabled = true;
			this.cmbSubStat16.Location = new System.Drawing.Point(226, 195);
			this.cmbSubStat16.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSubStat16.Name = "cmbSubStat16";
			this.cmbSubStat16.Size = new System.Drawing.Size(92, 20);
			this.cmbSubStat16.TabIndex = 61;
			// 
			// cmbSetBonus1
			// 
			this.cmbSetBonus1.FormattingEnabled = true;
			this.cmbSetBonus1.Location = new System.Drawing.Point(31, 232);
			this.cmbSetBonus1.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSetBonus1.Name = "cmbSetBonus1";
			this.cmbSetBonus1.Size = new System.Drawing.Size(92, 20);
			this.cmbSetBonus1.TabIndex = 69;
			// 
			// cmbSetBonus2
			// 
			this.cmbSetBonus2.FormattingEnabled = true;
			this.cmbSetBonus2.Location = new System.Drawing.Point(131, 232);
			this.cmbSetBonus2.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSetBonus2.Name = "cmbSetBonus2";
			this.cmbSetBonus2.Size = new System.Drawing.Size(92, 20);
			this.cmbSetBonus2.TabIndex = 70;
			// 
			// cmbSetBonus3
			// 
			this.cmbSetBonus3.FormattingEnabled = true;
			this.cmbSetBonus3.Location = new System.Drawing.Point(226, 232);
			this.cmbSetBonus3.Margin = new System.Windows.Forms.Padding(2);
			this.cmbSetBonus3.Name = "cmbSetBonus3";
			this.cmbSetBonus3.Size = new System.Drawing.Size(92, 20);
			this.cmbSetBonus3.TabIndex = 71;
			// 
			// cmbUnique1
			// 
			this.cmbUnique1.FormattingEnabled = true;
			this.cmbUnique1.Location = new System.Drawing.Point(321, 232);
			this.cmbUnique1.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique1.Name = "cmbUnique1";
			this.cmbUnique1.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique1.TabIndex = 72;
			// 
			// cmbUnique2
			// 
			this.cmbUnique2.FormattingEnabled = true;
			this.cmbUnique2.Location = new System.Drawing.Point(417, 232);
			this.cmbUnique2.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique2.Name = "cmbUnique2";
			this.cmbUnique2.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique2.TabIndex = 73;
			// 
			// cmbUnique3
			// 
			this.cmbUnique3.FormattingEnabled = true;
			this.cmbUnique3.Location = new System.Drawing.Point(512, 232);
			this.cmbUnique3.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique3.Name = "cmbUnique3";
			this.cmbUnique3.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique3.TabIndex = 74;
			// 
			// cmbUnique4
			// 
			this.cmbUnique4.FormattingEnabled = true;
			this.cmbUnique4.Location = new System.Drawing.Point(607, 232);
			this.cmbUnique4.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique4.Name = "cmbUnique4";
			this.cmbUnique4.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique4.TabIndex = 75;
			// 
			// cmbUnique5
			// 
			this.cmbUnique5.FormattingEnabled = true;
			this.cmbUnique5.Location = new System.Drawing.Point(703, 232);
			this.cmbUnique5.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique5.Name = "cmbUnique5";
			this.cmbUnique5.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique5.TabIndex = 76;
			// 
			// cmbUnique6
			// 
			this.cmbUnique6.FormattingEnabled = true;
			this.cmbUnique6.Location = new System.Drawing.Point(798, 232);
			this.cmbUnique6.Margin = new System.Windows.Forms.Padding(2);
			this.cmbUnique6.Name = "cmbUnique6";
			this.cmbUnique6.Size = new System.Drawing.Size(92, 20);
			this.cmbUnique6.TabIndex = 77;
			// 
			// txtMitamaOnly
			// 
			this.txtMitamaOnly.Location = new System.Drawing.Point(132, 267);
			this.txtMitamaOnly.Margin = new System.Windows.Forms.Padding(2);
			this.txtMitamaOnly.Name = "txtMitamaOnly";
			this.txtMitamaOnly.ReadOnly = true;
			this.txtMitamaOnly.Size = new System.Drawing.Size(923, 19);
			this.txtMitamaOnly.TabIndex = 78;
			// 
			// txtFinalStats
			// 
			this.txtFinalStats.Location = new System.Drawing.Point(132, 290);
			this.txtFinalStats.Margin = new System.Windows.Forms.Padding(2);
			this.txtFinalStats.Name = "txtFinalStats";
			this.txtFinalStats.ReadOnly = true;
			this.txtFinalStats.Size = new System.Drawing.Size(923, 19);
			this.txtFinalStats.TabIndex = 79;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(26, 270);
			this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(105, 12);
			this.label7.TabIndex = 80;
			this.label7.Text = "御魂のみのステータス";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(26, 292);
			this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(74, 12);
			this.label8.TabIndex = 81;
			this.label8.Text = "最終ステータス";
			// 
			// btnCalc
			// 
			this.btnCalc.Location = new System.Drawing.Point(132, 318);
			this.btnCalc.Margin = new System.Windows.Forms.Padding(2);
			this.btnCalc.Name = "btnCalc";
			this.btnCalc.Size = new System.Drawing.Size(91, 25);
			this.btnCalc.TabIndex = 82;
			this.btnCalc.Text = "計算";
			this.btnCalc.UseVisualStyleBackColor = true;
			this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(321, 318);
			this.btnSave.Margin = new System.Windows.Forms.Padding(2);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(91, 25);
			this.btnSave.TabIndex = 83;
			this.btnSave.Text = "保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(416, 318);
			this.btnLoad.Margin = new System.Windows.Forms.Padding(2);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(91, 25);
			this.btnLoad.TabIndex = 84;
			this.btnLoad.Text = "読み込み";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(34, 218);
			this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(60, 12);
			this.label9.TabIndex = 85;
			this.label9.Text = "2セット効果";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(324, 218);
			this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(53, 12);
			this.label10.TabIndex = 86;
			this.label10.Text = "固有効果";
			// 
			// btnReLoad
			// 
			this.btnReLoad.Location = new System.Drawing.Point(32, 53);
			this.btnReLoad.Margin = new System.Windows.Forms.Padding(2);
			this.btnReLoad.Name = "btnReLoad";
			this.btnReLoad.Size = new System.Drawing.Size(91, 25);
			this.btnReLoad.TabIndex = 87;
			this.btnReLoad.Text = "更新";
			this.btnReLoad.UseVisualStyleBackColor = true;
			this.btnReLoad.Click += new System.EventHandler(this.btnReLoad_Click);
			// 
			// btnAddShikigami
			// 
			this.btnAddShikigami.Location = new System.Drawing.Point(132, 53);
			this.btnAddShikigami.Name = "btnAddShikigami";
			this.btnAddShikigami.Size = new System.Drawing.Size(91, 25);
			this.btnAddShikigami.TabIndex = 88;
			this.btnAddShikigami.Text = "式神登録";
			this.btnAddShikigami.UseVisualStyleBackColor = true;
			this.btnAddShikigami.Click += new System.EventHandler(this.btnAddShikigami_Click);
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(606, 318);
			this.btnClear.Margin = new System.Windows.Forms.Padding(2);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(91, 25);
			this.btnClear.TabIndex = 89;
			this.btnClear.Text = "クリア";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// btnResultView
			// 
			this.btnResultView.Location = new System.Drawing.Point(226, 318);
			this.btnResultView.Margin = new System.Windows.Forms.Padding(2);
			this.btnResultView.Name = "btnResultView";
			this.btnResultView.Size = new System.Drawing.Size(91, 25);
			this.btnResultView.TabIndex = 90;
			this.btnResultView.Text = "結果表示";
			this.btnResultView.UseVisualStyleBackColor = true;
			this.btnResultView.Click += new System.EventHandler(this.btnResultView_Click);
			// 
			// btnEditShikigami
			// 
			this.btnEditShikigami.Location = new System.Drawing.Point(226, 53);
			this.btnEditShikigami.Name = "btnEditShikigami";
			this.btnEditShikigami.Size = new System.Drawing.Size(91, 25);
			this.btnEditShikigami.TabIndex = 91;
			this.btnEditShikigami.Text = "式神編集";
			this.btnEditShikigami.UseVisualStyleBackColor = true;
			this.btnEditShikigami.Click += new System.EventHandler(this.btnEditShikigami_Click);
			// 
			// btnCompareResult
			// 
			this.btnCompareResult.Location = new System.Drawing.Point(511, 318);
			this.btnCompareResult.Margin = new System.Windows.Forms.Padding(2);
			this.btnCompareResult.Name = "btnCompareResult";
			this.btnCompareResult.Size = new System.Drawing.Size(91, 25);
			this.btnCompareResult.TabIndex = 92;
			this.btnCompareResult.Text = "結果比較";
			this.btnCompareResult.UseVisualStyleBackColor = true;
			this.btnCompareResult.Click += new System.EventHandler(this.btnCompareResult_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1066, 352);
			this.Controls.Add(this.btnCompareResult);
			this.Controls.Add(this.btnEditShikigami);
			this.Controls.Add(this.btnResultView);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnAddShikigami);
			this.Controls.Add(this.btnReLoad);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.btnCalc);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.txtFinalStats);
			this.Controls.Add(this.txtMitamaOnly);
			this.Controls.Add(this.cmbUnique6);
			this.Controls.Add(this.cmbUnique5);
			this.Controls.Add(this.cmbUnique4);
			this.Controls.Add(this.cmbUnique3);
			this.Controls.Add(this.cmbUnique2);
			this.Controls.Add(this.cmbUnique1);
			this.Controls.Add(this.cmbSetBonus3);
			this.Controls.Add(this.cmbSetBonus2);
			this.Controls.Add(this.cmbSetBonus1);
			this.Controls.Add(this.txtSubVal46);
			this.Controls.Add(this.cmbSubStat46);
			this.Controls.Add(this.txtSubVal36);
			this.Controls.Add(this.cmbSubStat36);
			this.Controls.Add(this.txtSubVal26);
			this.Controls.Add(this.cmbSubStat26);
			this.Controls.Add(this.txtSubVal16);
			this.Controls.Add(this.cmbSubStat16);
			this.Controls.Add(this.txtSubVal45);
			this.Controls.Add(this.cmbSubStat45);
			this.Controls.Add(this.txtSubVal35);
			this.Controls.Add(this.cmbSubStat35);
			this.Controls.Add(this.txtSubVal25);
			this.Controls.Add(this.cmbSubStat25);
			this.Controls.Add(this.txtSubVal15);
			this.Controls.Add(this.cmbSubStat15);
			this.Controls.Add(this.txtSubVal44);
			this.Controls.Add(this.cmbSubStat44);
			this.Controls.Add(this.txtSubVal34);
			this.Controls.Add(this.cmbSubStat34);
			this.Controls.Add(this.txtSubVal24);
			this.Controls.Add(this.cmbSubStat24);
			this.Controls.Add(this.txtSubVal14);
			this.Controls.Add(this.cmbSubStat14);
			this.Controls.Add(this.txtSubVal43);
			this.Controls.Add(this.cmbSubStat43);
			this.Controls.Add(this.txtSubVal33);
			this.Controls.Add(this.cmbSubStat33);
			this.Controls.Add(this.txtSubVal23);
			this.Controls.Add(this.cmbSubStat23);
			this.Controls.Add(this.txtSubVal13);
			this.Controls.Add(this.cmbSubStat13);
			this.Controls.Add(this.txtSubVal42);
			this.Controls.Add(this.cmbSubStat42);
			this.Controls.Add(this.txtSubVal32);
			this.Controls.Add(this.cmbSubStat32);
			this.Controls.Add(this.txtSubVal22);
			this.Controls.Add(this.cmbSubStat22);
			this.Controls.Add(this.txtSubVal12);
			this.Controls.Add(this.cmbSubStat12);
			this.Controls.Add(this.txtSubVal41);
			this.Controls.Add(this.cmbSubStat41);
			this.Controls.Add(this.txtSubVal31);
			this.Controls.Add(this.cmbSubStat31);
			this.Controls.Add(this.txtSubVal21);
			this.Controls.Add(this.cmbSubStat21);
			this.Controls.Add(this.txtSubVal11);
			this.Controls.Add(this.cmbSubStat11);
			this.Controls.Add(this.txtMainVal6);
			this.Controls.Add(this.cmbMainStat6);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtMainVal5);
			this.Controls.Add(this.cmbMainStat5);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtMainVal4);
			this.Controls.Add(this.cmbMainStat4);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtMainVal3);
			this.Controls.Add(this.cmbMainStat3);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtMainVal2);
			this.Controls.Add(this.cmbMainStat2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtMainVal1);
			this.Controls.Add(this.cmbMainStat1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtBaseStats);
			this.Controls.Add(this.cmbShikigami);
			this.Controls.Add(this.lblShikigami);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "式神ステータス計算";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblShikigami;
		private System.Windows.Forms.ComboBox cmbShikigami;
		private System.Windows.Forms.TextBox txtBaseStats;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbMainStat1;
		private System.Windows.Forms.TextBox txtMainVal1;
		private System.Windows.Forms.TextBox txtMainVal2;
		private System.Windows.Forms.ComboBox cmbMainStat2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtMainVal3;
		private System.Windows.Forms.ComboBox cmbMainStat3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtMainVal4;
		private System.Windows.Forms.ComboBox cmbMainStat4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtMainVal5;
		private System.Windows.Forms.ComboBox cmbMainStat5;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtMainVal6;
		private System.Windows.Forms.ComboBox cmbMainStat6;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cmbSubStat11;
		private System.Windows.Forms.TextBox txtSubVal11;
		private System.Windows.Forms.TextBox txtSubVal21;
		private System.Windows.Forms.ComboBox cmbSubStat21;
		private System.Windows.Forms.TextBox txtSubVal31;
		private System.Windows.Forms.ComboBox cmbSubStat31;
		private System.Windows.Forms.TextBox txtSubVal41;
		private System.Windows.Forms.ComboBox cmbSubStat41;
		private System.Windows.Forms.TextBox txtSubVal42;
		private System.Windows.Forms.ComboBox cmbSubStat42;
		private System.Windows.Forms.TextBox txtSubVal32;
		private System.Windows.Forms.ComboBox cmbSubStat32;
		private System.Windows.Forms.TextBox txtSubVal22;
		private System.Windows.Forms.ComboBox cmbSubStat22;
		private System.Windows.Forms.TextBox txtSubVal12;
		private System.Windows.Forms.ComboBox cmbSubStat12;
		private System.Windows.Forms.TextBox txtSubVal43;
		private System.Windows.Forms.ComboBox cmbSubStat43;
		private System.Windows.Forms.TextBox txtSubVal33;
		private System.Windows.Forms.ComboBox cmbSubStat33;
		private System.Windows.Forms.TextBox txtSubVal23;
		private System.Windows.Forms.ComboBox cmbSubStat23;
		private System.Windows.Forms.TextBox txtSubVal13;
		private System.Windows.Forms.ComboBox cmbSubStat13;
		private System.Windows.Forms.TextBox txtSubVal44;
		private System.Windows.Forms.ComboBox cmbSubStat44;
		private System.Windows.Forms.TextBox txtSubVal34;
		private System.Windows.Forms.ComboBox cmbSubStat34;
		private System.Windows.Forms.TextBox txtSubVal24;
		private System.Windows.Forms.ComboBox cmbSubStat24;
		private System.Windows.Forms.TextBox txtSubVal14;
		private System.Windows.Forms.ComboBox cmbSubStat14;
		private System.Windows.Forms.TextBox txtSubVal45;
		private System.Windows.Forms.ComboBox cmbSubStat45;
		private System.Windows.Forms.TextBox txtSubVal35;
		private System.Windows.Forms.ComboBox cmbSubStat35;
		private System.Windows.Forms.TextBox txtSubVal25;
		private System.Windows.Forms.ComboBox cmbSubStat25;
		private System.Windows.Forms.TextBox txtSubVal15;
		private System.Windows.Forms.ComboBox cmbSubStat15;
		private System.Windows.Forms.TextBox txtSubVal46;
		private System.Windows.Forms.ComboBox cmbSubStat46;
		private System.Windows.Forms.TextBox txtSubVal36;
		private System.Windows.Forms.ComboBox cmbSubStat36;
		private System.Windows.Forms.TextBox txtSubVal26;
		private System.Windows.Forms.ComboBox cmbSubStat26;
		private System.Windows.Forms.TextBox txtSubVal16;
		private System.Windows.Forms.ComboBox cmbSubStat16;
		private System.Windows.Forms.ComboBox cmbSetBonus1;
		private System.Windows.Forms.ComboBox cmbSetBonus2;
		private System.Windows.Forms.ComboBox cmbSetBonus3;
		private System.Windows.Forms.ComboBox cmbUnique1;
		private System.Windows.Forms.ComboBox cmbUnique2;
		private System.Windows.Forms.ComboBox cmbUnique3;
		private System.Windows.Forms.ComboBox cmbUnique4;
		private System.Windows.Forms.ComboBox cmbUnique5;
		private System.Windows.Forms.ComboBox cmbUnique6;
		private System.Windows.Forms.TextBox txtMitamaOnly;
		private System.Windows.Forms.TextBox txtFinalStats;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnCalc;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnLoad;
		private Label label9;
		private Label label10;
		private Button btnReLoad;
		private Button btnAddShikigami;
		private Button btnClear;
		private Button btnResultView;
		private Button btnEditShikigami;
		private Button btnCompareResult;
	}
}

