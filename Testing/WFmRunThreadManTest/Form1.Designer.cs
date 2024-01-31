
namespace WFmRunThreadManTest
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
            this.btnTest1 = new System.Windows.Forms.Button();
            this.btnShowNoOfRunThreadMan = new System.Windows.Forms.Button();
            this.btnClearRunThreadManList = new System.Windows.Forms.Button();
            this.btnAbortTesting = new System.Windows.Forms.Button();
            this.btnWaitToEnd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTest1
            // 
            this.btnTest1.Location = new System.Drawing.Point(12, 12);
            this.btnTest1.Name = "btnTest1";
            this.btnTest1.Size = new System.Drawing.Size(119, 40);
            this.btnTest1.TabIndex = 0;
            this.btnTest1.Text = "Test1";
            this.btnTest1.UseVisualStyleBackColor = true;
            this.btnTest1.Click += new System.EventHandler(this.btnTest1_Click);
            // 
            // btnShowNoOfRunThreadMan
            // 
            this.btnShowNoOfRunThreadMan.Location = new System.Drawing.Point(137, 12);
            this.btnShowNoOfRunThreadMan.Name = "btnShowNoOfRunThreadMan";
            this.btnShowNoOfRunThreadMan.Size = new System.Drawing.Size(119, 40);
            this.btnShowNoOfRunThreadMan.TabIndex = 1;
            this.btnShowNoOfRunThreadMan.Text = "Show Number of RunThreadMan";
            this.btnShowNoOfRunThreadMan.UseVisualStyleBackColor = true;
            this.btnShowNoOfRunThreadMan.Click += new System.EventHandler(this.btnShowNoOfRunThreadMan_Click);
            // 
            // btnClearRunThreadManList
            // 
            this.btnClearRunThreadManList.Location = new System.Drawing.Point(12, 58);
            this.btnClearRunThreadManList.Name = "btnClearRunThreadManList";
            this.btnClearRunThreadManList.Size = new System.Drawing.Size(494, 40);
            this.btnClearRunThreadManList.TabIndex = 2;
            this.btnClearRunThreadManList.Text = "Clear RunThreadMan List";
            this.btnClearRunThreadManList.UseVisualStyleBackColor = true;
            this.btnClearRunThreadManList.Click += new System.EventHandler(this.btnClearRunThreadManList_Click);
            // 
            // btnAbortTesting
            // 
            this.btnAbortTesting.Location = new System.Drawing.Point(387, 12);
            this.btnAbortTesting.Name = "btnAbortTesting";
            this.btnAbortTesting.Size = new System.Drawing.Size(119, 40);
            this.btnAbortTesting.TabIndex = 3;
            this.btnAbortTesting.Text = "Abort Test partial RunThreadMan List";
            this.btnAbortTesting.UseVisualStyleBackColor = true;
            this.btnAbortTesting.Click += new System.EventHandler(this.btnAbortTesting_Click);
            // 
            // btnWaitToEnd
            // 
            this.btnWaitToEnd.Location = new System.Drawing.Point(262, 12);
            this.btnWaitToEnd.Name = "btnWaitToEnd";
            this.btnWaitToEnd.Size = new System.Drawing.Size(119, 40);
            this.btnWaitToEnd.TabIndex = 4;
            this.btnWaitToEnd.Text = "WaitToEnd partial RunThreadMan List";
            this.btnWaitToEnd.UseVisualStyleBackColor = true;
            this.btnWaitToEnd.Click += new System.EventHandler(this.btnWaitToEnd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 166);
            this.Controls.Add(this.btnWaitToEnd);
            this.Controls.Add(this.btnAbortTesting);
            this.Controls.Add(this.btnClearRunThreadManList);
            this.Controls.Add(this.btnShowNoOfRunThreadMan);
            this.Controls.Add(this.btnTest1);
            this.Name = "Form1";
            this.Text = "WFmRunThreadManTest - Form 1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTest1;
        private System.Windows.Forms.Button btnShowNoOfRunThreadMan;
        private System.Windows.Forms.Button btnClearRunThreadManList;
        private System.Windows.Forms.Button btnAbortTesting;
        private System.Windows.Forms.Button btnWaitToEnd;
    }
}

