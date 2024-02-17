
namespace FileIndexer;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FileIndexer_Console_0.1");
        Test().Wait();
    }

    static async Task Test()
    {
        // Read in settings from json file
        Utility.LoadSettings("settings.json");
        Utility.SetSetting("RootDirectory", AppDomain.CurrentDomain.BaseDirectory);
        Utility.WriteSettings("settings.json");
        
        // Index root directory
        Dictionary<string, HashFile> files = await HashWorker.IndexDirectoryAsync("C:\\Users\\Christoph\\Desktop");

        FileWorker fileWorker = new FileWorker();   

        foreach (var file in files) 
        {
            fileWorker.AddFile(file.Value);
        }

        fileWorker.WriteTable("allocationTable.json");

        var table = fileWorker.GetTable();

        // Print out the table
        foreach (var item in table)
        {
            Console.WriteLine(item.Value.ToString());
            foreach (var metadata in item.Value.Metadata)
            {
                Console.WriteLine($"{metadata.Key}: {metadata.Value}");
            }
        }
    }
}