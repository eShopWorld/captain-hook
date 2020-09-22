using System;
using System.Threading.Tasks;

namespace CaptainHook.Domain.Results
{
    public class OperationResult<TData>
    {
        public ErrorBase Error { get; }
        public TData Data { get; }
        public bool IsError => Error != null;

        public OperationResult(ErrorBase error)
        {
            Error = error;
        }

        public OperationResult(TData data)
        {
            Data = data;
        }

        public T Match<T>(Func<ErrorBase, T> leftFunc, Func<TData, T> rightFunc)
        {
            return IsError ? leftFunc(Error) : rightFunc(Data);
        }

        public OperationResult<TResult> Then<TResult>(Func<TData, OperationResult<TResult>> func)
        {
            return IsError ? Error : func(Data);
        }

        public async Task<OperationResult<TResult>> Then<TResult>(Func<TData, Task<OperationResult<TResult>>> func)
        {
            return IsError ? Error : await func(Data);
        }

        public static implicit operator OperationResult<TData>(ErrorBase error) => new OperationResult<TData>(error);

        public static implicit operator OperationResult<TData>(TData data) => new OperationResult<TData>(data);

        public static implicit operator TData(OperationResult<TData> result) => result.Data;
    }
}
