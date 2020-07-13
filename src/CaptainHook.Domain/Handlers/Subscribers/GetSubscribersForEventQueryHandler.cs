using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Contract;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Requests.Subscribers;
using CaptainHook.Domain.Results;
using MediatR;

namespace CaptainHook.Domain.Handlers.Subscribers
{
    public class GetSubscribersForEventQueryHandler : IRequestHandler<GetSubscribersForEventQuery, EitherErrorOr<List<SubscriberDto>>>
    {
        private readonly ISubscriberRepository _repository;

        public GetSubscribersForEventQueryHandler(ISubscriberRepository repository)
        {
            _repository = repository;
        }

        public async Task<EitherErrorOr<List<SubscriberDto>>> Handle(GetSubscribersForEventQuery query, CancellationToken cancellationToken)
        {
            if (query.Name == "error")
            {
                return new BusinessError("Error is not a valid name!");
            }

            var subscriberEntities = await _repository.GetSubscribersListAsync(query.Name);
            var dtos = subscriberEntities.IfValid(x => x.Select(s => new SubscriberDto { Name = s.Name }).ToList());
            return dtos;
        }
    }
}