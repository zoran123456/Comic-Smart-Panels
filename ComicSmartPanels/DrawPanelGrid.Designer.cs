namespace ComicSmartPanels
{
	partial class DrawPanelGrid
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
			this.tbMargin = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbRows = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbColumns = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.cbManga = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbManga);
			this.groupBox1.Controls.Add(this.tbMargin);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.tbRows);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.tbColumns);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(347, 191);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Properties";
			// 
			// tbMargin
			// 
			this.tbMargin.Location = new System.Drawing.Point(111, 105);
			this.tbMargin.Name = "tbMargin";
			this.tbMargin.Size = new System.Drawing.Size(138, 22);
			this.tbMargin.TabIndex = 5;
			this.tbMargin.Text = "3";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(99, 14);
			this.label3.TabIndex = 4;
			this.label3.Text = "Panel margin (%)";
			// 
			// tbRows
			// 
			this.tbRows.Location = new System.Drawing.Point(111, 55);
			this.tbRows.Name = "tbRows";
			this.tbRows.Size = new System.Drawing.Size(138, 22);
			this.tbRows.TabIndex = 3;
			this.tbRows.Text = "3";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(36, 14);
			this.label2.TabIndex = 2;
			this.label2.Text = "Rows";
			// 
			// tbColumns
			// 
			this.tbColumns.Location = new System.Drawing.Point(111, 22);
			this.tbColumns.Name = "tbColumns";
			this.tbColumns.Size = new System.Drawing.Size(138, 22);
			this.tbColumns.TabIndex = 1;
			this.tbColumns.Text = "1";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 14);
			this.label1.TabIndex = 0;
			this.label1.Text = "Columns";
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(203, 232);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(284, 232);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// cbManga
			// 
			this.cbManga.AutoSize = true;
			this.cbManga.Location = new System.Drawing.Point(9, 167);
			this.cbManga.Name = "cbManga";
			this.cbManga.Size = new System.Drawing.Size(204, 18);
			this.cbManga.TabIndex = 6;
			this.cbManga.Text = "Right to left (Manga Page Layout)";
			this.cbManga.UseVisualStyleBackColor = true;
			// 
			// DrawPanelGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(371, 267);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DrawPanelGrid";
			this.Text = "Draw Panel Grid";
			this.Load += new System.EventHandler(this.DrawPanelGrid_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbRows;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbColumns;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbMargin;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.CheckBox cbManga;
	}
}