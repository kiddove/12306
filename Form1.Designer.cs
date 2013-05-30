namespace _12306
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
            this.buttonLogin = new System.Windows.Forms.Button();
            this.pictureBoxLogin = new System.Windows.Forms.PictureBox();
            this.buttonLoginRand = new System.Windows.Forms.Button();
            this.textBoxLoginCode = new System.Windows.Forms.TextBox();
            this.pictureBoxOrder = new System.Windows.Forms.PictureBox();
            this.textBoxOrderCode = new System.Windows.Forms.TextBox();
            this.buttonOrderRand = new System.Windows.Forms.Button();
            this.buttonOrder = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrder)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonLogin
            // 
            this.buttonLogin.Location = new System.Drawing.Point(23, 44);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(75, 23);
            this.buttonLogin.TabIndex = 0;
            this.buttonLogin.Text = "Login";
            this.buttonLogin.UseVisualStyleBackColor = true;
            this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
            // 
            // pictureBoxLogin
            // 
            this.pictureBoxLogin.Location = new System.Drawing.Point(116, 44);
            this.pictureBoxLogin.Name = "pictureBoxLogin";
            this.pictureBoxLogin.Size = new System.Drawing.Size(74, 23);
            this.pictureBoxLogin.TabIndex = 1;
            this.pictureBoxLogin.TabStop = false;
            // 
            // buttonLoginRand
            // 
            this.buttonLoginRand.Location = new System.Drawing.Point(352, 44);
            this.buttonLoginRand.Name = "buttonLoginRand";
            this.buttonLoginRand.Size = new System.Drawing.Size(75, 23);
            this.buttonLoginRand.TabIndex = 2;
            this.buttonLoginRand.Text = "刷新";
            this.buttonLoginRand.UseVisualStyleBackColor = true;
            this.buttonLoginRand.Click += new System.EventHandler(this.buttonLoginRand_Click);
            // 
            // textBoxLoginCode
            // 
            this.textBoxLoginCode.Location = new System.Drawing.Point(228, 44);
            this.textBoxLoginCode.Name = "textBoxLoginCode";
            this.textBoxLoginCode.Size = new System.Drawing.Size(100, 21);
            this.textBoxLoginCode.TabIndex = 3;
            // 
            // pictureBoxOrder
            // 
            this.pictureBoxOrder.Location = new System.Drawing.Point(116, 109);
            this.pictureBoxOrder.Name = "pictureBoxOrder";
            this.pictureBoxOrder.Size = new System.Drawing.Size(74, 23);
            this.pictureBoxOrder.TabIndex = 4;
            this.pictureBoxOrder.TabStop = false;
            // 
            // textBoxOrderCode
            // 
            this.textBoxOrderCode.Location = new System.Drawing.Point(228, 110);
            this.textBoxOrderCode.Name = "textBoxOrderCode";
            this.textBoxOrderCode.Size = new System.Drawing.Size(100, 21);
            this.textBoxOrderCode.TabIndex = 5;
            // 
            // buttonOrderRand
            // 
            this.buttonOrderRand.Enabled = false;
            this.buttonOrderRand.Location = new System.Drawing.Point(352, 109);
            this.buttonOrderRand.Name = "buttonOrderRand";
            this.buttonOrderRand.Size = new System.Drawing.Size(75, 23);
            this.buttonOrderRand.TabIndex = 6;
            this.buttonOrderRand.Text = "刷新";
            this.buttonOrderRand.UseVisualStyleBackColor = true;
            this.buttonOrderRand.Click += new System.EventHandler(this.buttonOrderRand_Click);
            // 
            // buttonOrder
            // 
            this.buttonOrder.Location = new System.Drawing.Point(23, 109);
            this.buttonOrder.Name = "buttonOrder";
            this.buttonOrder.Size = new System.Drawing.Size(75, 23);
            this.buttonOrder.TabIndex = 7;
            this.buttonOrder.Text = "Order";
            this.buttonOrder.UseVisualStyleBackColor = true;
            this.buttonOrder.Click += new System.EventHandler(this.buttonOrder_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 266);
            this.Controls.Add(this.buttonOrder);
            this.Controls.Add(this.buttonOrderRand);
            this.Controls.Add(this.textBoxOrderCode);
            this.Controls.Add(this.pictureBoxOrder);
            this.Controls.Add(this.textBoxLoginCode);
            this.Controls.Add(this.buttonLoginRand);
            this.Controls.Add(this.pictureBoxLogin);
            this.Controls.Add(this.buttonLogin);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.PictureBox pictureBoxLogin;
        private System.Windows.Forms.Button buttonLoginRand;
        private System.Windows.Forms.TextBox textBoxLoginCode;
        private System.Windows.Forms.PictureBox pictureBoxOrder;
        private System.Windows.Forms.TextBox textBoxOrderCode;
        private System.Windows.Forms.Button buttonOrderRand;
        private System.Windows.Forms.Button buttonOrder;
    }
}

