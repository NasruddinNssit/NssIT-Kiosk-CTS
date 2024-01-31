namespace NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi
{
	partial class AxCX2_3
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
			CloseDevice();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxCX2_3));
			this.axTQ01001 = new AxTQ0100Lib.AxTQ0100();
			((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).BeginInit();
			this.SuspendLayout();
			// 
			// axTQ01001
			// 
			this.axTQ01001.Enabled = true;
			this.axTQ01001.Location = new System.Drawing.Point(12, 12);
			this.axTQ01001.Name = "axTQ01001";
			this.axTQ01001.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axTQ01001.OcxState")));
			this.axTQ01001.Size = new System.Drawing.Size(100, 50);
			this.axTQ01001.TabIndex = 0;
			// 
			// AxCX2_3
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(372, 74);
			this.Controls.Add(this.axTQ01001);
			this.Name = "AxCX2_3";
			this.Text = "AxCX2_3 - NssIT.Kiosk.Device.Telequip.CX2_3.OrgApi";
			((System.ComponentModel.ISupportInitialize)(this.axTQ01001)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private AxTQ0100Lib.AxTQ0100 axTQ01001;
	}
}