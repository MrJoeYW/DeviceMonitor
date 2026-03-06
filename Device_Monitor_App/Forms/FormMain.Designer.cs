namespace Device_Monitor_App.Forms;

partial class FormMain
{
    private System.ComponentModel.IContainer components = null;
    private Microsoft.Web.WebView2.WinForms.WebView2 webView;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
        this.SuspendLayout();

        // webView
        this.webView.Dock = DockStyle.Fill;
        this.webView.Location = new Point(0, 0);
        this.webView.Name = "webView";
        this.webView.Size = new Size(1280, 800);
        this.webView.TabIndex = 0;

        // FormMain
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1280, 800);
        this.Controls.Add(this.webView);
        this.Name = "FormMain";
        this.Text = "设备监控系统";
        this.Load += new EventHandler(this.FormMain_Load);

        ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
        this.ResumeLayout(false);
    }
}
