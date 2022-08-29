using System.Collections.Generic;

namespace PowerLinesDataService.Common;

public class File : IFile
{
    public string Filepath { get; }

    protected IReader reader;

    public File(string filepath, IReader reader)
    {
        Filepath = filepath;
        this.reader = reader;
    }

    public void DeleteFileIfExists()
    {
        if (System.IO.File.Exists(Filepath))
        {
            System.IO.File.Delete(Filepath);
        }
    }

    public IList<object> ReadFileToList()
    {
        return reader.ReadToList(Filepath);
    }
}
