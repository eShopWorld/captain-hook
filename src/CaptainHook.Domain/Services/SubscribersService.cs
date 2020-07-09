using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Repositories;

namespace CaptainHook.Domain.Services
{
    public class SubscribersService
    {
        private readonly ISubscriberRepository _subscriberRepository;

        public SubscribersService(ISubscriberRepository subscriberRepository)
        {
            _subscriberRepository = subscriberRepository;
        }

        public EitherErrorOr<Guid> AddSubscriber(SubscriberDto subscriber)
        {
            if (subscriber.Name == "error")
            {
                return new BusinessError("Error is not a valid name!");
            }

            return Guid.NewGuid();
        }

        public async Task<EitherErrorOr<IEnumerable<SubscriberEntity>>> GetByEvent(string eventName)
        {
            if (eventName == "error")
            {
                return new BusinessError("Invalid event name");
            }

            var subscribers = await _subscriberRepository.GetSubscribersListAsync(eventName);
            return subscribers;
        }
    }

    public class BusinessError
    {
        public string Message { get; }

        public BusinessError(string message)
        {
            Message = message;
        }
    }

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

    //public class AsyncEitherErrorOr<TData>
    //{
    //    public Task<BusinessError> Error { get; }
    //    public Task<TData> Data { get; }
    //    private readonly bool _isError;


    //    public AsyncEitherErrorOr(Task<BusinessError> error)
    //    {
    //        Error = error;
    //        _isError = true;
    //    }

    //    public AsyncEitherErrorOr(Task<TData> data)
    //    {
    //        Data = data;
    //        _isError = false;
    //    }

    //    //public T Match<T>(Func<BusinessError, T> leftFunc, Func<TData, T> rightFunc)
    //    //{
    //    //    return _isError ? leftFunc(Error) : rightFunc(Data);
    //    //}

    //    public TResult IfValid<TResult>(Func<Task<TData>, TResult> action)
    //    {
    //        return action(Data);
    //    }

    //    public static implicit operator AsyncEitherErrorOr<TData>(Task<BusinessError> error) => new AsyncEitherErrorOr<TData>(error);

    //    public static implicit operator AsyncEitherErrorOr<TData>(Task<TData> data) => new AsyncEitherErrorOr<TData>(data);

    //    public static implicit operator AsyncEitherErrorOr<TData>(BusinessError error) => new AsyncEitherErrorOr<TData>(Task.FromResult(error));

    //    public static implicit operator AsyncEitherErrorOr<TData>(TData data) => new AsyncEitherErrorOr<TData>(Task.FromResult(data));


    //    //public static implicit operator EitherErrorOr<TData>(TData right) => new EitherErrorOr<TData>(right);
    //}
}
