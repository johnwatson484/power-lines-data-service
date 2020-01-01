using System;
using System.Net;
using System.Collections.Generic;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports
{
    public class ResultImport : Import
    {
        public ResultImport(IFile file, string source) : base(file, source)
        {
        }

        public override void Load(string[] args)
        {
            bool currentSeasonOnly = true;

            int firstSeason = GetFirstSeasonYear(currentSeasonOnly);
            int lastSeason = GetLastSeasonYear();

            while (firstSeason < lastSeason)
            {
                foreach (var league in leagues)
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(string.Format(source, GetSeason(firstSeason % 100), league), file.Filepath);
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

                firstSeason++;
            }
        }

        private string GetSeason(int firstYear)
        {
            int secondYear = firstYear + 1;

            return string.Format("{0}{1}", firstYear.ToString("D2"), secondYear.ToString("D2"));
        }

        private int GetFirstSeasonYear(bool currentSeasonOnly = false)
        {
            if (!currentSeasonOnly)
            {
                return 1993;
            }

            DateTime current = DateTime.Now;

            if (current.Month <= 5)
            {
                return current.Year - 1;
            }
            return current.Year;
        }

        private int GetLastSeasonYear()
        {
            DateTime current = DateTime.Now;

            if (current.Month <= 5)
            {
                return current.Year;
            }
            return current.Year + 1;
        }

        private List<string> leagues = new List<string>
        {
            "E0",
            "E1",
            "E2",
            "E3",
            "EC",
            "SC0",
            "SC1",
            "SC2",
            "SC3",
            "D1",
            "D2",
            "I1",
            "I2",
            "SP1",
            "SP2",
            "F1",
            "F2",
            "N1",
            "B1",
            "P1",
            "T1",
            "G1"
        };
    }
}
