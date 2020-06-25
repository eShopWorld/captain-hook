using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using CaptainHook.Common.Configuration.KeyVault;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Common.Configuration.KeyVault
{
    public class KeyVaultSecretManagerTests
    {
        private readonly KeyVaultSecretProvider _secretProvider;

        private readonly Mock<SecretClient> _secretClientMock;

        private readonly Mock<IBigBrother> _bigBrotherMock;

        public KeyVaultSecretManagerTests()
        {
            _bigBrotherMock = new Mock<IBigBrother>();
            _secretClientMock = new Mock<SecretClient>();
            _secretClientMock.SetupGet(m => m.VaultUri).Returns(new Uri("https://abc.vault.test.com"));

            _secretProvider = new KeyVaultSecretProvider(_bigBrotherMock.Object, _secretClientMock.Object);
        }

        [Fact, IsUnit]
        public void GetSecretValueAsync_NoSecretName_ExceptionThrown()
        {
            // Act
            Func<Task<string>> secretFunction = async () => await _secretProvider.GetSecretValueAsync(null);

            // Assert
            secretFunction.Should().Throw<ArgumentException>();
        }

        [Fact, IsUnit]
        public async Task GetSecretValueAsync_SecretFromKeyVault_CorrectSecretRetrieved()
        {
            // Arrange
            const string secretName = "my-secret";
            const string secretValue = "my-value";
            _secretClientMock
                .Setup(m => m.GetSecretAsync(secretName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(secretName, secretValue), null));

            // Act
            var result = await _secretProvider.GetSecretValueAsync(secretName);

            // Assert
            result.Should().Be(secretValue);
        }

        [Fact, IsUnit]
        public void GetSecretValueAsync_SecretClientThrowsException_ExceptionIsLoggedAndRethrown()
        {
            // Arrange
            const string secretName = "my-secret";
            _secretClientMock
                .Setup(m => m.GetSecretAsync(secretName, null, CancellationToken.None))
                .Throws(new RequestFailedException("dummy"));

            // Act
            Func<Task<string>> secretFunction = async () => await _secretProvider.GetSecretValueAsync(secretName);

            // Assert
            secretFunction.Should().Throw<RequestFailedException>();
            _bigBrotherMock.Verify(m => m.Publish(
                It.IsAny<KeyVaultSecretException>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()));
        }
    }
}