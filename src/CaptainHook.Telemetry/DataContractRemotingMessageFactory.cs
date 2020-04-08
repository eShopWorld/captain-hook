using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace CaptainHook.Telemetry
{
    internal class DataContractRemotingMessageFactory : IServiceRemotingMessageBodyFactory
    {
        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object wrappedRequestObject)
        {
            return new MyServiceRemotingRequestMessageBody(numberOfParameters);
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName, object wrappedResponseObject)
        {
            return new MyServiceRemotingResponseMessageBody();
        }
    }
}