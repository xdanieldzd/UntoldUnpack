using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace UntoldUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            int indent = 0;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                var version = new Version((assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute).Version);
                var description = (assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() as AssemblyDescriptionAttribute).Description;
                var copyright = (assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault() as AssemblyCopyrightAttribute).Copyright;

                IndentWriteLine(indent, "{0} v{1}.{2} - {3}", assembly.GetName().Name, version.Major, version.Minor, description);
                IndentWriteLine(indent, "{0}", copyright);
                IndentWriteLine(indent, "ACMP reverse-engineering by @FireyFly; greetings to @Ehm2k");
                Console.WriteLine();

                args = CommandLineTools.CreateArgs(Environment.CommandLine);
                EO4String.TransformToAscii = true;

                if (args.Length < 2 || args.Length > 3)
                    throw new Exception("Invalid arguments; expected: <archive> <output dir>");

                FileInfo inputFile = new FileInfo(args[1]);
                DirectoryInfo outputDir = new DirectoryInfo(args[2]);

                IndentWriteLine(indent, "Opening archive '{0}'...", Path.GetFileNameWithoutExtension(inputFile.Name));

                Archive archive = new Archive(inputFile);
                Dictionary<int, string> fileDictionary = archive.GetFileDictionary().OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                IndentWriteLine(indent, "Found {0} file(s).", fileDictionary.Count);

                indent++;

                foreach (var file in fileDictionary)
                {
                    string path;
                    byte[] data;
                    archive.GetFileData(file.Key, out path, out data);

                    if (data != null)
                    {
                        IndentWriteLine(indent, "Dumping '{0}'...", path);

                        FileInfo outputFile = new FileInfo(Path.Combine(outputDir.FullName, path.ToLowerInvariant()));

                        DirectoryInfo dirinfo = new DirectoryInfo(outputFile.DirectoryName);
                        if (!dirinfo.Exists) Directory.CreateDirectory(dirinfo.FullName);

                        using (FileStream fileStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            fileStream.Write(data, 0, data.Length);
                        }

                        indent++;

                        switch (outputFile.Extension)
                        {
                            case ".stex":
                                IndentWriteLine(indent, "Converting STEX to PNG...");
                                STEX stexInstance = new STEX(outputFile.FullName);
                                stexInstance.ToBitmap().Save(Path.ChangeExtension(outputFile.FullName, ".png"));
                                break;

                            case ".mbm":
                                IndentWriteLine(indent, "Exporting MBM to text file...");
                                MBM mbmInstance = new MBM(outputFile.FullName);
                                mbmInstance.Export(Path.ChangeExtension(outputFile.FullName, ".txt"));
                                break;
                        }

                        indent--;
                    }
                    else
                    {
                        IndentWriteLine(indent, "Could not dump '{0}'.", path);
                    }
                }

                indent--;
            }
            catch (Exception e)
            {
                IndentWriteLine(indent, "-- Error: {0}.", e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void IndentWriteLine(int indent, string format, params object[] param)
        {
            Console.WriteLine(format.Insert(0, new string(' ', indent)), param);
        }
    }
}
