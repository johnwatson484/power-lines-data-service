namespace PowerLinesDataService.Common;

public class Folder(string folderPath) : IFolder
{
    private readonly string folderPath = folderPath;

    public void CreateFolderIfNotExists()
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                return;
            }
            Directory.CreateDirectory(folderPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unable to create directory: {0}", e.ToString());
        }
    }
}
