using System;
using System.IO;

namespace PowerLinesDataService.Common
{
    public class Folder : IFolder
    {
        private readonly string folderPath;

        public Folder(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public void CreateFolderIfNotExists()
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    return;
                }
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to create directory: {0}", e.ToString());
            }
        }
    }
}
