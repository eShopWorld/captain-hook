using CaptainHook.Common;
using CaptainHook.Common.Remoting.Types;
using CaptainHook.DirectorService.ReaderServiceManagement;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Director.ReaderServiceManagement
{
    public class DesiredReaderDefinitionTests
    {
        [IsUnit, Theory]
        [InlineData (true)]
        [InlineData (false)]
        public void Ctor_WithValidConfiguration_CreatesValidServiceName (bool asDlq)
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .AsDLQ (asDlq)
                .Create ();

            // Act
            var definition = new DesiredReaderDefinition (config);

            // Assert
            var expectedName = ServiceNaming.EventReaderServiceFullUri (eventName, subscriberName, asDlq);

            Assert.Equal (expectedName, definition.ServiceName);
            Assert.StartsWith (definition.ServiceName, definition.ServiceNameWithSuffix);
        }

        [IsUnit, Fact]
        public void Ctor_WithValidConfiguration_CreatesValidDefinition ()
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            // Act
            var definition = new DesiredReaderDefinition (config);

            // Assert
            Assert.True (definition.IsValid);
            Assert.Equal (config, definition.SubscriberConfig);
            Assert.NotNull (definition.ServiceNameWithSuffix);
            Assert.NotNull (definition.ServiceName);
        }

        [Fact, IsUnit]
        public void IsTheSameService_ForTheSameDefinition_ReturnsTrue ()
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            var definition = new DesiredReaderDefinition (config);
            var existing = new ExistingReaderDefinition (definition.ServiceNameWithSuffix);

            // Act
            var result = definition.IsTheSameService (existing);

            // Assert
            Assert.True (result);
        }

        [Fact, IsUnit]
        public void IsTheSameService_ForDifferentServiceName_ReturnsFalse ()
        {
            // Arrange
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType ("event-one")
                .WithSubscriberName (subscriberName)
                .Create ();

            var definition = new DesiredReaderDefinition (config);

            var different = new SubscriberConfigurationBuilder ()
                .WithType ("different-event")
                .WithSubscriberName (subscriberName)
                .Create ();
            var existing = new ExistingReaderDefinition (new DesiredReaderDefinition (different).ServiceNameWithSuffix);

            // Act
            var result = definition.IsTheSameService (existing);

            // Assert
            Assert.False (result);
        }

        [Fact, IsUnit]
        public void IsTheSameService_ForTheServiceNameButDifferentVersion_ReturnsTrue ()
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("first-uri")
                .Create ();

            var definition = new DesiredReaderDefinition (config);

            var existingConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("different-uri")
                .Create ();

            var existingDefinition = new DesiredReaderDefinition (existingConfig);
            var existing = new ExistingReaderDefinition (existingDefinition.ServiceNameWithSuffix);

            // Act
            var result = definition.IsTheSameService (existing);

            // Assert
            Assert.Equal (definition.ServiceName, existingDefinition.ServiceName);
            Assert.True (result);
        }

        [Fact, IsUnit]
        public void IsUnchanged_ForTheServiceNameButDifferentVersion_ReturnsFalse ()
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("first-uri")
                .Create ();

            var definition = new DesiredReaderDefinition (config);

            var existingConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .WithUri ("different-uri")
                .Create ();

            var existingDefinition = new DesiredReaderDefinition (existingConfig);
            var existing = new ExistingReaderDefinition (existingDefinition.ServiceNameWithSuffix);

            // Act
            var result = definition.IsUnchanged (existing);

            // Assert
            Assert.Equal (definition.ServiceName, existingDefinition.ServiceName);
            Assert.False (result);
        }

        [Fact, IsUnit]
        public void IsUnchanged_ForTheSameConfig_ReturnsTrue ()
        {
            // Arrange
            const string eventName = "my.event.type";
            const string subscriberName = "my-subscriber";

            var config = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            var definition = new DesiredReaderDefinition (config);

            var existingConfig = new SubscriberConfigurationBuilder ()
                .WithType (eventName)
                .WithSubscriberName (subscriberName)
                .Create ();

            var existingDefinition = new DesiredReaderDefinition (existingConfig);
            var existing = new ExistingReaderDefinition (existingDefinition.ServiceNameWithSuffix);

            // Act
            var result = definition.IsUnchanged (existing);

            // Assert
            Assert.True (result);
            Assert.Equal (definition.ServiceName, existingDefinition.ServiceName);
            Assert.Equal (definition.ServiceNameWithSuffix, existingDefinition.ServiceNameWithSuffix);
        }
    }
}
