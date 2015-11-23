using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RectifyLib
{
    public abstract class BackgroundProgressArgs : EventArgs
    {
        /// <summary>
        /// The total number of steps that the process will perform. 
        /// Must be greater than zero.
        /// </summary>
        public int TotalSteps { get; }

        /// <summary>
        /// The current number of step that the process is at.
        /// Must be lower or equal to <see cref="TotalSteps"/>
        /// </summary>
        public int CurrentStep { get; }

        protected BackgroundProgressArgs(int currentStep, int totalSteps)
        {
            Contract.Requires(currentStep <= totalSteps);
            Contract.Requires(totalSteps > 0);

            CurrentStep = currentStep;
            TotalSteps = totalSteps;
        }
    }

    public abstract class AsyncBackgroundProcessor<TResult, TProgressEventArgs, TProcessArgs> where TProgressEventArgs : BackgroundProgressArgs
    {
        private CancellationTokenSource _cancelSource = null;

        #region Events

        public event EventHandler<TProgressEventArgs> BackgroundProgress;

        /// <summary>
        /// Raises the <see cref="BackgroundProgress"/> event that signals a change in the current progress of the operation to any listeners.
        /// </summary>
        /// <param name="e">The progress args</param>
        protected virtual void OnBackgroundProgress(TProgressEventArgs e)
        {
            BackgroundProgress?.Raise(this, e);
        }

        #endregion

        /// <summary>
        /// Runs an the process asynchronously for the given <see cref="args"/> arguments.
        /// </summary>
        /// <param name="args">The arguments to the background process</param>
        public Task<TResult> RunAsync(TProcessArgs args)
        {
            // Create a new cancellation source for the task
            _cancelSource?.Dispose();
            _cancelSource = new CancellationTokenSource();
            var cancellationToken = _cancelSource.Token;

            return new TaskFactory<TResult>().StartNew(CreateAsyncProcess(args, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// This function returns a functor that defines the background action to take
        /// </summary>
        /// <returns></returns>
        protected abstract Func<TResult> CreateAsyncProcess(TProcessArgs args, CancellationToken cancellationToken);

        /// <summary>
        /// Cancel an ongoing background job. If nothing is running or another cancellation is already 
        /// pending calling this funciton will have no effect.
        /// </summary>
        public void CancelAsync()
        {
            if (_cancelSource != null && !_cancelSource.IsCancellationRequested)
                _cancelSource.Cancel();
        }
    }
}
