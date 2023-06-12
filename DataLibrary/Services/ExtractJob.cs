namespace DataLibrary.Services
{
    public class ExtractJob
    {
        public CancellationTokenSource TokenSource { get; set; }
        public Task Worker { get; set; }
        public string County { get; set; }
        public bool Running
        {
            get
            {
                return Worker != null && !Worker.IsCompleted;
            }
        }
    }
}
