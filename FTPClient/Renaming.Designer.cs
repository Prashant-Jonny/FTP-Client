namespace FTPClient
{
    partial class Renaming
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Renaming));
            this.newNameBttn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.newNameTB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // newNameBttn
            // 
            this.newNameBttn.Location = new System.Drawing.Point(105, 36);
            this.newNameBttn.Name = "newNameBttn";
            this.newNameBttn.Size = new System.Drawing.Size(75, 23);
            this.newNameBttn.TabIndex = 0;
            this.newNameBttn.Text = "Confirm";
            this.newNameBttn.UseVisualStyleBackColor = true;
            this.newNameBttn.Click += new System.EventHandler(this.newNameBttn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "New name:";
            // 
            // newNameTB
            // 
            this.newNameTB.Location = new System.Drawing.Point(80, 10);
            this.newNameTB.Name = "newNameTB";
            this.newNameTB.Size = new System.Drawing.Size(192, 20);
            this.newNameTB.TabIndex = 2;
            // 
            // Renaming
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 65);
            this.Controls.Add(this.newNameTB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.newNameBttn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Renaming";
            this.Text = "Renaming";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button newNameBttn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox newNameTB;
    }
}