using System;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports
{
    public abstract class Import
    {
        protected string source;

        protected File file;

        public Import(File file, string source)
        {
            this.file = file;
            this.source = source;
        }

        public abstract void Load(string[] args);
    }
}
