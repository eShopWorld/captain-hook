namespace CaptainHook.Domain.Results
{
    public abstract class FailureBase
    {
        public abstract string Id { get; }
        //public string Code { get; }
        //public string Message { get; }

        //protected FailureBase(string code, string message)
        //{
        //    Code = code;
        //    Message = message;
        //}
    }

    public class Failure : FailureBase
    {
        public override string Id => Code;
        public string Code { get; }
        public string Message { get; }
        //public string Property { get; }

        public Failure(string code, string message)
        {
            Code = code;
            Message = message;
        }

        //public Failure(string code, string message, string property)
        //{
        //    Code = code;
        //    Message = message;
        //    Property = property;
        //}
    }

    public class ValidationFailure : FailureBase
    {
        public override string Id => Code;
        public string Code { get; }
        public string Message { get; }
        public string Property { get; }

        //public Failure(string code, string message)
        //{
        //    Code = code;
        //    Message = message;
        //}

        public ValidationFailure(string code, string message, string property)
        {
            Code = code;
            Message = message;
            Property = property;
        }
    }
}