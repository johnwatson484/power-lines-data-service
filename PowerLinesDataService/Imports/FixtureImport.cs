using System;
using System.Net;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports
{
    public class FixtureImport : Import
    {
        public FixtureImport(File file, string source) : base (file, source)
        {
        }

        public override void Load(string[] args)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(string.Format(source), file.Filepath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading fixtures: {0}", ex);
            }
            finally
            {
                file.DeleteFileIfExists();
            }
        }
    }
}
