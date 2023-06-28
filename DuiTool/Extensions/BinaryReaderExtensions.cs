using System.Text;

namespace DuiTool.Extensions
{
    static class BinaryReaderExtensions
    {
        public static string ReadNullTerminatedUnicodeString(this BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();

            while (true)
            {
                byte b1 = reader.ReadByte();
                byte b2 = reader.ReadByte();

                if (b1 == 0 && b2 == 0)
                    break;

                bytes.Add(b1);
                bytes.Add(b2);
            }

            return Encoding.Unicode.GetString(bytes.ToArray());
        }
    }
}
