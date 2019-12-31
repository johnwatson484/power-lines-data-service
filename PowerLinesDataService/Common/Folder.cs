using System;
using System.IO;

namespace PowerLinesDataService.Common
{
    public class Folder : IFolder
    {
        private string folderPath;

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
                    Console.WriteLine("That path exists already.");
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(folderPath);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(folderPath));
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
    }
}
