using System;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    public readonly struct ReaderChangeInfo
    {
        public readonly ReaderChangeTypes ChangeType;
        public readonly DesiredReaderDefinition NewReader;
        public readonly ExistingReaderDefinition OldReader;

        private ReaderChangeInfo (ReaderChangeTypes type, DesiredReaderDefinition newReader, ExistingReaderDefinition oldReader)
        {
            ChangeType = type;
            NewReader = newReader;
            OldReader = oldReader;
        }

        public static ReaderChangeInfo ToBeCreated (DesiredReaderDefinition newReader)
        {
            if (! newReader.IsValid) throw new ArgumentException ("Invalid reader definition");

            return new ReaderChangeInfo (ReaderChangeTypes.ToBeCreated, newReader, default);
        }

        public static ReaderChangeInfo ToBeRemoved (ExistingReaderDefinition oldReader)
        {
            if (! oldReader.IsValid) throw new ArgumentException ("Invalid reader definition");

            return new ReaderChangeInfo (ReaderChangeTypes.ToBeRemoved, default, oldReader);
        }

        public static ReaderChangeInfo ToBeUpdated (DesiredReaderDefinition newReader, ExistingReaderDefinition oldReader)
        {
            if (! newReader.IsValid) throw new ArgumentException ("Invalid new reader definition");
            if (! oldReader.IsValid) throw new ArgumentException ("Invalid old reader definition");

            return new ReaderChangeInfo (ReaderChangeTypes.ToBeUpdated, newReader, oldReader);
        }
    }
}