using System.Xml;
using System.Linq;
using DuiTool.Duib;
using DuiTool.Helpers;

namespace DuibTool
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ConsoleEx.WriteMessage("Welcome to DuiTool!");

            if (args.Length < 2)
            {
                ConsoleEx.WriteMessage("Usage: DuiTool.exe compile <path to duixml file>");
                ConsoleEx.WriteMessage("Usage: DuiTool.exe decompile <path to duib file>");
                return;
            }
            else if (args[0].ToLower() is not ("compile" or "decompile"))
            {
                ConsoleEx.WriteMessage("Usage: DuiTool.exe compile <path to duixml file>");
                ConsoleEx.WriteMessage("Usage: DuiTool.exe decompile <path to duib file>");
                return;
            }
            else if (File.Exists(args[1]) == false)
            {
                ConsoleEx.WriteError("File not found: " + args[1]);
                return;
            }

            if (args[0].ToLower() == "compile")
            {
                string path = args[1];

                ConsoleEx.WriteInfo("Parsing file...");

                string duixml = File.ReadAllText(path);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(duixml);

                if (doc.ChildNodes.Count != 1 || doc.ChildNodes[0]?.Name != "duixml")
                {
                    ConsoleEx.WriteError("Invalid duixml file");
                    return;
                }

                ConsoleEx.WriteInfo("Compiling file...");

                DuibFileWriter writer = new DuibFileWriter(doc);

                if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".xml", ".duib", StringComparison.OrdinalIgnoreCase);
                else if (path.EndsWith(".duixml", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".duixml", ".duib", StringComparison.OrdinalIgnoreCase);
                else if (path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".txt", ".duib", StringComparison.OrdinalIgnoreCase);
                else
                    path += ".duib";

                writer.Write(path);

                ConsoleEx.WriteSuccess($"File saved successfully at {path}");
            }
            else
            {
                string path = args[1];

                ConsoleEx.WriteInfo("Decompiling file...");

                DuibFileReader reader = new DuibFileReader(path);
                
                if (path.EndsWith(".duib", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".duib", ".duixml", StringComparison.OrdinalIgnoreCase);
                else if (path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".bin", ".duixml", StringComparison.OrdinalIgnoreCase);
                else if (path.EndsWith(".uib", StringComparison.OrdinalIgnoreCase))
                    path = path.Replace(".uib", ".duixml", StringComparison.OrdinalIgnoreCase);
                else
                    path += ".duixml";

                ConsoleEx.WriteInfo("Saving file...");

                reader.Save(path);

                ConsoleEx.WriteSuccess($"File saved successfully at {path}");
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;

            if (exception is XmlException xmlException)
                ConsoleEx.WriteError($"Error while parsing duixml: {xmlException}");
            else
                ConsoleEx.WriteError($"Unknown error happened: {exception}");

            Environment.Exit(exception.HResult);
        }
    }
}
