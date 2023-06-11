using DataLibrary.DbServices;

namespace DataLibrary.Services
{
    public class ExtractController : IDisposable
    {
        public event EventHandler<JobFinishedEventArgs>? JobFinished;

        private Dictionary<string, ExtractJob> _jobs;

        public ExtractController()
        {
            this._jobs = new Dictionary<string, ExtractJob>();
        }

        public bool Running(string county)
        {
            return _jobs.ContainsKey(county) && _jobs[county].Running;         
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
            if (!_jobs.ContainsKey(county) || _jobs[county].Worker.IsCompleted)
            {
                var job = new ExtractJob();

                job.TokenSource = new CancellationTokenSource();
                job.County = county;
                job.Worker = Task.Run(async () =>
                {
                    await extractor.Extract(
                        dataServiceFactory,
                        county,
                        firefoxDriverPath,
                        geckoDriverPath,
                        dropDownSelect,
                        blobContainer,
                        amount,
                        job.TokenSource.Token);
                });
                job.Worker.ContinueWith((o) =>
                {
                    this.OnJobFinished(new JobFinishedEventArgs() { County = county });
                });

                if (_jobs.ContainsKey(county))
                {
                    _jobs[county] = job;
                }
                else
                {
                    _jobs.Add(county, job);
                }
            }
            return Task.CompletedTask;
        }

        internal void OnJobFinished(JobFinishedEventArgs args)
        {
            JobFinished?.Invoke(this, args);
        }

        public void Cancel(string county)
        {
            if (Running(county) && _jobs[county].TokenSource != null)
            {
                _jobs[county].TokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            foreach(var job in _jobs.Values)
            {
                if (job.Running && job.TokenSource != null)
                {
                    job.TokenSource.Cancel(); 
                }
            }
        }
    }
}
