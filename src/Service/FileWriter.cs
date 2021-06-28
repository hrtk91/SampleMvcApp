using System.IO;

namespace SampleMvcApp.Services
{
    public class FileWriter : IFileWriter
    {
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        public FileStream Open(string filePath, FileMode fileMode = FileMode.Open)
        {
            return File.Open(filePath, fileMode);
        }

        public void Dispose() { }
    }
}