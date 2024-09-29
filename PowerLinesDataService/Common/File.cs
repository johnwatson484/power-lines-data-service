namespace PowerLinesDataService.Common;

public class File(string filepath, IReader reader) : IFile
{
    public string Filepath { get; } = filepath;

    protected IReader reader = reader;

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
