using System;
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
            throw new NotImplementedException();
        }
    }
}
