using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UntoldUnpack.HPx;

namespace UntoldUnpack
{
    public class Archive
    {
        FileInfo indexFile, binaryFile;
        IndexDataFile indexData;

        public Archive(FileInfo archiveFile)
        {
            if (archiveFile.Extension.ToLowerInvariant() == ".hpi")
            {
                indexFile = archiveFile;
                binaryFile = new FileInfo(Path.ChangeExtension(indexFile.FullName, ".hpb"));
            }
            else if (archiveFile.Extension.ToLowerInvariant() == ".hpb")
            {
                binaryFile = archiveFile;
                indexFile = new FileInfo(Path.ChangeExtension(binaryFile.FullName, ".hpi"));
            }
            else
                throw new Exception("Could not recognize archive file");

            if (!indexFile.Exists) throw new FileNotFoundException("Index file not found", indexFile.Name);
            if (!binaryFile.Exists) throw new FileNotFoundException("Binary file not found", binaryFile.Name);

            using (FileStream indexStream = new FileStream(indexFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                indexData = new IndexDataFile(indexStream);
            }
        }

        public Dictionary<int, string> GetFileDictionary()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            for (int i = 0; i < indexData.FileEntries.Count; i++)
            {
                string path = Encoding.GetEncoding(932).GetString(indexData.FilenameTableData.Skip((int)indexData.FileEntries[i].FilenameOffset).TakeWhile(x => !x.Equals(0)).ToArray());
                dict.Add(i, path);
            }
            return dict;
        }

        public void GetFileData(int index, out string path, out byte[] data)
        {
            IndexFileEntry fileEntry = indexData.FileEntries[index];

            path = Encoding.GetEncoding(932).GetString(indexData.FilenameTableData.Skip((int)fileEntry.FilenameOffset).TakeWhile(x => !x.Equals(0)).ToArray());

            using (FileStream binaryStream = new FileStream(binaryFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fileEntry.FileOffset >= 0 && fileEntry.FileOffset < binaryStream.Length)
                {
                    binaryStream.Seek(fileEntry.FileOffset, SeekOrigin.Begin);

                    if (fileEntry.DecompressedFileSize != 0)
                    {
                        CompressedFile file = new CompressedFile(binaryStream);
                        data = file.DecompressedData;
                    }
                    else
                    {
                        data = new byte[fileEntry.CompressedFileSize];
                        binaryStream.Read(data, 0, data.Length);
                    }
                }
                else
                    data = null;
            }
        }

        public void GetFileData(string filename, out string path, out byte[] data)
        {
            for (int i = 0; i < indexData.FileEntries.Count; i++)
            {
                string checkPath = Encoding.GetEncoding(932).GetString(indexData.FilenameTableData.Skip((int)indexData.FileEntries[i].FilenameOffset).TakeWhile(x => !x.Equals(0)).ToArray());
                if (checkPath.ToLowerInvariant().Contains(filename.ToLowerInvariant()))
                {
                    GetFileData(i, out path, out data);
                    return;
                }
            }

            throw new FileNotFoundException("File not found in archive", filename);
        }
    }
}
