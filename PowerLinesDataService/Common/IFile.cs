using System.Collections.Generic;

namespace PowerLinesDataService.Common
{
    public interface IFile
    {
        string Filepath { get; }

        void DeleteFileIfExists();

        IList<object> ReadFileToList();

    }
}
