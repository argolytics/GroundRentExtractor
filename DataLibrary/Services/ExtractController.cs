using DataLibrary.DbServices;

namespace DataLibrary.Services
{
    public class ExtractController : IDisposable
    {
        public event EventHandler JobFinished;
        private CancellationTokenSource _tokenSource;

        private Task _worker;

        public bool Running
        {
            get
            {
                return _worker != null && !_worker.IsCompleted;
            }
        }

        public Task StartExtract(
            Extractor extractor,
            IDataServiceFactory dataServiceFactory,
            string county,
            string firefoxDriverPath,
            string geckoDriverPath,
            string dropDownSelect,
            string blobContainer,
            int amount)
        {
            if (_worker == null || _worker.IsCompleted)
            {
                _tokenSource = new CancellationTokenSource();
                _worker = Task.Run(async () =>
                {
                    await extractor.Extract(
                        dataServiceFactory,
                        county,
                        firefoxDriverPath,
                        geckoDriverPath,
                        dropDownSelect,
                        blobContainer,
                        amount,
                        _tokenSource.Token);
                });
                OnJobFinished();
            }
            return Task.CompletedTask;
        }

        internal void OnJobFinished()
        {
            JobFinished?.Invoke(this, new EventArgs());
        }

        public void Cancel()
        {
            if (Running && _tokenSource != null)
            {
                _tokenSource.Cancel();
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
