namespace WFmServerApplicationTest
{
    partial class Form1
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
            this.btnGetDestinationList = new System.Windows.Forms.Button();
            this.btnIsServerReady = new System.Windows.Forms.Button();
            this.btnReLogon = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGetDestinationList
            // 
            this.btnGetDestinationList.Location = new System.Drawing.Point(12, 74);
            this.btnGetDestinationList.Name = "btnGetDestinationList";
            this.btnGetDestinationList.Size = new System.Drawing.Size(283, 23);
            this.btnGetDestinationList.TabIndex = 0;
            this.btnGetDestinationList.Text = "Get Destination List (Base On Melaka Sentral Plan)";
            this.btnGetDestinationList.UseVisualStyleBackColor = true;
            this.btnGetDestinationList.Click += new System.EventHandler(this.btnGetDestinationList_Click);
            // 
            // btnIsServerReady
            // 
            this.btnIsServerReady.Location = new System.Drawing.Point(12, 12);
            this.btnIsServerReady.Name = "btnIsServerReady";
            this.btnIsServerReady.Size = new System.Drawing.Size(193, 23);
            this.btnIsServerReady.TabIndex = 1;
            this.btnIsServerReady.Text = "Is Server Ready ?";
            this.btnIsServerReady.UseVisualStyleBackColor = true;
            this.btnIsServerReady.Click += new System.EventHandler(this.btnIsServerReady_Click);
            // 
            // btnReLogon
            // 
            this.btnReLogon.Location = new System.Drawing.Point(12, 41);
            this.btnReLogon.Name = "btnReLogon";
            this.btnReLogon.Size = new System.Drawing.Size(283, 23);
            this.btnReLogon.TabIndex = 2;
            this.btnReLogon.Text = "Re-logon";
            this.btnReLogon.UseVisualStyleBackColor = true;
            this.btnReLogon.Click += new System.EventHandler(this.btnReLogon_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 450);
            this.Controls.Add(this.btnReLogon);
            this.Controls.Add(this.btnIsServerReady);
            this.Controls.Add(this.btnGetDestinationList);
            this.Name = "Form1";
            this.Text = "Kiosk Server Appliaction Testing";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGetDestinationList;
        private System.Windows.Forms.Button btnIsServerReady;
        private System.Windows.Forms.Button btnReLogon;
    }
}

