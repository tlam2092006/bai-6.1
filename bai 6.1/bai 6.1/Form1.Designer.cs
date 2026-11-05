namespace bai_6._1
{
    partial class Form1
    {
        private TabControl mainTabs = null!;
        private TabPage catalogTab = null!;
        private TabPage checkoutTab = null!;
        private TabPage reportsTab = null!;
        private Label catalogPreviewLabel = null!;
        private Label checkoutPreviewLabel = null!;
        private Label reportsPreviewLabel = null!;

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
            components = new System.ComponentModel.Container();
            mainTabs = new TabControl();
            catalogTab = new TabPage();
            checkoutTab = new TabPage();
            reportsTab = new TabPage();
            catalogPreviewLabel = new Label();
            checkoutPreviewLabel = new Label();
            reportsPreviewLabel = new Label();
            mainTabs.SuspendLayout();
            catalogTab.SuspendLayout();
            checkoutTab.SuspendLayout();
            reportsTab.SuspendLayout();

            catalogPreviewLabel.AutoSize = true;
            catalogPreviewLabel.Location = new Point(24, 24);
            catalogPreviewLabel.Name = "catalogPreviewLabel";
            catalogPreviewLabel.Text = "Quản lý sản phẩm, khách hàng và nhân viên.";
            catalogTab.Controls.Add(catalogPreviewLabel);
            catalogTab.Name = "catalogTab";
            catalogTab.Text = "Quản lý danh mục";

            checkoutPreviewLabel.AutoSize = true;
            checkoutPreviewLabel.Location = new Point(24, 24);
            checkoutPreviewLabel.Name = "checkoutPreviewLabel";
            checkoutPreviewLabel.Text = "Tìm sản phẩm, tạo giỏ hàng và thanh toán hóa đơn.";
            checkoutTab.Controls.Add(checkoutPreviewLabel);
            checkoutTab.Name = "checkoutTab";
            checkoutTab.Text = "Bán hàng";

            reportsPreviewLabel.AutoSize = true;
            reportsPreviewLabel.Location = new Point(24, 24);
            reportsPreviewLabel.Name = "reportsPreviewLabel";
            reportsPreviewLabel.Text = "Xem doanh thu ngày, tháng và sản phẩm bán chạy.";
            reportsTab.Controls.Add(reportsPreviewLabel);
            reportsTab.Name = "reportsTab";
            reportsTab.Text = "Báo cáo";

            mainTabs.Dock = DockStyle.Fill;
            mainTabs.Name = "mainTabs";
            mainTabs.TabPages.Add(catalogTab);
            mainTabs.TabPages.Add(checkoutTab);
            mainTabs.TabPages.Add(reportsTab);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 780);
            Controls.Add(mainTabs);
            Name = "Form1";
            Text = "Hệ thống quản lý bán hàng siêu thị mini";

            catalogTab.ResumeLayout(false);
            catalogTab.PerformLayout();
            checkoutTab.ResumeLayout(false);
            checkoutTab.PerformLayout();
            reportsTab.ResumeLayout(false);
            reportsTab.PerformLayout();
            mainTabs.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
