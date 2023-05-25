namespace DataLibrary.Extractors
{
    public class GARRExtractController : IDisposable
    {
        public event EventHandler JobFinishedEvent;
        private CancellationTokenSource _tokenSource;

        private Task _worker;

        public bool Running
        {
            get
            {
                return _worker != null && !_worker.IsCompleted;
            }
        }

        public Task StartGARRExtract(GARRExtractor extractor, int amount)
        {
            if (_worker == null || _worker.IsCompleted)
            {
                _tokenSource = new CancellationTokenSource();
                _worker = Task.Run(async () =>
                {
                    await extractor.Extract(amount, _tokenSource.Token);
                });

                _worker.ContinueWith((o) =>
                {
                    OnJobFinished();
                });
            }
            return Task.CompletedTask;
        }

        internal void OnJobFinished()
        {
            if (JobFinishedEvent != null)
            {
                JobFinishedEvent(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            if (Running && _tokenSource != null)
            {
                _tokenSource.Cancel();
            }
        }
    }
}
