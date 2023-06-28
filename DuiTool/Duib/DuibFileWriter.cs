using System.Xml;
using System.Text;

namespace DuiTool.Duib
{
    class DuibFileWriter
    {
        List<DUIBEntry> Entries = new List<DUIBEntry>();
        List<string> Strings = new List<string>();
        List<DUIBResource> Resources = new List<DUIBResource>();

        public DuibFileWriter(XmlDocument doc)
        {
            ReadEntry(doc.ChildNodes[0]!);
        }

        public void Write(string path)
        {
            using FileStream file = File.Create(path);
            using BinaryWriter writer = new BinaryWriter(file);

            uint oldPos = 0;

            // Write header
            writer.Write(new byte[] { (byte)'d', (byte)'u', (byte)'i', (byte)'b' }); // Signature
            writer.Write((uint)5); // Version
            writer.Write((uint)0x0); // Placeholder for entry chunk offset
            writer.Write((uint)0x0); // Placeholder for string chunk offset
            writer.Write((uint)0x0); // Placeholder for resource chunk offset

            // Write resource chunk
            if (Resources.Count > 0)
            {
                uint startResourceChunkOffset = (uint)file.Position;

                writer.Write((uint)0x0); // Placeholder for resource chunk size
                writer.Write((uint)Resources.Count);

                foreach (var resource in Resources)
                {
                    writer.Write(resource.IdIndex);
                    writer.Write(resource.AtEntry);
                }

                uint resourceChunkSize = (uint)file.Position - startResourceChunkOffset;

                oldPos = (uint)file.Position;
                file.Seek(startResourceChunkOffset, SeekOrigin.Begin);
                writer.Write(resourceChunkSize);

                // Write resource chunk offset
                file.Seek(0x10, SeekOrigin.Begin);
                writer.Write(startResourceChunkOffset);

                file.Seek(oldPos, SeekOrigin.Begin);
            }

            // Write entry chunk
            uint startEntryChunkOffset = (uint)file.Position;

            writer.Write((uint)0x0); // Placeholder for entry chunk size
            writer.Write((uint)Entries.Count);

            foreach (var entry in Entries)
            {
                uint numProps = entry.Properties == null ? 0 : (uint)entry.Properties.Length;

                writer.Write((ushort)(numProps << 4 | (ushort)entry.Type));
                writer.Write(entry.NameIndex);

                if (entry.Properties != null)
                {
                    foreach (var property in entry.Properties)
                    {
                        writer.Write(property.NameIndex);
                        writer.Write(property.ValueIndex);
                    }
                }
            }

            uint entryChunkSize = (uint)file.Position - startEntryChunkOffset;

            oldPos = (uint)file.Position;
            file.Seek(startEntryChunkOffset, SeekOrigin.Begin);
            writer.Write(entryChunkSize);

            // Write string chunk offset
            file.Seek(0x0C, SeekOrigin.Begin);
            writer.Write(oldPos);

            // Write entry chunk offset
            file.Seek(0x08, SeekOrigin.Begin);
            writer.Write(startEntryChunkOffset);

            file.Seek(oldPos, SeekOrigin.Begin);

            // Write string chunk
            uint startStringChunkOffset = (uint)file.Position;

            writer.Write((uint)0x0); // Placeholder for string chunk size
            writer.Write((uint)Strings.Count);

            uint stringListItemOffset = (uint)(Strings.Count * 4) + 8;

            foreach (var str in Strings)
            {
                writer.Write(stringListItemOffset);
                stringListItemOffset += (uint)((str.Length + 1) * 2);
            }

            foreach (var str in Strings)
            {
                writer.Write(Encoding.Unicode.GetBytes(str));
                writer.Write((ushort)0x0);
            }

            if (Resources.Count == 0) writer.Write((ushort)0x0); // End strings list

            uint stringChunkSize = (uint)file.Position - startStringChunkOffset;

            oldPos = (uint)file.Position;
            file.Seek(startStringChunkOffset, SeekOrigin.Begin);
            writer.Write(stringChunkSize);
            file.Seek(oldPos, SeekOrigin.Begin);
        }

        void ReadEntry(XmlNode entry)
        {
            DUIBEntry duibEntry = new DUIBEntry();

            if (entry.NodeType == XmlNodeType.Element)
            {
                duibEntry.Type = entry.ChildNodes.Count == 0 ? DUIBEntryType.StartElementEmpty : DUIBEntryType.StartElement;

                if (Constants.BDXCommonStringTable.Contains(entry.Name))
                    duibEntry.NameIndex = (ushort)(Array.IndexOf(Constants.BDXCommonStringTable, entry.Name) | 0x8000);
                else
                {
                    if (!Strings.Contains(entry.Name)) Strings.Add(entry.Name);
                    duibEntry.NameIndex = (ushort)Strings.IndexOf(entry.Name);
                }

                if (entry.Attributes?.Count > 0)
                {
                    duibEntry.Properties = new DUIBProperty[entry.Attributes.Count];

                    for (int i = 0; i < entry.Attributes.Count; i++)
                    {
                        var attr = entry.Attributes[i];
                        var prop = duibEntry.Properties[i] = ReadProperty(attr);

                        if (attr.Name == "resid" && entry.Name != "style")
                        {
                            DUIBResource res = new DUIBResource();
                            res.IdIndex = prop.ValueIndex;
                            res.AtEntry = (uint)Entries.Count;
                            Resources.Add(res);
                        }
                    }
                }
            }

            Entries.Add(duibEntry);

            if (entry.ChildNodes.Count > 0)
            {
                foreach (XmlNode node in entry.ChildNodes)
                {
                    ReadEntry(node);
                }

                DUIBEntry endElement = new DUIBEntry();
                endElement.Type = DUIBEntryType.EndElement;

                if (Constants.BDXCommonStringTable.Contains(entry.Name))
                    endElement.NameIndex = (ushort)(Array.IndexOf(Constants.BDXCommonStringTable, entry.Name) | 0x8000);
                else
                {
                    if (!Strings.Contains(entry.Name)) Strings.Add(entry.Name);
                    endElement.NameIndex = (ushort)Strings.IndexOf(entry.Name);
                }

                Entries.Add(endElement);
            }
        }

        DUIBProperty ReadProperty(XmlAttribute property)
        {
            DUIBProperty prop = new DUIBProperty();

            if (Constants.BDXCommonStringTable.Contains(property.Name))
                prop.NameIndex = (ushort)(Array.IndexOf(Constants.BDXCommonStringTable, property.Name) | 0x8000);
            else
            {
                if (!Strings.Contains(property.Name)) Strings.Add(property.Name);
                prop.NameIndex = (ushort)Strings.IndexOf(property.Name);
            }

            if (Constants.BDXCommonStringTable.Contains(property.Value))
                prop.ValueIndex = (ushort)(Array.IndexOf(Constants.BDXCommonStringTable, property.Value) | 0x8000);
            else
            {
                if (!Strings.Contains(property.Value)) Strings.Add(property.Value);
                prop.ValueIndex = (ushort)Strings.IndexOf(property.Value);
            }

            return prop;
        }
    }
}
