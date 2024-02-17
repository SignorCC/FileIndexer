using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileIndexer
{
    public class FileWorker
    {
        private  Dictionary<string, HashFile> AllocationTable;

        public FileWorker()
        {
            AllocationTable = new Dictionary<string, HashFile>();
        }

        // Search for a file with the same hash in allocation Table
        public HashFile SearchFile(string hash)
        {
            if(AllocationTable.TryGetValue(hash, out HashFile file))
                return file;

            else
                return null;
        }

        // Add a file to the allocation table if not present
        public bool AddFile(HashFile file)
        {
            if(AllocationTable.ContainsKey(file.hash))
                return false;
            
            else
                AllocationTable.Add(file.hash, file);
                return true;
        }

        // Remove a file from the allocation table if present
        public bool RemoveFile(HashFile file)
        {
            if(AllocationTable.TryGetValue(file.hash, out HashFile fileToRemove))
            {
                AllocationTable.Remove(fileToRemove.hash);
                return true;
            }

            else
                return false;
        }

        // Read table in from disk
        public bool ReadTable(string path)
        {
            // Read in settings from json file

            path = Utility.ResolvePath(path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                AllocationTable = JsonConvert.DeserializeObject<Dictionary<string, HashFile>>(json);
                return true;
            }

            else
                return false;
        }

        // Write table to disk
        public bool WriteTable(string path)
        {
            path = Utility.ResolvePath(path);

            if(AllocationTable.Count <= 0)
                return false;

            else
            {
                string json = JsonConvert.SerializeObject(AllocationTable);
                File.WriteAllText(path, json);
                return true;
            }
        }

        public Dictionary<string, HashFile> GetTable()
        {
            return AllocationTable;
        }
    }
}
