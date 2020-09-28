namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class PutSubscriberFile
    {
        public string Filename { get; }

        public PutSubscriberRequest Request { get; }

        public string Error { get; }

        public bool IsError => !string.IsNullOrEmpty(Error);

        private PutSubscriberFile(string filename)
        {
            Filename = filename;
        }

        public PutSubscriberFile(string filename, string error): this(filename)
        {
            Error = error;
        }

        public PutSubscriberFile(string filename, PutSubscriberRequest request) : this(filename)
        {
            Request = request;
        }
    }
}