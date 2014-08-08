using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Custom.Tournament.Helpers
{
    public class CacheHelper
    {
        public CacheHelper(string fileServerPath)
        {
            FileServerPath = fileServerPath;
        }

        public string FileServerPath { get; private set; }

        public string GetCachedData(string directoryPath, string fileName)
        {
            var fullDirectoryPath = GetFullDirectoryPath(directoryPath);
            var fullFileName = Path.Combine(fullDirectoryPath, fileName);

            return File.ReadAllText(fullFileName);
        }

        public void SaveDataToCache(string directoryPath, string fileName, string data)
        {
            var fullDirectoryPath = GetFullDirectoryPath(directoryPath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            var fullFileName = Path.Combine(fullDirectoryPath, fileName);
            File.WriteAllText(fullFileName, data);
        }

        public void ClearCache(string directoryPath)
        {
            var fullDirectoryPath = GetFullDirectoryPath(directoryPath);
            if (Directory.Exists(fullDirectoryPath))
            {
                var directory = new DirectoryInfo(fullDirectoryPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
            }
        }

        public bool ExistInCache(string directoryPath, string fileName)
        {
            var fullDirectoryPath = GetFullDirectoryPath(directoryPath);
            var fullFileName = Path.Combine(fullDirectoryPath, fileName);

            return File.Exists(fullFileName);
        }

        private string GetFullDirectoryPath(string directoryPath)
        {
            return Path.Combine(FileServerPath, directoryPath);
        }
    }
}