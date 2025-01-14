namespace OcrReadGUI
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.midSplitter = new System.Windows.Forms.Splitter();
            this.HeadPanel = new System.Windows.Forms.Panel();
            this.RunBtn = new System.Windows.Forms.Button();
            this.ClearBtn = new System.Windows.Forms.Button();
            this.FileBtn = new System.Windows.Forms.Button();
            this.ScriptTxtBox = new System.Windows.Forms.TextBox();
            this.TargetFileTxt = new System.Windows.Forms.TextBox();
            this.DocCtrl = new OcrReadGUI.DocumentControl();
            this.HeadPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // midSplitter
            // 
            this.midSplitter.Dock = System.Windows.Forms.DockStyle.Right;
            this.midSplitter.Location = new System.Drawing.Point(464, 0);
            this.midSplitter.Name = "midSplitter";
            this.midSplitter.Size = new System.Drawing.Size(3, 711);
            this.midSplitter.TabIndex = 1;
            this.midSplitter.TabStop = false;
            // 
            // HeadPanel
            // 
            this.HeadPanel.Controls.Add(this.RunBtn);
            this.HeadPanel.Controls.Add(this.ClearBtn);
            this.HeadPanel.Controls.Add(this.FileBtn);
            this.HeadPanel.Controls.Add(this.ScriptTxtBox);
            this.HeadPanel.Controls.Add(this.TargetFileTxt);
            this.HeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeadPanel.Location = new System.Drawing.Point(0, 0);
            this.HeadPanel.Name = "HeadPanel";
            this.HeadPanel.Size = new System.Drawing.Size(464, 74);
            this.HeadPanel.TabIndex = 2;
            // 
            // RunBtn
            // 
            this.RunBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunBtn.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.RunBtn.Location = new System.Drawing.Point(348, 37);
            this.RunBtn.Name = "RunBtn";
            this.RunBtn.Size = new System.Drawing.Size(110, 28);
            this.RunBtn.TabIndex = 4;
            this.RunBtn.Text = "実行";
            this.RunBtn.UseVisualStyleBackColor = true;
            this.RunBtn.Click += new System.EventHandler(this.RunBtn_Click);
            // 
            // ClearBtn
            // 
            this.ClearBtn.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ClearBtn.Location = new System.Drawing.Point(12, 37);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(110, 28);
            this.ClearBtn.TabIndex = 3;
            this.ClearBtn.Text = "クリア";
            this.ClearBtn.UseVisualStyleBackColor = true;
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // FileBtn
            // 
            this.FileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FileBtn.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FileBtn.Location = new System.Drawing.Point(295, 9);
            this.FileBtn.Name = "FileBtn";
            this.FileBtn.Size = new System.Drawing.Size(25, 25);
            this.FileBtn.TabIndex = 2;
            this.FileBtn.Text = "...";
            this.FileBtn.UseVisualStyleBackColor = true;
            this.FileBtn.Click += new System.EventHandler(this.FileBtn_Click);
            // 
            // ScriptTxtBox
            // 
            this.ScriptTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ScriptTxtBox.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ScriptTxtBox.Location = new System.Drawing.Point(326, 12);
            this.ScriptTxtBox.Name = "ScriptTxtBox";
            this.ScriptTxtBox.Size = new System.Drawing.Size(132, 19);
            this.ScriptTxtBox.TabIndex = 1;
            this.ScriptTxtBox.Text = "action.txt";
            // 
            // TargetFileTxt
            // 
            this.TargetFileTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetFileTxt.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TargetFileTxt.Location = new System.Drawing.Point(12, 12);
            this.TargetFileTxt.Name = "TargetFileTxt";
            this.TargetFileTxt.Size = new System.Drawing.Size(277, 19);
            this.TargetFileTxt.TabIndex = 0;
            // 
            // DocCtrl
            // 
            this.DocCtrl.BackColor = System.Drawing.Color.White;
            this.DocCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DocCtrl.Document = null;
            this.DocCtrl.Location = new System.Drawing.Point(0, 74);
            this.DocCtrl.Name = "DocCtrl";
            this.DocCtrl.OcrLayout = null;
            this.DocCtrl.Rectangles = ((System.Collections.Generic.List<System.Drawing.Rectangle>)(resources.GetObject("DocCtrl.Rectangles")));
            this.DocCtrl.Size = new System.Drawing.Size(464, 637);
            this.DocCtrl.TabIndex = 0;
            this.DocCtrl.Text = "documentControl1";
            this.DocCtrl.Zoom = 1D;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(467, 711);
            this.Controls.Add(this.DocCtrl);
            this.Controls.Add(this.HeadPanel);
            this.Controls.Add(this.midSplitter);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "OcrReadGUI";
            this.HeadPanel.ResumeLayout(false);
            this.HeadPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DocumentControl DocCtrl;
        private System.Windows.Forms.Splitter midSplitter;
        private System.Windows.Forms.Panel HeadPanel;
        private System.Windows.Forms.TextBox TargetFileTxt;
        private System.Windows.Forms.TextBox ScriptTxtBox;
        private System.Windows.Forms.Button FileBtn;
        private System.Windows.Forms.Button ClearBtn;
        private System.Windows.Forms.Button RunBtn;
    }
}

