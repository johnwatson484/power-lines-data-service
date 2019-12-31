using System.Collections.Generic;

namespace PowerLinesDataService.Common
{
    public interface IReader
    {
        dynamic ReadToList(string filepath);
    }
}
