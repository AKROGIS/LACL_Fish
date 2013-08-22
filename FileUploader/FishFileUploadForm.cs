using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataModel;

namespace FileUploader
{
    internal sealed partial class FishFileUploadForm : BaseForm
    {

        public FishFileUploadForm()
        {
            InitializeComponent();
            RestoreWindow();
            SetupForm();
        }

        private Boolean HaveFolder { get; set; }
        private Boolean Uploading { get; set; }
        private FishTaggingDataContext CurrentContext { get; set; }

        #region Form Control Events

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowser.ShowDialog(this);
            FolderTextBox.Text = FolderBrowser.SelectedPath;
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            UploadFolder(FolderTextBox.Text);
        }

        private void CancelUploadButton_Click(object sender, EventArgs e)
        {
            UploadBackgroundWorker.CancelAsync();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var files =
                FilesDataGridView.SelectedRows.Cast<DataGridViewRow>()
                                 .Select(row => (RawDataFile)row.DataBoundItem)
                                 .ToList();
            foreach (var file in files)
            {
                CurrentContext.RawDataFiles.DeleteOnSubmit(file);
            }
            if (SubmitChanges())
                RefreshDataGrid();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private void FolderTextBox_TextChanged(object sender, EventArgs e)
        {
            HaveFolder = Directory.Exists(FolderTextBox.Text);
            if (!Uploading)
            {
                if (HaveFolder)
                    PrepareProgressBar();
                else
                    ResetProgressBar();
            }
            EnableControls();
        }

        private void FilesDataGridView_DoubleClick(object sender, EventArgs e)
        {
            if (FilesDataGridView.CurrentRow == null)
                return;
            var file = FilesDataGridView.CurrentRow.DataBoundItem as RawDataFile;
            if (file == null)
                return;
            ShowFile(file);
        }

        private void FilesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            EnableControls();
        }

        #endregion

        private void SetupForm ()
        {
            Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents(); // Make sure the wait cursor is shown;
            try
            {
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(Environment.NewLine + ex.Message + Environment.NewLine +
                    "Connection String:" + Environment.NewLine + CurrentContext.Connection.ConnectionString,
                    "Unable to connect to the database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Cursor.Current = Cursors.Default;
            Enabled = true;
            EnableControls();
        }

        private void EnableControls ()
        {
            UploadButton.Enabled = HaveFolder && !Uploading;
            CancelUploadButton.Enabled = Uploading;
            DeleteButton.Enabled = FilesDataGridView.SelectedRows.Count > 0;
        }

        private void RefreshDataGrid()
        {
            FilesDataGridView.DataSource = null;
            CurrentContext = new FishTaggingDataContext();
            FilesDataGridView.DataSource = CurrentContext.RawDataFiles;
            FilesDataGridView.Columns[0].Visible = false;
            FilesDataGridView.Columns[1].HeaderText = "File";
            FilesDataGridView.Columns[2].HeaderText = "Folder";
            FilesDataGridView.Columns[3].HeaderText = "When";
            FilesDataGridView.Columns[4].HeaderText = "Who";
            FilesDataGridView.Columns[5].Visible = false;
        }

        private bool SubmitChanges()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                CurrentContext.SubmitChanges();
            }
            catch (SqlException ex)
            {
                string msg = "Unable to submit changes to the database.\n" +
                             "Error message:\n" + ex.Message;
                MessageBox.Show(msg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            return true;
        }

        private void ShowFile(RawDataFile file)
        {
            Cursor.Current = Cursors.WaitCursor;
            var form = new FileContentsForm(file.Contents.ToArray(), file.FileName);
            Cursor.Current = Cursors.Default;
            form.Show(this);
        }

        private void ResetProgressBar()
        {
            UploadProgressBar.Value = 0;
            UploadProgressBar.Maximum = 100;
            UploadProgressLabel.Text = "";
        }

        private void PrepareProgressBar()
        {
            var files = Directory.GetFiles(FolderTextBox.Text);
            UploadProgressBar.Value = 0;
            UploadProgressBar.Maximum = files.Length;
            UpdateProgressLabel();
        }

        private void UpdateProgressLabel()
        {
            UploadProgressLabel.Text = String.Format("{0} of {1}",
                UploadProgressBar.Value, UploadProgressBar.Maximum);
        }

        private void UploadFolder(string folder)
        {
            if (!Uploading && !UploadBackgroundWorker.IsBusy)
            {
                Uploading = true;
                FolderTextBox.Text = "";
                UploadBackgroundWorker.RunWorkerAsync(folder);
            }
        }

        //Done on background thread
        private void UploadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var folder = (String) e.Argument;
            var files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
                if ((worker == null || worker.CancellationPending))
                {
                    e.Cancel = true;
                    break;
                }
                UploadFile(file);
                worker.ReportProgress(1); //I'm ignoring the reported percentage
            }
        }

        //Done on background thread
        static private void UploadFile(string path)
        {
            Byte[] contents;
            try
            {
                contents = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Debug.Print("Failed to read file contents.  "+ ex.Message);
                return;
            }
            var db = new FishTaggingDataContext();
            var file = new RawDataFile
            {
                FileName = Path.GetFileName(path),
                FolderName = Path.GetDirectoryName(path),
                Contents = contents
            };
            db.RawDataFiles.InsertOnSubmit(file);
            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                Debug.Print("Failed to submit file.  " + ex.Message);
            }
        }

        //Done on UI thread
        private void UploadBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UploadProgressBar.PerformStep();
            UpdateProgressLabel();
            RefreshDataGrid();
        }

        //Done on UI thread
        private void UploadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Uploading = false;
            if (HaveFolder)
                PrepareProgressBar();
            else
                ResetProgressBar();
            EnableControls();
            RefreshDataGrid();
        }

    }
}
