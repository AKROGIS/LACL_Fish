namespace FileUploader
{
    partial class FishFileUploadForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.UploadButton = new System.Windows.Forms.Button();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.FilesDataGridView = new System.Windows.Forms.DataGridView();
            this.FolderTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.UploadProgressBar = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.UploadProgressLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.UploadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.CancelUploadButton = new System.Windows.Forms.Button();
            this.FolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.FilesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Step 1) Select and upload a folder of fish telemetry data files.";
            // 
            // UploadButton
            // 
            this.UploadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UploadButton.Location = new System.Drawing.Point(432, 31);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(75, 23);
            this.UploadButton.TabIndex = 1;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(15, 117);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(75, 23);
            this.RefreshButton.TabIndex = 2;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // FilesDataGridView
            // 
            this.FilesDataGridView.AllowUserToAddRows = false;
            this.FilesDataGridView.AllowUserToDeleteRows = false;
            this.FilesDataGridView.AllowUserToOrderColumns = true;
            this.FilesDataGridView.AllowUserToResizeRows = false;
            this.FilesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilesDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.FilesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FilesDataGridView.Location = new System.Drawing.Point(15, 146);
            this.FilesDataGridView.Name = "FilesDataGridView";
            this.FilesDataGridView.ReadOnly = true;
            this.FilesDataGridView.Size = new System.Drawing.Size(492, 218);
            this.FilesDataGridView.TabIndex = 3;
            this.FilesDataGridView.SelectionChanged += new System.EventHandler(this.FilesDataGridView_SelectionChanged);
            this.FilesDataGridView.DoubleClick += new System.EventHandler(this.FilesDataGridView_DoubleClick);
            // 
            // FolderTextBox
            // 
            this.FolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FolderTextBox.Location = new System.Drawing.Point(57, 33);
            this.FolderTextBox.Name = "FolderTextBox";
            this.FolderTextBox.Size = new System.Drawing.Size(339, 20);
            this.FolderTextBox.TabIndex = 4;
            this.FolderTextBox.TextChanged += new System.EventHandler(this.FolderTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Folder:";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseButton.Location = new System.Drawing.Point(402, 31);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(24, 23);
            this.BrowseButton.TabIndex = 6;
            this.BrowseButton.Text = "...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteButton.Location = new System.Drawing.Point(432, 117);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 7;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(394, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Step 2) Review and correct the file list (double click a row to see the file cont" +
    "ents).";
            // 
            // UploadProgressBar
            // 
            this.UploadProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UploadProgressBar.Location = new System.Drawing.Point(69, 59);
            this.UploadProgressBar.Name = "UploadProgressBar";
            this.UploadProgressBar.Size = new System.Drawing.Size(293, 24);
            this.UploadProgressBar.Step = 1;
            this.UploadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.UploadProgressBar.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Progress:";
            // 
            // UploadProgressLabel
            // 
            this.UploadProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UploadProgressLabel.AutoSize = true;
            this.UploadProgressLabel.Location = new System.Drawing.Point(368, 65);
            this.UploadProgressLabel.Name = "UploadProgressLabel";
            this.UploadProgressLabel.Size = new System.Drawing.Size(0, 13);
            this.UploadProgressLabel.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 367);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Step 3) Repeat";
            // 
            // UploadBackgroundWorker
            // 
            this.UploadBackgroundWorker.WorkerReportsProgress = true;
            this.UploadBackgroundWorker.WorkerSupportsCancellation = true;
            this.UploadBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UploadBackgroundWorker_DoWork);
            this.UploadBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.UploadBackgroundWorker_ProgressChanged);
            this.UploadBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.UploadBackgroundWorker_RunWorkerCompleted);
            // 
            // CancelUploadButton
            // 
            this.CancelUploadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelUploadButton.Location = new System.Drawing.Point(432, 60);
            this.CancelUploadButton.Name = "CancelUploadButton";
            this.CancelUploadButton.Size = new System.Drawing.Size(75, 23);
            this.CancelUploadButton.TabIndex = 13;
            this.CancelUploadButton.Text = "Cancel";
            this.CancelUploadButton.UseVisualStyleBackColor = true;
            this.CancelUploadButton.Click += new System.EventHandler(this.CancelUploadButton_Click);
            // 
            // FolderBrowser
            // 
            this.FolderBrowser.ShowNewFolderButton = false;
            // 
            // FishFileUploadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 389);
            this.Controls.Add(this.CancelUploadButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.UploadProgressLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.UploadProgressBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FolderTextBox);
            this.Controls.Add(this.FilesDataGridView);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.UploadButton);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(315, 315);
            this.Name = "FishFileUploadForm";
            this.Text = "Super-Duper Fish File Uploader";
            ((System.ComponentModel.ISupportInitialize)(this.FilesDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.DataGridView FilesDataGridView;
        private System.Windows.Forms.TextBox FolderTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar UploadProgressBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label UploadProgressLabel;
        private System.Windows.Forms.Label label6;
        private System.ComponentModel.BackgroundWorker UploadBackgroundWorker;
        private System.Windows.Forms.Button CancelUploadButton;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowser;
    }
}

