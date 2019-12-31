using System.Collections.Generic;

namespace PowerLinesDataService.Common
{
    public interface IReader
    {
        IList<object> ReadToList(string filepath);
    }
}
