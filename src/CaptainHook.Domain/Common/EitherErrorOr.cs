using System;

namespace CaptainHook.Domain.Common
{
    public abstract class EitherErrorOr
    {
    }

    public class EitherErrorOr<TData> : EitherErrorOr
    {
        public ErrorBase Error { get; }
        public TData Data { get; }
        public bool IsError => Error != null;

        public EitherErrorOr(ErrorBase error)
        {
            Error = error;
        }

        public EitherErrorOr(TData data)
        {
            Data = data;
        }

        public T Match<T>(Func<ErrorBase, T> leftFunc, Func<TData, T> rightFunc)
        {
            return IsError ? leftFunc(Error) : rightFunc(Data);
        }

        public TResult IfValid<TResult>(Func<TData, TResult> action)
        {
            return action(Data);
        }

        public static implicit operator EitherErrorOr<TData>(ErrorBase error) => new EitherErrorOr<TData>(error);

        public static implicit operator EitherErrorOr<TData>(TData data) => new EitherErrorOr<TData>(data);
    }
}
