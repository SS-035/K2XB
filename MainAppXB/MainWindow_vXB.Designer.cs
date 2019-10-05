namespace AppK2J
{
    partial class MainWindow
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
            this.LogBox = new System.Windows.Forms.TextBox();
            this.ShowDef = new System.Windows.Forms.Button();
            this.labelG = new System.Windows.Forms.Label();
            this.labelM = new System.Windows.Forms.Label();
            this.labelF = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LogBox
            // 
            this.LogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogBox.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogBox.Location = new System.Drawing.Point(0, 38);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox.Size = new System.Drawing.Size(369, 183);
            this.LogBox.TabIndex = 0;
            // 
            // ShowDef
            // 
            this.ShowDef.Location = new System.Drawing.Point(12, 9);
            this.ShowDef.Name = "ShowDef";
            this.ShowDef.Size = new System.Drawing.Size(93, 23);
            this.ShowDef.TabIndex = 1;
            this.ShowDef.Text = "Add Keyboard";
            this.ShowDef.UseVisualStyleBackColor = true;
            this.ShowDef.Click += new System.EventHandler(this.ShowDef_Click);
            // 
            // labelG
            // 
            this.labelG.AutoSize = true;
            this.labelG.Location = new System.Drawing.Point(111, 14);
            this.labelG.Name = "labelG";
            this.labelG.Size = new System.Drawing.Size(80, 13);
            this.labelG.TabIndex = 2;
            this.labelG.Text = "Game Mode (+)";
            this.labelG.Visible = false;
            // 
            // labelM
            // 
            this.labelM.AutoSize = true;
            this.labelM.BackColor = System.Drawing.SystemColors.Control;
            this.labelM.Location = new System.Drawing.Point(196, 14);
            this.labelM.Name = "labelM";
            this.labelM.Size = new System.Drawing.Size(84, 13);
            this.labelM.TabIndex = 3;
            this.labelM.Text = "Customization (-)";
            this.labelM.Visible = false;
            // 
            // labelF
            // 
            this.labelF.AutoSize = true;
            this.labelF.Location = new System.Drawing.Point(283, 14);
            this.labelF.Name = "labelF";
            this.labelF.Size = new System.Drawing.Size(75, 13);
            this.labelF.TabIndex = 4;
            this.labelF.Text = "Feed Off (end)";
            this.labelF.Visible = false;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 221);
            this.Controls.Add(this.labelF);
            this.Controls.Add(this.labelM);
            this.Controls.Add(this.labelG);
            this.Controls.Add(this.ShowDef);
            this.Controls.Add(this.LogBox);
            this.MinimumSize = new System.Drawing.Size(385, 260);
            this.Name = "MainWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.Button ShowDef;
        private System.Windows.Forms.Label labelG;
        private System.Windows.Forms.Label labelM;
        private System.Windows.Forms.Label labelF;
    }
}

