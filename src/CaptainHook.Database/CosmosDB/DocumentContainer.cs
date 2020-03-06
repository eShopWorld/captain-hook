﻿namespace CaptainHook.Database.CosmosDB
{
    public struct DocumentContainer<TDoc>
    {
        public readonly string ETag;
        public readonly TDoc Document;

        public DocumentContainer (TDoc document, string etag)
        {
            Document = document;
            ETag = etag;
        }
    }
}