using CaptainHook.Tests.Web.FlowTests;

// ReSharper disable once CheckNamespace
namespace Core.Events.Test /*this is synced to tracking event to keep these grouped together at SB topic level*/
{
    /// <summary>
    /// this event represents the flow of
    ///
    /// basic web hook flow with no routing
    /// </summary>
    public class WebHookFlowTestEvent : FlowTestEventBase
    {
    }

    /// <summary>
    /// this event represents the flow of
    ///
    /// basic web hook flow with matched route
    /// </summary>
    public class WebHookFlowRoutedTestEvent : FlowTestEventBase
    {
    }
}
