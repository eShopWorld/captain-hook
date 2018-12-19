namespace CaptainHook.Common.Nasty
{
    using System;

    /// <summary>
    /// Really temp dto
    /// </summary>
    public class HttpResponseDto
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }

        public Guid OrderCode { get; set; }
    }
}
