using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace WinFormsUI;

partial class Main
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
        blazorWebView = new BlazorWebView();
        SuspendLayout();
        // 
        // blazorWebView
        // 
        blazorWebView.Dock = DockStyle.Fill;
        blazorWebView.Location = new Point(0, 0);
        blazorWebView.Name = "blazorWebView";
        blazorWebView.Size = new Size(800, 450);
        blazorWebView.TabIndex = 0;
        blazorWebView.Text = "blazorWebView1";
        // 
        // Main
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(blazorWebView);
        Name = "Main";
        Text = "Form1";
        ResumeLayout(false);
    }

    #endregion

    private BlazorWebView blazorWebView;
}