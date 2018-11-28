namespace CaptainHook.Common
{
    public class MessageHook
    {
        public int HandlerId { get; set; }

        public string Type { get; set; }

        // Don't store the payload for now
        // it slows down the handling process because it needs to get quorum and the larger the save the longer it takes to get it
    }
}