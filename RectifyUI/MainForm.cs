using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RectifyLib;
using RectifyLib.Analysis;
using RectifyLib.Correct;
using RectifyUI.DataGridView;


namespace RectifyUI
{
    public partial class MainForm : Form
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MainForm));
        
        private readonly ProgressDialog _progressDialog;

        private readonly Analyser _analyser = new Analyser();
        private readonly Corrector _corrector = new Corrector();

        private readonly TaskScheduler _uiScheduler;
        private DateLimits _dateLimit;
        private CancellationToken _currentCancelToken = CancellationToken.None;

        public DateLimits DateLimit
        {
            get { return _dateLimit; }
            set
            {
                _dateLimit = value;
                dtpLimitsDateValue.Visible = _dateLimit != DateLimits.NoLimit;

                switch (_dateLimit)
                {
                    case DateLimits.ExactDate:
                        dtpLimitsDateValue.CustomFormat = "dd MMMM yyyy";
                        dtpLimitsDateValue.ShowUpDown = false;
                        break;
                    case DateLimits.MonthAndYear:
                        dtpLimitsDateValue.CustomFormat = "MMMM yyyy";
                        dtpLimitsDateValue.ShowUpDown = true;
                        break;
                    case DateLimits.Year:
                        dtpLimitsDateValue.CustomFormat = "yyyy";
                        dtpLimitsDateValue.ShowUpDown = true;
                        break;
                }
            }
        }

        /*
         TODO:
            X Right click menu to check/uncheck all selected items in the grid or when multiple items are selected a single check will toggle them all
            - Destination folders will also retain any suffix naming that the originating folder had (e.g. 2013-05-05 Hulda gusti london)
            - Type and Error icons are wrong
            X Correct casing in file ending column (all lower case)
            - Show rationale for date selection and detection in column
            - Allow inclusion/exclusion of columns in grid and save its state
            - Consider master-detail grid (source folder or destination folder tree)
            X Report process and don't lock up while program is scanning folders
            X Show x of n folders scanned next to progress bar
            - Exclusion of certain file endings (zip, thm etc)
            - Exclusion of certain folders
            - Allow filtering of grid
                - Show only conflicts in source folder *e.g. files that need to be renamed
                - Show possible duplicates (in source folder)
            X Show total file count found that need rectification
            - Make analyse and rectify buttons bigger and colored
            X Program Icon!
            - Create a reverse file, meaning log all the actions taken, source to destination copy so that the action can be undoed
            X Validate date check for snapchat and photogrid pictures and pictures which name cannot be used to detect date
            - Extend the filename date extraction to handle more prefixes
                - 2008-10-05\060920081906.jpg
                - 
            - Attempt to detect which program took picture (e.g. is there meta info or file name pattern that can be read?) possible filter option as well
            - Allow user to mark images as "incorrectly identified as needing repair" store this information for future use and ensure that these
              suggestions do not show up again
            - Implement a "diff" feature so that the app will only attempt to process new files created in the location since last run
                - Store last run in a file in root folder and some validation hashes etc to detect folder changes
            - New columns
                - File size
                - File complete metadata dump
                - ExtInf raw information
            - Store previous x locations scanned and allow user to select from dropdown
            - Filter based on file ending
            X Show folder on disk option
            - Double click file entry to open file in default system viewer
            X Right click menu item to open file properties window for selected file(s) http://stackoverflow.com/a/1281485/779521
            - Confirm close if user has un-rectified files in the view when closing
            X Limit rectification to a particular date range. Meaning that I am only interested in finding pictures that should belong to May 2016 that may have been misplaced.
            X About window with link to page
            X When exiting application any currently running background process should be terminated.
            X Version numbering
            X Display version numbering in taskbar and about window
             */

        public MainForm()
        {
            InitializeComponent();

            if (this.DesignMode)
                return;

            // Create the progress dialog and attach it to the form's disposal container for auto-cleanup
            _progressDialog = new ProgressDialog();
            _progressDialog.Cancelled += _progressDialog_Cancelled; ;

            // Hide all controls we need in the beginning
            toolStripProgress.Visible = false;
            toolStripProgresslbl.Visible = false;
            toolStripCancelBtn.Visible = false;

            // Must create the synchronization context here, otherwise it will not be correct
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            // We want to report on the background progress of the background workers
            _analyser.BackgroundProgress += _analyser_AnalysisProgress;
            _corrector.BackgroundProgress += _corrector_BackgroundProgress;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Append the version number to the form title
            var version = typeof(MainForm).Assembly.GetName().Version;
            if (version != null)
                this.Text += $" | v{version}";

            DateLimit = DateLimits.NoLimit;
        }

        private void _progressDialog_Cancelled(object sender, EventArgs e)
        {
            // Just cancel either or both doesn't matter which
            _analyser?.CancelAsync();
            _corrector?.CancelAsync();
        }

        private void UpdateProgress(int totalSteps, int currentStep)
        {
            if (totalSteps <= 0)
                return;

            // Set the necessary control values for the bar (safer to do every time)
            toolStripProgress.Maximum = totalSteps;
            toolStripProgress.Minimum = 0;
            toolStripProgress.Visible = true;

            // Advance the progress bar
            toolStripProgress.Value = currentStep;
        }

        private void ShowProgressAndDisableUI(string operationTitle, string initialMessage)
        {
            // Show the progress indicators
            toolStripProgress.Visible = true;
            toolStripProgresslbl.Visible = true;
            toolStripCancelBtn.Visible = true;

            // Disable the UI 
            splitMain.Enabled = false;

            lblAnalysisResults.Text = initialMessage;

            if (_progressDialog.State == ProgressDialogState.Stopped)
            {
                _progressDialog.Title = operationTitle;
                _progressDialog.CancelButton = true;
                //_progressDialog.Modal = true;
                _progressDialog.ShowTimeRemaining = false;
                _progressDialog.Show(this);

                _progressDialog.Line1 = initialMessage;
                _progressDialog.Line2 = "Starting...";
                //_progressDialog.CompactPaths = true;
            }
        }

        private void ShowProgressDialogMessage(string title = null, string line1 = null, string line2 = null, string line3 = null, int currentStep = -1, int totalSteps = -1)
        {
            if (_progressDialog.State != ProgressDialogState.Stopped)
            {
                if( title != null )
                    _progressDialog.Title = title;
                if( line1 != null )
                    _progressDialog.Line1 = line1;
                if (line2 != null)
                    _progressDialog.Line2 = line2;
                if (!_progressDialog.ShowTimeRemaining && line3 != null)
                    _progressDialog.Line3 = line3;

                if (totalSteps != -1)
                    _progressDialog.Maximum = totalSteps;
                if (currentStep != -1)
                    _progressDialog.Value = currentStep;
            }
        }

        private void HideProgressAndEnableUI()
        {
            // Hide the progress indicators
            toolStripProgress.Visible = false;
            toolStripProgresslbl.Visible = false;
            toolStripCancelBtn.Visible = false;

            // Enable the UI again
            splitMain.Enabled = true;

            lblAnalysisResults.Text = "";

            if (_progressDialog.State != ProgressDialogState.Stopped)
                _progressDialog.Close();
        }

        private void btnAnalyseLibrary_Click(object sender, EventArgs e)
        {
            var dirpath = tbPicasaLibraryPath.Text?.Trim();

            if (string.IsNullOrWhiteSpace(dirpath) || !Directory.Exists(dirpath))
            {
                MessageBox.Show("Directory does not exist.", "Invalid Picasa Library Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable all the interactable UI
            ShowProgressAndDisableUI("Analysing Library", "Pre-Analysis in progress, please wait");

            _analyser.RunAsync(new AnalyserStartupArgs(dirpath, DateLimit, dtpLimitsDateValue.Value)).ContinueWith(task =>
            {
                try
                {
                    lblAnalysisResults.Text = "Failure!";

                    if (task.IsCanceled)
                    {
                        toolStripStatuslbl.Text = "Analysing Cancelled";
                    }
                    else if (task.IsFaulted)
                    {
                        toolStripStatuslbl.Text = "Analysing Error";
                    }
                    else if (task.IsCompleted)
                    {
                        toolStripStatuslbl.Text = "Analysing Completed";
                        PopulateGrid(task.Result);
                        lblAnalysisResults.Text = $"Analysis Complete: {dataGridMain.Rows.Count} potential errors found";
                    }
                }
                catch (Exception ex)
                {
                    // TODO: show errors, how?
                }
                finally
                {
                    HideProgressAndEnableUI();
                }
            }, scheduler: _uiScheduler, continuationOptions: TaskContinuationOptions.AttachedToParent, cancellationToken: CancellationToken.None);
        }

        private void _analyser_AnalysisProgress(object sender, AnalyserProgressArgs e)
        {
            lblAnalysisResults.Text = "Analysis in progress";

            string line1 = $"Analysing file {e.CurrentStep:#,##0} of {e.TotalSteps:#,##0}";
            string line2 = $"{e.DirectoryName} > {Path.GetFileName(e.FilePath)}";

            string line3 = e.RemainingTime.TotalMinutes < 1 
                             ? $"About {e.RemainingTime.TotalSeconds:#,##0} seconds remaining" 
                             : $"About {e.RemainingTime.TotalMinutes:#,##0} minutes remaining";

            UpdateProgress(e.TotalSteps, e.CurrentStep);
            ShowProgressDialogMessage(line1: line1, 
                                      line2: line2, 
                                      line3: line3, 
                                      currentStep: e.CurrentStep, 
                                      totalSteps: e.TotalSteps);

            toolStripProgresslbl.Visible = true;
            toolStripProgresslbl.Text = line3;
            toolStripCancelBtn.Visible = true;

            // Update the progress message
            toolStripStatuslbl.Text =  line1 + " | "+ line2;
        }

        #region Grid Updating and Interaction Handling

        private void PopulateGrid(AnalysisResults result)
        {
            this.dataGridMain.SuspendDrawing();
            try
            {
                this.dataGridMain.Rows.Clear();

                foreach (var dir in result.Directories)
                {
                    foreach (var file in dir.Files)
                    {
                        if (file.IsCorrect)
                            continue;

                        var row = (DataGridViewRow) dataGridMain.RowTemplate.Clone();
                        row.CreateCells(dataGridMain);
                        row.Tag = file;

                        row.Cells[colSelected.Index].Value = false; // Initialize to ensure that we don't get a nullreference exception later!
                        row.Cells[colFileName.Index].Value = Path.GetFileName(file.FilePath);
                        row.Cells[colSource.Index].Value = file.FilePath;
                        row.Cells[colDestination.Index].Value = file.CorrectedFilePath;
                        row.Cells[colDate.Index].Value = file.DateCategoryDetected;
                        row.Cells[colFileTypeText.Index].Value = Path.GetExtension(file.FilePath)?.Trim('.').ToLower();
                        //row.Cells[colFileTypeImage.Name].Value = 


                        dataGridMain.Rows.Add(row);
                    }
                }

                // Store the results that are being displayed as a part of the grid
                this.dataGridMain.Tag = result;
            }
            finally
            {
                this.dataGridMain.ResumeDrawing(true);
            }
        }

        private void ResetMainGrid()
        {
            this.dataGridMain.SuspendDrawing();
            try
            {
                this.dataGridMain.Rows.Clear();

                // Remove the results
                this.dataGridMain.Tag = null;
            }
            finally
            {
                this.dataGridMain.ResumeDrawing(true);
            }
        }

        private void toolStripCancelBtn_ButtonClick(object sender, EventArgs e)
        {
            _analyser.CancelAsync();
        }

        private void dataGridMain_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            // End of edition on each click on column of checkbox
            if (e.ColumnIndex == colSelected.Index && e.RowIndex != -1)
                ToggleCheckedForRow(e.RowIndex);
        }

        private void ToggleCheckedForRow(int rowIndex, bool? isChecked = null)
        {
            ToggleCheckedForRow(new[] {rowIndex}, isChecked);
        }

        private void ToggleCheckedForRow(int[] rowIndexes, bool? isChecked = null)
        {
            foreach (var rowIndex in rowIndexes)
            {
                if (rowIndex == -1 || dataGridMain.RowCount <= rowIndex)
                    continue;


                DataGridViewCell cell = this.dataGridMain.Rows[rowIndex].Cells[colSelected.Index];

                // If the checked parameter is set then assign that value, otherwise toggle the checkmark
                if (isChecked.HasValue)
                    cell.Value = isChecked;
                else
                    cell.Value = cell.Value == null ? true : !(bool) cell.Value;
            }

            UpdateCheckAllValueBasedOnCheckedStatesInMainGrid();
            this.dataGridMain.EndEdit();
        }

        private void UpdateCheckAllValueBasedOnCheckedStatesInMainGrid()
        {
            // Now figure out the state of the "select all" checkbox
            int selectedCount = this.dataGridMain.Rows.Cast<DataGridViewRow>().Count(row => (bool) row.Cells[colSelected.Index].Value);
            if (selectedCount <= 0)
                cbSelectAllRows.CheckState = CheckState.Unchecked;
            else if (selectedCount == this.dataGridMain.RowCount)
                cbSelectAllRows.CheckState = CheckState.Checked;
            else
                cbSelectAllRows.CheckState = CheckState.Indeterminate;
        }

        private void cbSelectAllRows_CheckedChanged(object sender, EventArgs e)
        {
            // Don't do anything if we're dealing with the "third state" 
            // (.Checked will return true in this case and mess everything up)
            if (cbSelectAllRows.CheckState == CheckState.Indeterminate)
                return;

            // Either select or deselect stuff
            bool selected = cbSelectAllRows.Checked;

            this.dataGridMain.SuspendDrawing();
            try
            {
                foreach (DataGridViewRow row in this.dataGridMain.Rows)
                {
                    row.Cells[colSelected.Index].Value = selected;
                }
            }
            finally
            {
                this.dataGridMain.ResumeDrawing(true);
            }
        }

        #endregion

        private void btnRectifySelected_Click(object sender, EventArgs e)
        {
            var results = this.dataGridMain.Tag as AnalysisResults;
            if (results == null)
            {
                MessageBox.Show("No results available");
                return;
            }

            // Collect the selected results from the grid
            var selectedFiles = GetCheckedRowsInGrid().Select(row => row.Tag as FileAnalysis).ToArray();

            RunCorrector(selectedFiles);
        }

        /// <summary>
        /// Returns all rows in the grid that the user has placed a checkmark against. 
        /// These cells may or may not be a part of the current selection of rows in the grid.
        /// To only get the selected cells use <see cref="GetSelectedRowsInGrid"/>.
        /// </summary>
        private IEnumerable<DataGridViewRow> GetCheckedRowsInGrid()
        {
            return this.dataGridMain.Rows.OfType<DataGridViewRow>().Where(row =>
            {
                var value = row.Cells[colSelected.Index].Value;
                return value != null && (bool) value;
            });
        }

        /// <summary>
        /// Returns all rows that are selected in the grid. This indicates rows that the user has clicked and are highlighted.
        /// For checked cells use <see cref="GetCheckedRowsInGrid"/>.
        /// </summary>
        private IEnumerable<DataGridViewRow> GetSelectedRowsInGrid()
        {
            return this.dataGridMain.SelectedRows.OfType<DataGridViewRow>();
        }

        private void RunCorrector(FileAnalysis[] selectedFiles)
        {
            // If nothing is selected exit
            if (selectedFiles == null || selectedFiles.Length <= 0)
                return;

            // Disable all the interactable UI
            ShowProgressAndDisableUI("Correcting library files", "Correcting file locations");

            _corrector.RunAsync(selectedFiles).ContinueWith(task =>
            {
                try
                {
                    if (task.IsCanceled)
                    {
                        toolStripStatuslbl.Text = "Correction Cancelled";
                    }
                    else if (task.IsFaulted)
                    {
                        toolStripStatuslbl.Text = "Correction Error";
                    }
                    else if (task.IsCompleted)
                    {
                        toolStripStatuslbl.Text = "Correction Completed";
                        ResetMainGrid();
                    }
                }
                catch (Exception ex)
                {
                    // TODO: show errors, how?
                }
                finally
                {
                    HideProgressAndEnableUI();
                }
            }, scheduler: _uiScheduler, continuationOptions: TaskContinuationOptions.AttachedToParent, cancellationToken: CancellationToken.None);
        }

        private void _corrector_BackgroundProgress(object sender, CorrectorProgressArgs e)
        {
            string line1 = $"Moving {e.TotalSteps} items";
            string line2 = $"from {Path.GetFileName(e.FilePathFrom)} to {Path.GetDirectoryName(e.FilePathTo)}";

            UpdateProgress(e.TotalSteps, e.CurrentStep);
            ShowProgressDialogMessage(line1: line1,
                                      line2: line2,
                                      line3: line1,
                                      currentStep: e.CurrentStep,
                                      totalSteps: e.TotalSteps);

            toolStripProgresslbl.Visible = true;
            toolStripProgresslbl.Text = line1;
            toolStripCancelBtn.Visible = true;

            // Update the progress message
            toolStripStatuslbl.Text = line1 + " " + line2;
        }

        private void dataGridMain_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                HandleGridRightClick(e);
            }
        }


        private void HandleGridRightClick(MouseEventArgs mouseEvent)
        {
            // Clear the tag first
            ctxMenuGrid.Tag = null;

            // When the right mouse button is clicked on the grid I populate the Tag for the context menu with the selected cells or if none are selected
            // the row that is directly under the mouse cursor
            if (this.dataGridMain.SelectedRows.Count <= 0)
            {
                // Find out what row is under the mouse
                var rowIndex = this.dataGridMain.HitTest(mouseEvent.X, mouseEvent.Y).RowIndex;
                if (rowIndex == -1) // No row available
                    return;

                // Clear all other selections before making a new selection
                this.dataGridMain.ClearSelection();

                // Select the found DataGridViewRow
                this.dataGridMain.Rows[rowIndex].Selected = true;
            }

            // Now get all selected rows and extract the Tag from them
            ctxMenuGrid.Tag = GetSelectedRowsInGrid().Select(row => row.Tag as FileAnalysis).ToArray();
        }

        private void ctxMenuItemShowFileProperties_Click(object sender, EventArgs e)
        {
            var files = ctxMenuGrid.Tag as FileAnalysis[];
            if (files == null || files.Length <= 0)
                return;

            Win32.OpenFileProperties(files.Select(x => x.FilePath).First());
        }

        private void ctxMenuItemLocateFileOnDisk_Click(object sender, EventArgs e)
        {
            var files = ctxMenuGrid.Tag as FileAnalysis[];
            if (files == null || files.Length <= 0)
                return;

            Win32.OpenFileLocation(files.Select(x => Path.GetDirectoryName(x.FilePath)).First());
        }

        private void ctxMenuItemCheckSelected_Click(object sender, EventArgs e)
        {
            ToggleCheckedForRow(GetSelectedRowsInGrid().Select(x => x.Index).ToArray(), true);
        }

        private void ctxMenuItemUncheckSelected_Click(object sender, EventArgs e)
        {
            ToggleCheckedForRow(GetSelectedRowsInGrid().Select(x => x.Index).ToArray(), false);
        }

        private void rbLimitNoLimits_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLimitNoLimits.Checked)
                DateLimit = DateLimits.NoLimit;
        }

        private void rbLimitDate_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLimitDate.Checked)
                DateLimit = DateLimits.ExactDate;
        }

        private void rbLimitMonth_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLimitMonth.Checked)
                DateLimit = DateLimits.MonthAndYear;
        }

        private void rbLimitYear_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLimitYear.Checked)
                DateLimit = DateLimits.Year;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _analyser?.CancelAsync();
            _corrector?.CancelAsync();
        }

        private void mItemAboutRectify_Click(object sender, EventArgs e)
        {
            using (AboutForm about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        private void mItemExitApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
