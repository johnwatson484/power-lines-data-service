using System;
using System.Net;
using System.Collections.Generic;
using PowerLinesDataService.Common;
using PowerLinesDataService.Models;

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

            int firstSeason = GetFirstSeasonYear(DateTime.Now, currentSeasonOnly);
            int lastSeason = GetLastSeasonYear(DateTime.Now);

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
            return string.Format("{0}{1}", firstYear.ToString("D2"), (++firstYear).ToString("D2"));
        }

        private int GetFirstSeasonYear(DateTime currentDate, bool currentSeasonOnly = false)
        {
            if (!currentSeasonOnly)
            {
                return 1993;
            }

            if (currentDate.Month <= 5)
            {
                return currentDate.Year - 1;
            }
            return currentDate.Year;
        }

        private int GetLastSeasonYear(DateTime currentDate)
        {
            if (currentDate.Month <= 5)
            {
                return currentDate.Year;
            }
            return currentDate.Year + 1;
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
