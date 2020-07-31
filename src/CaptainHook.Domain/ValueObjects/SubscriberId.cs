namespace CaptainHook.Domain.ValueObjects
{
    public class SubscriberId
    {
        private readonly string _id;

        public SubscriberId(string eventName, string subscriberName)
        {
            _id = $"{eventName}-{subscriberName}";
        }

        public override bool Equals(object obj)
        {
            var otherId = obj as SubscriberId;
            return _id == otherId?._id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id;
        }
    }
}
