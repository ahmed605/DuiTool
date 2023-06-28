namespace DuiTool.Duib
{
    enum DUIBEntryType
    {
        StartElement = 0x0, // Entry has child elements/entries/xml nodes
        StartElementEmpty = 0x1, // Entry doesn't have child elements/entries/xml nodes, entries of this type do not need to be closed with an entry of type EndElement
        EndElement = 0x2 // Ends a StartElement entry
    }

    struct DUIBProperty
    {
        public ushort NameIndex;
        public ushort ValueIndex;
    }

    struct DUIBResource
    {
        public uint IdIndex;
        public uint AtEntry;
    }

    struct DUIBEntry
    {
        public DUIBEntryType Type;
        public ushort NameIndex;
        public DUIBProperty[] Properties;
    }
}
