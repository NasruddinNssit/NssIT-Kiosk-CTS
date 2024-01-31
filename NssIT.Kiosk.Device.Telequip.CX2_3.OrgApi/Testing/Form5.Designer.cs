namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	partial class Form5
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
			this.btnTest = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnTest
			// 
			this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnTest.Location = new System.Drawing.Point(12, 12);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(388, 103);
			this.btnTest.TabIndex = 1;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// Form5
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(412, 127);
			this.Controls.Add(this.btnTest);
			this.Name = "Form5";
			this.Text = "Form5 - NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnTest;
	}
}

