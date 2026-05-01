namespace GameLauncherApp;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        btnLaunch = new Button();
        lblStatus = new Label();
        SuspendLayout();
        // 
        // btnLaunch
        // 
        btnLaunch.Location = new Point(12, 12);
        btnLaunch.Name = "btnLaunch";
        btnLaunch.Size = new Size(150, 40);
        btnLaunch.TabIndex = 0;
        btnLaunch.Text = "Launch Game";
        btnLaunch.UseVisualStyleBackColor = true;
        btnLaunch.Click += btnLaunch_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 65);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(50, 20);
        lblStatus.TabIndex = 1;
        lblStatus.Text = "Status: Ready";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(300, 120);
        Controls.Add(lblStatus);
        Controls.Add(btnLaunch);
        Name = "Form1";
        Text = "Game Launcher";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Button btnLaunch;
    private System.Windows.Forms.Label lblStatus;

    #endregion
}
