namespace CoreTestDataConverter.Form
{
	partial class CalculationTestDataForm
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
			this.btnGenerate = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtTestSourcePath = new System.Windows.Forms.TextBox();
			this.txtMitamaCalculatorOutputPath = new System.Windows.Forms.TextBox();
			this.txtStatusCalculatorOutputPath = new System.Windows.Forms.TextBox();
			this.btnBrowseTestSource = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(429, 185);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(90, 23);
			this.btnGenerate.TabIndex = 0;
			this.btnGenerate.Text = "TestData生成";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(113, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "Calculation TestData";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 33);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "TestSource";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 12);
			this.label3.TabIndex = 3;
			this.label3.Text = "出力先";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 98);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(94, 12);
			this.label4.TabIndex = 4;
			this.label4.Text = "MitamaCalculator";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 145);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 12);
			this.label5.TabIndex = 5;
			this.label5.Text = "StatusCalculator";
			// 
			// txtTestSourcePath
			// 
			this.txtTestSourcePath.Location = new System.Drawing.Point(8, 48);
			this.txtTestSourcePath.Name = "txtTestSourcePath";
			this.txtTestSourcePath.ReadOnly = true;
			this.txtTestSourcePath.Size = new System.Drawing.Size(421, 19);
			this.txtTestSourcePath.TabIndex = 6;
			// 
			// txtMitamaCalculatorOut
			// 
			this.txtMitamaCalculatorOutputPath.Location = new System.Drawing.Point(8, 113);
			this.txtMitamaCalculatorOutputPath.Name = "txtMitamaCalculatorOut";
			this.txtMitamaCalculatorOutputPath.ReadOnly = true;
			this.txtMitamaCalculatorOutputPath.Size = new System.Drawing.Size(421, 19);
			this.txtMitamaCalculatorOutputPath.TabIndex = 7;
			// 
			// txtStatusCalculatorOutputPath
			// 
			this.txtStatusCalculatorOutputPath.Location = new System.Drawing.Point(8, 160);
			this.txtStatusCalculatorOutputPath.Name = "txtStatusCalculatorOutputPath";
			this.txtStatusCalculatorOutputPath.ReadOnly = true;
			this.txtStatusCalculatorOutputPath.Size = new System.Drawing.Size(421, 19);
			this.txtStatusCalculatorOutputPath.TabIndex = 8;
			// 
			// btnBrowseTestSource
			// 
			this.btnBrowseTestSource.Location = new System.Drawing.Point(435, 48);
			this.btnBrowseTestSource.Name = "btnBrowseTestSource";
			this.btnBrowseTestSource.Size = new System.Drawing.Size(84, 19);
			this.btnBrowseTestSource.TabIndex = 9;
			this.btnBrowseTestSource.Text = "参照";
			this.btnBrowseTestSource.UseVisualStyleBackColor = true;
			this.btnBrowseTestSource.Click += new System.EventHandler(this.btnBrowseTestSource_Click);
			// 
			// CalculationTestDataForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 215);
			this.Controls.Add(this.btnBrowseTestSource);
			this.Controls.Add(this.txtStatusCalculatorOutputPath);
			this.Controls.Add(this.txtMitamaCalculatorOutputPath);
			this.Controls.Add(this.txtTestSourcePath);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnGenerate);
			this.Name = "CalculationTestDataForm";
			this.Text = "CalculationTestDataForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtTestSourcePath;
		private System.Windows.Forms.TextBox txtMitamaCalculatorOutputPath;
		private System.Windows.Forms.TextBox txtStatusCalculatorOutputPath;
		private System.Windows.Forms.Button btnBrowseTestSource;
	}
}
