namespace serialCommunicationECR2
{
    partial class frmPayWave
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
            this.components = new System.ComponentModel.Container();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblPrice = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tmrDelay = new System.Windows.Forms.Timer(this.components);
            this.txtMacBusy = new System.Windows.Forms.TextBox();
            this.lblProcessTechMsg = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Panel();
            this.txtError = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimer.ForeColor = System.Drawing.Color.Red;
            this.lblTimer.Location = new System.Drawing.Point(395, 270);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(82, 29);
            this.lblTimer.TabIndex = 57;
            this.lblTimer.Text = "Timer";
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrice.ForeColor = System.Drawing.Color.Green;
            this.lblPrice.Location = new System.Drawing.Point(317, 129);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(78, 32);
            this.lblPrice.TabIndex = 56;
            this.lblPrice.Text = "label";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Green;
            this.label3.Location = new System.Drawing.Point(286, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 32);
            this.label3.TabIndex = 55;
            this.label3.Text = ":";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Green;
            this.label1.Location = new System.Drawing.Point(127, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 32);
            this.label1.TabIndex = 54;
            this.label1.Text = "Please Pay";
            // 
            // tmrDelay
            // 
            this.tmrDelay.Interval = 1000;
            // 
            // txtMacBusy
            // 
            this.txtMacBusy.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMacBusy.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMacBusy.ForeColor = System.Drawing.Color.Blue;
            this.txtMacBusy.Location = new System.Drawing.Point(130, 169);
            this.txtMacBusy.Margin = new System.Windows.Forms.Padding(8);
            this.txtMacBusy.Multiline = true;
            this.txtMacBusy.Name = "txtMacBusy";
            this.txtMacBusy.Size = new System.Drawing.Size(385, 62);
            this.txtMacBusy.TabIndex = 62;
            this.txtMacBusy.Text = "Error\r\nReason";
            // 
            // lblProcessTechMsg
            // 
            this.lblProcessTechMsg.AutoSize = true;
            this.lblProcessTechMsg.Font = new System.Drawing.Font("Arial", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProcessTechMsg.ForeColor = System.Drawing.Color.Black;
            this.lblProcessTechMsg.Location = new System.Drawing.Point(8, 326);
            this.lblProcessTechMsg.Name = "lblProcessTechMsg";
            this.lblProcessTechMsg.Size = new System.Drawing.Size(112, 10);
            this.lblProcessTechMsg.TabIndex = 61;
            this.lblProcessTechMsg.Text = "Machine is busy ... Please wait";
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImage = global::serialCommunicationECR2.Properties.Resources.B1_Cancel;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnCancel.Location = new System.Drawing.Point(130, 242);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(259, 76);
            this.btnCancel.TabIndex = 60;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.Paint += new System.Windows.Forms.PaintEventHandler(this.btnCancel_Paint);
            // 
            // txtError
            // 
            this.txtError.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtError.ForeColor = System.Drawing.Color.Red;
            this.txtError.Location = new System.Drawing.Point(132, 169);
            this.txtError.Margin = new System.Windows.Forms.Padding(8);
            this.txtError.Multiline = true;
            this.txtError.Name = "txtError";
            this.txtError.Size = new System.Drawing.Size(385, 62);
            this.txtError.TabIndex = 59;
            this.txtError.Text = "Error\r\nReason";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::serialCommunicationECR2.Properties.Resources.PayWave3B;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(287, 92);
            this.panel1.TabIndex = 58;
            // 
            // frmPayWave
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 355);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtMacBusy);
            this.Controls.Add(this.lblProcessTechMsg);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtError);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmPayWave";
            this.Text = "frmPayWave";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPayWave_FormClosing);
            this.Load += new System.EventHandler(this.frmPayWave_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer tmrDelay;
        private System.Windows.Forms.TextBox txtMacBusy;
        private System.Windows.Forms.Label lblProcessTechMsg;
        private System.Windows.Forms.Panel btnCancel;
        private System.Windows.Forms.TextBox txtError;
        private System.Windows.Forms.Panel panel1;
    }
}