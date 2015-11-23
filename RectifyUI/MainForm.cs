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

        private readonly Analyser _analyser = new Analyser();
        private readonly Corrector _corrector = new Corrector();

        private readonly TaskScheduler _uiScheduler;

        public MainForm()
        {
            InitializeComponent();

            if (this.DesignMode)
                return;

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

        private void HideProgressAndEnableUI()
        {
            // Hide the progress indicators
            toolStripProgress.Visible = false;
            toolStripProgresslbl.Visible = false;
            toolStripCancelBtn.Visible = false;

            // Enable the UI again
            splitMain.Enabled = true;
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
            splitMain.Enabled = false;

            _analyser.RunAsync(dirpath)
                     .ContinueWith(
                        task =>
                        {
                            try
                            {
                                if (task.IsCanceled)
                                {
                                    toolStripStatuslbl.Text = "Cancelled";
                                }
                                else if (task.IsFaulted)
                                {
                                    toolStripStatuslbl.Text = "Error";
                                }
                                else if (task.IsCompleted)
                                {
                                    toolStripStatuslbl.Text = "Completed";
                                    PopulateGrid(task.Result);
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
                        },
                        scheduler: _uiScheduler,
                        continuationOptions: TaskContinuationOptions.AttachedToParent, 
                        cancellationToken:CancellationToken.None);
        }

        private void _analyser_AnalysisProgress(object sender, AnalyserProgressArgs e)
        {
            UpdateProgress(e.TotalSteps, e.CurrentStep);

            toolStripProgresslbl.Visible = true;
            toolStripProgresslbl.Text = "Analysing";
            toolStripCancelBtn.Visible = true;

            // Update the progress message
            toolStripStatuslbl.Text = $"{e.DirectoryName} > {Path.GetFileName(e.FilePath)}";
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
                        row.Cells[colFileTypeText.Index].Value = Path.GetExtension(file.FilePath)?.Trim('.');
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
            {
                DataGridViewCell cell = this.dataGridMain.Rows[e.RowIndex].Cells[colSelected.Index];
                cell.Value = cell.Value == null ? true : !(bool)cell.Value;

                UpdateCheckAllValueBasedOnCheckedStatesInMainGrid();

                this.dataGridMain.EndEdit();
            }
        }

        private void UpdateCheckAllValueBasedOnCheckedStatesInMainGrid()
        {
            // Now figure out the state of the "select all" checkbox
            int selectedCount = this.dataGridMain.Rows.Cast<DataGridViewRow>().Count(row => (bool)row.Cells[colSelected.Index].Value);
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
            var selectedFiles = this.dataGridMain.Rows.OfType<DataGridViewRow>()
                                                      .Where(row =>
                                                                {
                                                                    var value = row.Cells[colSelected.Index].Value;
                                                                    return value != null && (bool) value;
                                                                })
                                                      .Select(row => row.Tag as FileAnalysis)
                                                      .ToArray();

            RunCorrector(selectedFiles);
        }

        private void RunCorrector(FileAnalysis[] selectedFiles)
        {
            // Disable all the interactable UI
            splitMain.Enabled = false;

            _corrector.RunAsync(selectedFiles)
                      .ContinueWith(
                task =>
                {
                    try
                    {
                        if (task.IsCanceled)
                        {
                            toolStripStatuslbl.Text = "Cancelled";
                        }
                        else if (task.IsFaulted)
                        {
                            toolStripStatuslbl.Text = "Error";
                        }
                        else if (task.IsCompleted)
                        {
                            toolStripStatuslbl.Text = "Completed";
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
                },
                scheduler: _uiScheduler,
                continuationOptions: TaskContinuationOptions.AttachedToParent,
                cancellationToken: CancellationToken.None);
        }

        private void _corrector_BackgroundProgress(object sender, CorrectorProgressArgs e)
        {
            UpdateProgress(e.TotalSteps, e.CurrentStep);

            toolStripProgresslbl.Visible = true;
            toolStripProgresslbl.Text = "Moving Files";
            toolStripCancelBtn.Visible = true;

            // Update the progress message
            toolStripStatuslbl.Text = $"Moving {Path.GetFileName(e.FilePathFrom)} to {Path.GetDirectoryName(e.FilePathTo)}";
        }
    }
}
