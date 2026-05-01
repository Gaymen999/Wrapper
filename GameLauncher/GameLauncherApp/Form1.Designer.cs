namespace GameLauncherApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lstGames = new System.Windows.Forms.ListBox();
            this.btnAddGame = new System.Windows.Forms.Button();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstGames
            // 
            this.lstGames.FormattingEnabled = true;
            this.lstGames.ItemHeight = 20;
            this.lstGames.Location = new System.Drawing.Point(12, 12);
            this.lstGames.Name = "lstGames";
            this.lstGames.Size = new System.Drawing.Size(360, 204);
            this.lstGames.TabIndex = 0;
            // 
            // btnAddGame
            // 
            this.btnAddGame.Location = new System.Drawing.Point(12, 222);
            this.btnAddGame.Name = "btnAddGame";
            this.btnAddGame.Size = new System.Drawing.Size(175, 45);
            this.btnAddGame.TabIndex = 1;
            this.btnAddGame.Text = "Add Game (.exe)";
            this.btnAddGame.UseVisualStyleBackColor = true;
            this.btnAddGame.Click += new System.EventHandler(this.btnAddGame_Click);
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(197, 222);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(175, 45);
            this.btnLaunch.TabIndex = 2;
            this.btnLaunch.Text = "Launch Selected";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 275);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(102, 20);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status: Ready";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 311);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.btnAddGame);
            this.Controls.Add(this.lstGames);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Game Optimizer Launcher";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lstGames;
        private System.Windows.Forms.Button btnAddGame;
        private System.Windows.Forms.Button btnLaunch;
        private System.Windows.Forms.Label lblStatus;
    }
}
