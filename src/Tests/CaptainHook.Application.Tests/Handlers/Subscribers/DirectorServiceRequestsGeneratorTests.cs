using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class DirectorServiceRequestsGeneratorTests
    {
        private readonly Mock<ISubscriberEntityToConfigurationMapper> _mapperMock = new Mock<ISubscriberEntityToConfigurationMapper>();

        private static readonly SubscriberEntity SubscriberWithoutDlq = new SubscriberBuilder()
            .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
            .Create();

        private static readonly SubscriberEntity SubscriberWithDlq = new SubscriberBuilder()
            .WithWebhook("https://blah-blah.eshopworld.com/webhook/", "PUT", "*")
            .WithDlqhook("https://blah-blah.eshopworld.com/dlq/", "PUT", "*")
            .Create();

        private DirectorServiceRequestsGenerator Generator => new DirectorServiceRequestsGenerator(_mapperMock.Object);

        public DirectorServiceRequestsGeneratorTests()
        {
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.Is<SubscriberEntity>(s => !s.HasDlqHooks))).ReturnsAsync(new SubscriberConfiguration { Name = "without-dlq" });
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.Is<SubscriberEntity>(s => s.HasDlqHooks))).ReturnsAsync(new SubscriberConfiguration { Name = "with-dlq" });
            _mapperMock.Setup(x => x.MapToDlqAsync(It.Is<SubscriberEntity>(s => s.HasDlqHooks))).ReturnsAsync(new SubscriberConfiguration { Name = "dlq" });
        }

        public static IEnumerable<object[]> FailureFlowsData
        {
            get
            {
                return new List<object[]>
                {
                    new object[] {SubscriberWithoutDlq, null, new MappingError("error"), null},
                    new object[] {SubscriberWithoutDlq, SubscriberWithoutDlq, new MappingError("error"), null},
                    new object[] {SubscriberWithoutDlq, SubscriberWithDlq, new MappingError("error"), null},
                    new object[] {SubscriberWithDlq, SubscriberWithDlq, new MappingError("error"), null},
                    new object[] {SubscriberWithDlq, SubscriberWithoutDlq, new MappingError("error"), null},
                    new object[] {SubscriberWithoutDlq, SubscriberWithDlq, new SubscriberConfiguration(), new MappingError("error")},
                    new object[] {SubscriberWithDlq, SubscriberWithDlq, new SubscriberConfiguration(), new MappingError("error")},
                    new object[] {SubscriberWithDlq, SubscriberWithoutDlq, new SubscriberConfiguration(), new MappingError("error")},
                };
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(FailureFlowsData))]
        public async void When_MapperReturnsError_ThenErrorIsReturned(
            SubscriberEntity subscriber,
            SubscriberEntity existingSubscriber,
            OperationResult<SubscriberConfiguration> webhookMappingResult,
            OperationResult<SubscriberConfiguration> dlqMappingResult)
        {
            _mapperMock.Setup(x => x.MapToWebhookAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(webhookMappingResult);
            _mapperMock.Setup(x => x.MapToDlqAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(dlqMappingResult);

            var result = await Generator.DefineChangesAsync(subscriber, existingSubscriber);

            result.IsError.Should().BeTrue();
        }


        [Fact, IsUnit]
        public async Task When_NewSubscriberWithoutDlq_ThenOnlyWebhookReaderCreated()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithoutDlq, null);

            result.Data.Single().Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("without-dlq");
        }

        [Fact, IsUnit]
        public async Task When_NewSubscriberWithDlq_ThenWebhookAndDlqReadersCreated()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithDlq, null);

            var array = result.Data.ToArray();
            array[0].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("with-dlq");
            array[1].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("dlq");
        }

        [Fact, IsUnit]
        public async Task When_UpdatingSubscriberWithoutDlq_ThenOnlyWebhookReaderUpdated()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithoutDlq, SubscriberWithoutDlq);

            result.Data.Single().Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("without-dlq");
        }

        [Fact, IsUnit]
        public async Task When_AddingDlqToExistingSubscriber_ThenWebhookUpdatedAndDlqReaderCreated()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithDlq, SubscriberWithoutDlq);

            var array = result.Data.ToArray();
            array[0].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("with-dlq");
            array[1].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("dlq");
        }

        [Fact, IsUnit]
        public async Task When_UpdatingDlqInExistingSubscriber_ThenWebhookUpdatedAndDlqReaderUpdated()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithDlq, SubscriberWithDlq);

            var array = result.Data.ToArray();
            array[0].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("with-dlq");
            array[1].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("dlq");
        }

        [Fact, IsUnit]
        public async Task When_RemovingDlqFromExistingSubscriber_ThenWebhookUpdatedAndDlqReaderRemoved()
        {
            var result = await Generator.DefineChangesAsync(SubscriberWithoutDlq, SubscriberWithDlq);

            var array = result.Data.ToArray();
            array[0].Should().BeOfType<UpdateReader>().Which.Subscriber.Name.Should().Be("without-dlq");
            array[1].Should().BeOfType<DeleteReader>().Which.Subscriber.Name.Should().Be("dlq");
        }
    }
}