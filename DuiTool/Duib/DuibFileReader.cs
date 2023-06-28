using System.Xml;
using DuiTool.Extensions;
using DuiTool.Helpers;

namespace DuiTool.Duib
{
    class DuibFileReader
    {
        List<DUIBEntry> Entries = new List<DUIBEntry>();
        List<string> Strings = new List<string>();
        BinaryReader Reader;

        public DuibFileReader(string path)
        {
            Reader = new BinaryReader(File.OpenRead(path));
            Read();
        }

        public void Save(string path)
        {
            using FileStream file = File.Create(path);
            using XmlWriter writer = XmlWriter.Create(file, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true, IndentChars = "\t" });

            void WriteProperty(DUIBProperty property)
            {
                string name = (property.NameIndex & 0x8000) != 0 ? Constants.BDXCommonStringTable[property.NameIndex & 0x7FFF] : Strings[property.NameIndex];
                string value = (property.ValueIndex & 0x8000) != 0 ? Constants.BDXCommonStringTable[property.ValueIndex & 0x7FFF] : Strings[property.ValueIndex];

                writer.WriteAttributeString(name, value);
            }

            foreach (var entry in Entries)
            {
                if (entry.Type == DUIBEntryType.StartElement)
                {
                    string name = (entry.NameIndex & 0x8000) != 0 ? Constants.BDXCommonStringTable[entry.NameIndex & 0x7FFF] : Strings[entry.NameIndex];
                    writer.WriteStartElement(name);

                    foreach (var property in entry.Properties)
                    {
                        WriteProperty(property);
                    }
                }
                else if (entry.Type == DUIBEntryType.StartElementEmpty)
                {
                    string name = (entry.NameIndex & 0x8000) != 0 ? Constants.BDXCommonStringTable[entry.NameIndex & 0x7FFF] : Strings[entry.NameIndex];
                    writer.WriteStartElement(name);

                    foreach (var property in entry.Properties)
                    {
                        WriteProperty(property);
                    }

                    writer.WriteEndElement();
                }
                else if (entry.Type == DUIBEntryType.EndElement)
                {
                    writer.WriteEndElement();
                }
            }
        }

        void Read()
        {
            // Reading header
            if (Reader.ReadBytes(4).SequenceEqual(new byte[] { (byte)'d', (byte)'u', (byte)'i', (byte)'b' }) == false)
            {
                ConsoleEx.WriteError("Invalid duib file");
                Environment.Exit(Constants.INVALID_DUIB_FILE);

                return;
            }

            uint version = Reader.ReadUInt32();
            if (version != 5)
            {
                ConsoleEx.WriteError("Only v5 duib files are supported");
                Environment.Exit(Constants.UNSUPPORTED_DUIB_VERSION);

                return;
            }

            uint entryChunkOffset = Reader.ReadUInt32();
            uint stringChunkOffset = Reader.ReadUInt32();
            uint resourceChunkOffset = Reader.ReadUInt32(); // We ignore the resource chunk since it doesn't appear in duixml files

            // Reading entry chunk
            Reader.BaseStream.Seek(entryChunkOffset, SeekOrigin.Begin);
            ReadEntryChunk();

            // Reading string chunk
            if (stringChunkOffset > 0)
            {
                Reader.BaseStream.Seek(stringChunkOffset, SeekOrigin.Begin);
                ReadStringChunk();
            }
        }

        void ReadEntryChunk()
        {
            // Reading chunk header
            uint chunkSize = Reader.ReadUInt32();
            uint entriesCount = Reader.ReadUInt32();

            // Reading entries
            for (int i = 0; i < entriesCount; i++)
            {
                DUIBEntry entry = new DUIBEntry();

                ushort typeAndPropCount = Reader.ReadUInt16();

                entry.Type = (DUIBEntryType)(typeAndPropCount & 0x0F);
                entry.NameIndex = Reader.ReadUInt16();

                ushort propertiesCount = (ushort)(typeAndPropCount >> 4);
                entry.Properties = new DUIBProperty[propertiesCount];

                for (int j = 0; j < propertiesCount; j++)
                {
                    entry.Properties[j].NameIndex = Reader.ReadUInt16();
                    entry.Properties[j].ValueIndex = Reader.ReadUInt16();
                }

                Entries.Add(entry);
            }
        }

        void ReadStringChunk()
        {
            // Reading chunk header
            uint chunkSize = Reader.ReadUInt32();
            uint stringsCount = Reader.ReadUInt32();

            // Reading strings
            List<uint> stringOffsets = new List<uint>(); // UNUSED FOR NOW, TODO: Skip this?

            for (int i = 0; i < stringsCount; i++)
                stringOffsets.Add(Reader.ReadUInt32());

            for (int i = 0; i < stringsCount; i++)
                Strings.Add(Reader.ReadNullTerminatedUnicodeString());
        }

        ~DuibFileReader()
        {
            // Some unnecessary cleanup because why not
            Reader.Close();
            Entries.Clear();
            Strings.Clear();

            Reader = null;
            Entries = null;
            Strings = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
