using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FileIndexer
{
    // Internal class for handling indexed files
    [Serializable]
    public class HashFile
    {
        public string filePath { get; set; }

        public string fileName { get; set; }
        public string hash { get; set; }
        public Dictionary<string, string> ?Metadata { get; set; }

        public HashFile(string filePath, string hash, Dictionary<string, string> Metadata = null, string fileName = null)
        {
            this.filePath = filePath;
            this.hash = hash;
            this.Metadata = Metadata;
        }


        // Override ToString method and Equals method to enable comparisons
        public override string ToString()
        {
            return $"File: {fileName}, Path: {filePath}, Hash: {hash}";
        }

        public override bool Equals(object obj)
        {
            if (obj is HashFile)
            {
                var file = obj as HashFile;
                return file.filePath == this.filePath && file.hash == this.hash;
            }
            return false;
        }
    }
}
