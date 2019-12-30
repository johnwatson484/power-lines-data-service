using System;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports
{
    public class ResultImport : Import
    {
        public ResultImport(File file, string source) : base (file, source)
        {
        }

        public override void Load(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
