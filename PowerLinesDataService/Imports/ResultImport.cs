using System;
using System.Net;
using System.Collections.Generic;
using PowerLinesDataService.Common;
using PowerLinesDataService.Models;
using System.Linq;
using PowerLinesDataService.Messaging;
using System.Threading.Tasks;

namespace PowerLinesDataService.Imports
{
    public class ResultImport : Import
    {
        public ResultImport(string source, IFile file, IConnection connection, MessageConfig messageConfig) : base(source, file, connection, messageConfig)
        {
        }

        public override void Load(string[] args)
        {
            Console.WriteLine("Importing results");

            bool currentSeasonOnly = !args.Contains("-all");

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
                            client.DownloadFile(string.Format(source, GetSeasonYears(firstSeason % 100), league), file.Filepath);
                        }
                        var results = file.ReadFileToList();
                        if (results.Count > 0)
                        {
                            Task.Run(() => connection.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.ResultUsername, messageConfig.ResultPassword).ToString(), messageConfig.ResultQueue)).Wait();
                            foreach (var result in results)
                            {
                                connection.SendMessage(result);
                            }
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

            Console.WriteLine("Import complete");
        }

        private string GetSeasonYears(int firstYear)
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
