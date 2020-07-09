using System;

namespace CaptainHook.Domain.Services
{
    public class EitherErrorOr<TData>
    {
        public BusinessError Error { get; }
        public TData Data { get; }
        public bool IsError => Error != null;

        public EitherErrorOr(BusinessError error)
        {
            Error = error;
        }

        public EitherErrorOr(TData data)
        {
            Data = data;
        }

        public T Match<T>(Func<BusinessError, T> leftFunc, Func<TData, T> rightFunc)
        {
            return IsError ? leftFunc(Error) : rightFunc(Data);
        }

        public TResult IfValid<TResult>(Func<TData, TResult> action)
        {
            return action(Data);
        }

        public static implicit operator EitherErrorOr<TData>(BusinessError error) => new EitherErrorOr<TData>(error);

        public static implicit operator EitherErrorOr<TData>(TData data) => new EitherErrorOr<TData>(data);
    }
}
