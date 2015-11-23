namespace RectifyUI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatuslbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgresslbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripCancelBtn = new System.Windows.Forms.ToolStripSplitButton();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.btnAnalyseLibrary = new System.Windows.Forms.Button();
            this.pnlAdvanced = new System.Windows.Forms.Panel();
            this.cbAdvanced = new System.Windows.Forms.CheckBox();
            this.btnBrowsePicasaLibrary = new System.Windows.Forms.Button();
            this.tbPicasaLibraryPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridMain = new System.Windows.Forms.DataGridView();
            this.colSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDestination = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFileTypeImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.colFileTypeText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colErrorImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.colErrorText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlAnalysisResults = new System.Windows.Forms.Panel();
            this.lblAnalysisResults = new System.Windows.Forms.Label();
            this.pnlActionChanges = new System.Windows.Forms.Panel();
            this.cbSelectAllRows = new System.Windows.Forms.CheckBox();
            this.btnRectifySelected = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemExitApplication = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutRectifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridMain)).BeginInit();
            this.pnlAnalysisResults.SuspendLayout();
            this.pnlActionChanges.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatuslbl,
            this.toolStripProgresslbl,
            this.toolStripProgress,
            this.toolStripCancelBtn});
            this.statusStripMain.Location = new System.Drawing.Point(0, 916);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(954, 22);
            this.statusStripMain.TabIndex = 0;
            this.statusStripMain.Text = "statusStrip1";
            // 
            // toolStripStatuslbl
            // 
            this.toolStripStatuslbl.AutoSize = false;
            this.toolStripStatuslbl.Name = "toolStripStatuslbl";
            this.toolStripStatuslbl.Size = new System.Drawing.Size(521, 17);
            this.toolStripStatuslbl.Spring = true;
            this.toolStripStatuslbl.Text = "Ready";
            this.toolStripStatuslbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgresslbl
            // 
            this.toolStripProgresslbl.Name = "toolStripProgresslbl";
            this.toolStripProgresslbl.Size = new System.Drawing.Size(59, 17);
            this.toolStripProgresslbl.Text = "Analysing";
            // 
            // toolStripProgress
            // 
            this.toolStripProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgress.Margin = new System.Windows.Forms.Padding(1, 3, 10, 3);
            this.toolStripProgress.Name = "toolStripProgress";
            this.toolStripProgress.Size = new System.Drawing.Size(300, 16);
            // 
            // toolStripCancelBtn
            // 
            this.toolStripCancelBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripCancelBtn.DropDownButtonWidth = 0;
            this.toolStripCancelBtn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripCancelBtn.Image")));
            this.toolStripCancelBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripCancelBtn.Name = "toolStripCancelBtn";
            this.toolStripCancelBtn.Size = new System.Drawing.Size(48, 20);
            this.toolStripCancelBtn.Text = "Cancel";
            this.toolStripCancelBtn.ToolTipText = "Click to cancel the currently active operation";
            this.toolStripCancelBtn.ButtonClick += new System.EventHandler(this.toolStripCancelBtn_ButtonClick);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.IsSplitterFixed = true;
            this.splitMain.Location = new System.Drawing.Point(0, 24);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.btnAnalyseLibrary);
            this.splitMain.Panel1.Controls.Add(this.pnlAdvanced);
            this.splitMain.Panel1.Controls.Add(this.cbAdvanced);
            this.splitMain.Panel1.Controls.Add(this.btnBrowsePicasaLibrary);
            this.splitMain.Panel1.Controls.Add(this.tbPicasaLibraryPath);
            this.splitMain.Panel1.Controls.Add(this.label1);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.dataGridMain);
            this.splitMain.Panel2.Controls.Add(this.pnlAnalysisResults);
            this.splitMain.Panel2.Controls.Add(this.pnlActionChanges);
            this.splitMain.Size = new System.Drawing.Size(954, 892);
            this.splitMain.SplitterDistance = 73;
            this.splitMain.TabIndex = 1;
            // 
            // btnAnalyseLibrary
            // 
            this.btnAnalyseLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnalyseLibrary.Location = new System.Drawing.Point(867, 40);
            this.btnAnalyseLibrary.Name = "btnAnalyseLibrary";
            this.btnAnalyseLibrary.Size = new System.Drawing.Size(75, 23);
            this.btnAnalyseLibrary.TabIndex = 5;
            this.btnAnalyseLibrary.Text = "Analyse";
            this.btnAnalyseLibrary.UseVisualStyleBackColor = true;
            this.btnAnalyseLibrary.Click += new System.EventHandler(this.btnAnalyseLibrary_Click);
            // 
            // pnlAdvanced
            // 
            this.pnlAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlAdvanced.Location = new System.Drawing.Point(95, 172);
            this.pnlAdvanced.Name = "pnlAdvanced";
            this.pnlAdvanced.Size = new System.Drawing.Size(847, 151);
            this.pnlAdvanced.TabIndex = 4;
            // 
            // cbAdvanced
            // 
            this.cbAdvanced.AutoSize = true;
            this.cbAdvanced.Location = new System.Drawing.Point(95, 149);
            this.cbAdvanced.Name = "cbAdvanced";
            this.cbAdvanced.Size = new System.Drawing.Size(80, 17);
            this.cbAdvanced.TabIndex = 3;
            this.cbAdvanced.Text = "checkBox1";
            this.cbAdvanced.UseVisualStyleBackColor = true;
            // 
            // btnBrowsePicasaLibrary
            // 
            this.btnBrowsePicasaLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowsePicasaLibrary.Location = new System.Drawing.Point(908, 12);
            this.btnBrowsePicasaLibrary.Name = "btnBrowsePicasaLibrary";
            this.btnBrowsePicasaLibrary.Size = new System.Drawing.Size(34, 23);
            this.btnBrowsePicasaLibrary.TabIndex = 2;
            this.btnBrowsePicasaLibrary.Text = "...";
            this.btnBrowsePicasaLibrary.UseVisualStyleBackColor = true;
            // 
            // tbPicasaLibraryPath
            // 
            this.tbPicasaLibraryPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPicasaLibraryPath.Location = new System.Drawing.Point(95, 14);
            this.tbPicasaLibraryPath.Name = "tbPicasaLibraryPath";
            this.tbPicasaLibraryPath.Size = new System.Drawing.Size(807, 20);
            this.tbPicasaLibraryPath.TabIndex = 1;
            this.tbPicasaLibraryPath.Text = "H:\\photos\\";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Picasa Library:";
            // 
            // dataGridMain
            // 
            this.dataGridMain.AllowUserToAddRows = false;
            this.dataGridMain.AllowUserToDeleteRows = false;
            this.dataGridMain.AllowUserToResizeRows = false;
            this.dataGridMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSelected,
            this.colFileName,
            this.colSource,
            this.colDestination,
            this.colFileTypeImage,
            this.colFileTypeText,
            this.colDate,
            this.colErrorImage,
            this.colErrorText});
            this.dataGridMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridMain.Location = new System.Drawing.Point(0, 23);
            this.dataGridMain.Name = "dataGridMain";
            this.dataGridMain.RowHeadersVisible = false;
            this.dataGridMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridMain.Size = new System.Drawing.Size(954, 754);
            this.dataGridMain.TabIndex = 3;
            this.dataGridMain.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridMain_CellMouseUp);
            // 
            // colSelected
            // 
            this.colSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSelected.FillWeight = 1F;
            this.colSelected.Frozen = true;
            this.colSelected.HeaderText = "";
            this.colSelected.MinimumWidth = 35;
            this.colSelected.Name = "colSelected";
            this.colSelected.Width = 35;
            // 
            // colFileName
            // 
            this.colFileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colFileName.FillWeight = 1F;
            this.colFileName.HeaderText = "File";
            this.colFileName.MinimumWidth = 250;
            this.colFileName.Name = "colFileName";
            this.colFileName.ReadOnly = true;
            // 
            // colSource
            // 
            this.colSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSource.FillWeight = 1F;
            this.colSource.HeaderText = "Source";
            this.colSource.MinimumWidth = 250;
            this.colSource.Name = "colSource";
            this.colSource.ReadOnly = true;
            this.colSource.Width = 250;
            // 
            // colDestination
            // 
            this.colDestination.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDestination.FillWeight = 1F;
            this.colDestination.HeaderText = "Destination";
            this.colDestination.MinimumWidth = 250;
            this.colDestination.Name = "colDestination";
            this.colDestination.ReadOnly = true;
            this.colDestination.Width = 250;
            // 
            // colFileTypeImage
            // 
            this.colFileTypeImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colFileTypeImage.FillWeight = 0.2961082F;
            this.colFileTypeImage.HeaderText = "Type";
            this.colFileTypeImage.MinimumWidth = 32;
            this.colFileTypeImage.Name = "colFileTypeImage";
            this.colFileTypeImage.ReadOnly = true;
            this.colFileTypeImage.Width = 32;
            // 
            // colFileTypeText
            // 
            this.colFileTypeText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colFileTypeText.FillWeight = 0.2961082F;
            this.colFileTypeText.HeaderText = "Type";
            this.colFileTypeText.MinimumWidth = 60;
            this.colFileTypeText.Name = "colFileTypeText";
            this.colFileTypeText.ReadOnly = true;
            this.colFileTypeText.Width = 60;
            // 
            // colDate
            // 
            this.colDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDate.FillWeight = 0.2961082F;
            this.colDate.HeaderText = "Date";
            this.colDate.MinimumWidth = 100;
            this.colDate.Name = "colDate";
            // 
            // colErrorImage
            // 
            this.colErrorImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colErrorImage.FillWeight = 0.2961082F;
            this.colErrorImage.HeaderText = "Error";
            this.colErrorImage.MinimumWidth = 32;
            this.colErrorImage.Name = "colErrorImage";
            this.colErrorImage.ReadOnly = true;
            this.colErrorImage.Width = 32;
            // 
            // colErrorText
            // 
            this.colErrorText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colErrorText.FillWeight = 0.2961082F;
            this.colErrorText.HeaderText = "Error";
            this.colErrorText.MinimumWidth = 250;
            this.colErrorText.Name = "colErrorText";
            this.colErrorText.ReadOnly = true;
            this.colErrorText.Width = 250;
            // 
            // pnlAnalysisResults
            // 
            this.pnlAnalysisResults.Controls.Add(this.lblAnalysisResults);
            this.pnlAnalysisResults.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAnalysisResults.Location = new System.Drawing.Point(0, 0);
            this.pnlAnalysisResults.Name = "pnlAnalysisResults";
            this.pnlAnalysisResults.Size = new System.Drawing.Size(954, 23);
            this.pnlAnalysisResults.TabIndex = 2;
            // 
            // lblAnalysisResults
            // 
            this.lblAnalysisResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAnalysisResults.Location = new System.Drawing.Point(0, 0);
            this.lblAnalysisResults.Name = "lblAnalysisResults";
            this.lblAnalysisResults.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblAnalysisResults.Size = new System.Drawing.Size(954, 23);
            this.lblAnalysisResults.TabIndex = 0;
            this.lblAnalysisResults.Text = "label2";
            // 
            // pnlActionChanges
            // 
            this.pnlActionChanges.Controls.Add(this.cbSelectAllRows);
            this.pnlActionChanges.Controls.Add(this.btnRectifySelected);
            this.pnlActionChanges.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActionChanges.Location = new System.Drawing.Point(0, 777);
            this.pnlActionChanges.Name = "pnlActionChanges";
            this.pnlActionChanges.Size = new System.Drawing.Size(954, 38);
            this.pnlActionChanges.TabIndex = 1;
            // 
            // cbSelectAllRows
            // 
            this.cbSelectAllRows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSelectAllRows.AutoSize = true;
            this.cbSelectAllRows.Location = new System.Drawing.Point(12, 10);
            this.cbSelectAllRows.Name = "cbSelectAllRows";
            this.cbSelectAllRows.Size = new System.Drawing.Size(70, 17);
            this.cbSelectAllRows.TabIndex = 2;
            this.cbSelectAllRows.Text = "Select &All";
            this.cbSelectAllRows.UseVisualStyleBackColor = true;
            this.cbSelectAllRows.CheckedChanged += new System.EventHandler(this.cbSelectAllRows_CheckedChanged);
            // 
            // btnRectifySelected
            // 
            this.btnRectifySelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRectifySelected.Location = new System.Drawing.Point(827, 6);
            this.btnRectifySelected.Name = "btnRectifySelected";
            this.btnRectifySelected.Size = new System.Drawing.Size(115, 23);
            this.btnRectifySelected.TabIndex = 1;
            this.btnRectifySelected.Text = "&Rectify Selected";
            this.btnRectifySelected.UseVisualStyleBackColor = true;
            this.btnRectifySelected.Click += new System.EventHandler(this.btnRectifySelected_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.mItemAbout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(954, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mItemExitApplication});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // mItemExitApplication
            // 
            this.mItemExitApplication.Name = "mItemExitApplication";
            this.mItemExitApplication.Size = new System.Drawing.Size(92, 22);
            this.mItemExitApplication.Text = "&Exit";
            // 
            // mItemAbout
            // 
            this.mItemAbout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutRectifyToolStripMenuItem});
            this.mItemAbout.Name = "mItemAbout";
            this.mItemAbout.Size = new System.Drawing.Size(44, 20);
            this.mItemAbout.Text = "&Help";
            // 
            // aboutRectifyToolStripMenuItem
            // 
            this.aboutRectifyToolStripMenuItem.Name = "aboutRectifyToolStripMenuItem";
            this.aboutRectifyToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.aboutRectifyToolStripMenuItem.Text = "&About Rectify";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(954, 938);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Rectify | for Google Picasa";
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridMain)).EndInit();
            this.pnlAnalysisResults.ResumeLayout(false);
            this.pnlActionChanges.ResumeLayout(false);
            this.pnlActionChanges.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatuslbl;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgress;
        private System.Windows.Forms.ToolStripStatusLabel toolStripProgresslbl;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button btnBrowsePicasaLibrary;
        private System.Windows.Forms.TextBox tbPicasaLibraryPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlAdvanced;
        private System.Windows.Forms.CheckBox cbAdvanced;
        private System.Windows.Forms.Button btnAnalyseLibrary;
        private System.Windows.Forms.Panel pnlAnalysisResults;
        private System.Windows.Forms.Panel pnlActionChanges;
        private System.Windows.Forms.DataGridView dataGridMain;
        private System.Windows.Forms.CheckBox cbSelectAllRows;
        private System.Windows.Forms.Button btnRectifySelected;
        private System.Windows.Forms.Label lblAnalysisResults;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mItemExitApplication;
        private System.Windows.Forms.ToolStripMenuItem mItemAbout;
        private System.Windows.Forms.ToolStripMenuItem aboutRectifyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripCancelBtn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDestination;
        private System.Windows.Forms.DataGridViewImageColumn colFileTypeImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileTypeText;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewImageColumn colErrorImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colErrorText;
    }
}

