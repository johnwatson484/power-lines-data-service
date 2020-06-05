using System;
using System.Collections.Generic;

namespace PowerLinesDataService.Common
{
    public class File : IFile
    {
        public string Filepath { get; }

        protected IReader reader;

        public File(string filepath, IReader reader)
        {
            Filepath = filepath;
            this.reader = reader;
        }

        public void DeleteFileIfExists()
        {
            if (System.IO.File.Exists(Filepath))
            {
                System.IO.File.Delete(Filepath);
            }
        }        

        public IList<object> ReadFileToList()
        {
            Console.WriteLine("Reading: {0}", Filepath);
            return System.IO.File.Exists(Filepath) ? reader.ReadToList(Filepath) : new List<object>();
        }
    }
}
