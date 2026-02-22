namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmAI_Chat
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
            DevExpress.XtraSplashScreen.SplashScreenManager splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, null, true, true);
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.memoQuestion = new DevExpress.XtraEditors.MemoEdit();
            this.Qesstion = new DevExpress.XtraLayout.LayoutControlItem();
            this.btnAsk = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.gridControlAI = new DevExpress.XtraGrid.GridControl();
            this.gridViewAI = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.memoSql = new DevExpress.XtraEditors.MemoEdit();
            this.SQL = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoQuestion.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Qesstion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoSql.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SQL)).BeginInit();
            this.SuspendLayout();
            // 
            // splashScreenManager1
            // 
            splashScreenManager1.ClosingDelay = 500;
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.gridControlAI);
            this.layoutControl1.Controls.Add(this.memoQuestion);
            this.layoutControl1.Controls.Add(this.btnAsk);
            this.layoutControl1.Controls.Add(this.memoSql);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(1108, 219, 812, 500);
            this.layoutControl1.Root = this.Root;
            this.layoutControl1.Size = new System.Drawing.Size(1177, 545);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.Qesstion,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.SQL});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(1177, 545);
            this.Root.TextVisible = false;
            // 
            // memoQuestion
            // 
            this.memoQuestion.Location = new System.Drawing.Point(73, 258);
            this.memoQuestion.Name = "memoQuestion";
            this.memoQuestion.Size = new System.Drawing.Size(1092, 120);
            this.memoQuestion.StyleController = this.layoutControl1;
            this.memoQuestion.TabIndex = 4;
            // 
            // Qesstion
            // 
            this.Qesstion.Control = this.memoQuestion;
            this.Qesstion.Location = new System.Drawing.Point(0, 246);
            this.Qesstion.Name = "Qesstion";
            this.Qesstion.Size = new System.Drawing.Size(1157, 124);
            this.Qesstion.TextSize = new System.Drawing.Size(49, 16);
            // 
            // btnAsk
            // 
            this.btnAsk.Location = new System.Drawing.Point(12, 506);
            this.btnAsk.Name = "btnAsk";
            this.btnAsk.Size = new System.Drawing.Size(1153, 27);
            this.btnAsk.StyleController = this.layoutControl1;
            this.btnAsk.TabIndex = 5;
            this.btnAsk.Text = "Gửi";
            this.btnAsk.Click += new System.EventHandler(this.btnAsk_Click);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnAsk;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 494);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(1157, 31);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // gridControlAI
            // 
            this.gridControlAI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlAI.Location = new System.Drawing.Point(12, 12);
            this.gridControlAI.MainView = this.gridViewAI;
            this.gridControlAI.Name = "gridControlAI";
            this.gridControlAI.Size = new System.Drawing.Size(1153, 242);
            this.gridControlAI.TabIndex = 6;
            this.gridControlAI.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewAI});
            // 
            // gridViewAI
            // 
            this.gridViewAI.GridControl = this.gridControlAI;
            this.gridViewAI.Name = "gridViewAI";
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.gridControlAI;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(1157, 246);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // memoSql
            // 
            this.memoSql.Location = new System.Drawing.Point(73, 382);
            this.memoSql.Name = "memoSql";
            this.memoSql.Size = new System.Drawing.Size(1092, 120);
            this.memoSql.StyleController = this.layoutControl1;
            this.memoSql.TabIndex = 7;
            // 
            // SQL
            // 
            this.SQL.Control = this.memoSql;
            this.SQL.Location = new System.Drawing.Point(0, 370);
            this.SQL.Name = "SQL";
            this.SQL.Size = new System.Drawing.Size(1157, 124);
            this.SQL.TextSize = new System.Drawing.Size(49, 16);
            // 
            // FrmAI_Chat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1177, 545);
            this.Controls.Add(this.layoutControl1);
            this.Name = "FrmAI_Chat";
            this.Text = "Trợ Lý Ảo";
            this.Load += new System.EventHandler(this.FrmAI_Chat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoQuestion.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Qesstion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoSql.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SQL)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.MemoEdit memoQuestion;
        private DevExpress.XtraEditors.SimpleButton btnAsk;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem Qesstion;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraGrid.GridControl gridControlAI;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewAI;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraEditors.MemoEdit memoSql;
        private DevExpress.XtraLayout.LayoutControlItem SQL;
    }
}