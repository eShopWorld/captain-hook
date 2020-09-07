namespace CaptainHook.Domain.Results
{
    public abstract class ErrorBase
    {
        public string Message { get; set; }
        public FailureBase[] Failures { get; set; }

        protected ErrorBase(string message, params FailureBase[] failures)
        {
            Message = message;
            Failures = failures;
        }

        //public override string ToString()
        //{
        //    return $"{Message} Failures:"
                
                
        //        .Select(x => $"{x.Id}, {x}").ToList();
        //}
    }
}