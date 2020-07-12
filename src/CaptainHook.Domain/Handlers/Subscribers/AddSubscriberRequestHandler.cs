using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Contract;
using CaptainHook.Domain.Common;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Requests;
using MediatR;

namespace CaptainHook.Domain.Handlers.Subscribers
{
    public class AddSubscriberRequestHandler : IRequestHandler<AddSubscriberRequest, EitherErrorOr<Guid>>
    {
        public async Task<EitherErrorOr<Guid>> Handle(AddSubscriberRequest request, CancellationToken cancellationToken)
        {
            if (request.Name == "error")
            {
                return new BusinessError("Error is not a valid name!");
            }

            return Guid.NewGuid();
        }
    }

    public class GetSubscribersForEventQueryHandler : IRequestHandler<GetSubscribersForEventQuery, EitherErrorOr<List<SubscriberDto>>>
    {
        private ISubscriberRepository _repository;

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
