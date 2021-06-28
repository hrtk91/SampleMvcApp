using System;
using System.IO;

namespace SampleMvcApp.Services
{
    public interface IFileWriter : IDisposable
    {
        bool Exists(string filePath);

        FileStream Open(string filePath, FileMode fileMode = FileMode.Open);
    }
}
