using System.Security.Cryptography;
using System.Text;


namespace FileIndexer
{
    public static class HashWorker
    {
        public static string CalculateMD5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int i = 0; i < hashBytes.Length; i++)
                        stringBuilder.Append(hashBytes[i].ToString("x2")); // Convert each byte to its hexadecimal representation
                    

                    return stringBuilder.ToString();
                }
            }
        }

        public static HashFile IndexFile(string filePath)
        {
            
            // Get file info
            var fileInfo = new FileInfo(filePath);
            
            if(!fileInfo.Exists)
                throw new FileNotFoundException("File not found", filePath);

            string hash = CalculateMD5Hash(filePath);

            // Create Metadata for file
            var metadata = new Dictionary<string, string>
            {
                { "Filename", fileInfo.Name },
                { "Filepath", fileInfo.DirectoryName },
                { "Filetype", fileInfo.Extension },
                { "Filesize", fileInfo.Length.ToString() },
                { "DateIndexed", DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") },
                { "DateCreated", fileInfo.CreationTime.ToString("dd.MM.yyyy-HH:mm:ss")},
                { "DateModified", fileInfo.LastWriteTime.ToString("dd.MM.yyyy-HH:mm:ss")},
                
                { "Hash", hash},

            };
            return new HashFile(filePath, hash, metadata);
        }

        public static Dictionary<string, HashFile> IndexDirectory(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath);
            var directories = Directory.GetDirectories(directoryPath);
            var indexedFiles = new Dictionary<string, HashFile>();

            foreach (var file in files)
            {
                var indexedFile = IndexFile(file);
                // Ignore duplicate Hashes
                if(!indexedFiles.ContainsKey(indexedFile.hash))
                    indexedFiles.Add(indexedFile.hash, indexedFile);
            }

            // Recurse through directories
            foreach (var directory in directories)
                IndexDirectory(directory);
          
            return indexedFiles;
        }

        public static async Task<HashFile> IndexFileAsync(string filePath)
        {

            // Get file info
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found", filePath);

            // Read and hash the file asynchronously
            string hash;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var md5 = MD5.Create())
                {
                    var hashBytes = await md5.ComputeHashAsync(stream);
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

            // Create Metadata for file
            var metadata = new Dictionary<string, string>
            {
                { "Filename", fileInfo.Name },
                { "Filepath", fileInfo.DirectoryName },
                { "Filetype", fileInfo.Extension },
                { "Filesize", fileInfo.Length.ToString() },
                { "DateIndexed", DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") },
                { "DateCreated", fileInfo.CreationTime.ToString("dd.MM.yyyy-HH:mm:ss")},
                { "DateModified", fileInfo.LastWriteTime.ToString("dd.MM.yyyy-HH:mm:ss")},

                { "Hash", hash},

            };

            return new HashFile(filePath, hash, metadata);
        }


        public static async Task<Dictionary<string, HashFile>> IndexDirectoryAsync(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath);
            var directories = Directory.GetDirectories(directoryPath);
            var indexedFiles = new Dictionary<string, HashFile>();

            // Create a list of tasks to index each file
            var fileTasks = new List<Task<HashFile>>();
            foreach (var file in files)
            {
                // Run the IndexFile method asynchronously and add the task to the list
                var fileTask = IndexFileAsync(file);
                fileTasks.Add(fileTask);
            }

            // Wait for all the file tasks to complete and add the results to the dictionary
            var fileResults = await Task.WhenAll(fileTasks);
            foreach (var indexedFile in fileResults)
            {
                // Ignore duplicate hashes
                if (!indexedFiles.ContainsKey(indexedFile.hash))
                    indexedFiles.Add(indexedFile.hash, indexedFile);
            }

            // Create a list of tasks to index each subdirectory
            var directoryTasks = new List<Task<Dictionary<string, HashFile>>>();
            foreach (var directory in directories)
            {
                // Run the IndexDirectoryAsync method recursively and add the task to the list
                var directoryTask = IndexDirectoryAsync(directory);
                directoryTasks.Add(directoryTask);
            }

            // Wait for all the directory tasks to complete and merge the results with the dictionary
            var directoryResults = await Task.WhenAll(directoryTasks);
            foreach (var directoryResult in directoryResults)
            {
                // Merge the dictionaries, ignoring duplicate hashes
                foreach (var kvp in directoryResult)
                {
                    if (!indexedFiles.ContainsKey(kvp.Key))
                        indexedFiles.Add(kvp.Key, kvp.Value);
                }
            }

            return indexedFiles;
        }

    }
}
