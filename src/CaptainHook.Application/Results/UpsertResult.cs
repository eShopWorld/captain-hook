namespace CaptainHook.Application.Results
{
    public class UpsertResult<T>
    {
        public T Dto { get; }
        public UpsertType UpsertType { get; }

        public UpsertResult(T dto, UpsertType upsertType)
        {
            Dto = dto;
            UpsertType = upsertType;
        }
    }

    public enum UpsertType
    {
        Created = 0,
        Updated
    }
}