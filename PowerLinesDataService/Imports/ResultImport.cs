using System;
using System.Net;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports
{
    public class ResultImport : Import
    {
        public ResultImport(IFile file, string source) : base (file, source)
        {
        }

        public override void Load()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(string.Format(source), file.Filepath);                    
                }
                var results = file.ReadFileToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing results: {0}", ex);
            }
            finally
            {
                file.DeleteFileIfExists();
            }
        }
    }
}
