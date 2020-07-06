using System.Linq;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    public class ReaderServiceChangesDetectorTests
    {
        [Fact, IsUnit]
        public void DetectChanges_WithNoConfigsAndNoOldServices_ReturnsEmpty ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            // Act
            var changes = detector.DetectChanges (Enumerable.Empty<SubscriberConfiguration> (), Enumerable.Empty<string> ());

            // Assert
            Assert.Empty (changes);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithNoConfigsAndOldService_ReturnsServicesForRemoval ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            var service = ServiceNaming.EventReaderServiceFullUri ("my-different-event", "sub");

            // Act
            var changes = detector.DetectChanges (Enumerable.Empty<SubscriberConfiguration> (), new [] { service }).ToArray ();

            // Assert
            var expected = new ExistingReaderDefinition (service);

            Assert.Single (changes);
            Assert.Equal (ReaderChangeType.ToBeRemoved, changes[0].ChangeType);
            Assert.Equal (expected, changes[0].OldReader);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithNoConfigsAndIrrelevantService_ReturnsEmpty ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            var services = new [] 
            {
                ServiceNaming.EventHandlerServiceFullName,
                ServiceNaming.DirectorServiceFullName,
                "some-other-random-name",
                ""
            };

            // Act
            var changes = detector.DetectChanges (Enumerable.Empty<SubscriberConfiguration> (), services);

            // Assert
            Assert.Empty (changes);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithNewConfigAndNoOldServices_ReturnsServicesToAdd ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            // Act
            var changes = detector.DetectChanges (new [] { config }, Enumerable.Empty<string> ()).ToArray ();

            // Assert
            var expected = new DesiredReaderDefinition (config);

            Assert.Single (changes);
            Assert.Equal (ReaderChangeType.ToBeCreated, changes[0].ChangeType);
            Assert.Equal (expected, changes[0].NewReader);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithNewConfigsAndDifferentServices_ReturnsServicesToAddAndRemove ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";
            var existingService = ServiceNaming.EventReaderServiceFullUri ("my-different-event", "sub");

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            // Act
            var changes = detector.DetectChanges (new[] { config }, new [] { existingService }).ToArray ();

            // Assert
            var expectedAdd = new DesiredReaderDefinition (config);
            var expectedRemove = new ExistingReaderDefinition (existingService);

            Assert.Equal (2, changes.Length);

            var added = changes.First (c => c.ChangeType == ReaderChangeType.ToBeCreated);
            Assert.Equal (expectedAdd, added.NewReader);

            var removed = changes.First (c => c.ChangeType == ReaderChangeType.ToBeRemoved);
            Assert.Equal (expectedRemove, removed.OldReader);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithNewConfigAndTheSameService_ReturnsServiceToUpdate ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var newConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            var existingConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("some-random-uri")
                .Create ();

            var existingService = new DesiredReaderDefinition (existingConfig).ServiceNameWithSuffix;

            // Act
            var changes = detector.DetectChanges (new[] { newConfig }, new[] { existingService }).ToArray ();

            // Assert
            var expectedAdd = new DesiredReaderDefinition (newConfig);
            var expectedRemove = new ExistingReaderDefinition (existingService);

            Assert.Single (changes);
            Assert.Equal (ReaderChangeType.ToBeUpdated, changes[0].ChangeType);
            Assert.Equal (expectedRemove, changes[0].OldReader);
            Assert.Equal (expectedAdd, changes[0].NewReader);
        }

        [Fact, IsUnit]
        public void DetectChanges_WithUnchangedConfig_ReturnsEmpty ()
        {
            // Arrange
            var detector = new ReaderServiceChangesDetector ();

            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var existingConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("some-random-uri")
                .Create ();

            var existingService = new DesiredReaderDefinition (existingConfig).ServiceNameWithSuffix;

            // Act
            var changes = detector.DetectChanges (new[] { existingConfig }, new[] { existingService }).ToArray ();

            // Assert
            Assert.Empty (changes);
        }

    }
}
