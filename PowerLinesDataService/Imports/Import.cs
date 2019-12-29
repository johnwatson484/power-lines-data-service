using System;

namespace PowerLinesDataService.Imports
{
    public abstract class Import
    {
        public abstract void Load(string[] args);
    }
}
