using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileIndexer
{   
    // Class for interaction with mongoDB
    public class DatabaseWorker
    {

        private MongoClient client;

        public DatabaseWorker(string URL, string username, string password)
        {
            this.client = new MongoClient($"mongodb://{username}:{password}@{URL}");
        }

        public bool AddFile(string filePath, string hash, string database)
        {
            
            var db = client.GetDatabase(database);
            var collection = db.GetCollection<BsonDocument>("files");

            var document = new BsonDocument
            {
                { "filePath", filePath },
                { "hash", hash }
            };

            try
            {
                collection.InsertOne(document);
                return true;
            }

            catch (Exception e)
            {
                Utility.Log(e.Message);
                return false;
            }
        }

        public async Task<bool> BulkInsertFiles(List<(string, string)> files, string database)
        {

            var db = client.GetDatabase(database);
            var collection = db.GetCollection<BsonDocument>("files");
            var documents = new List<BsonDocument>();


            foreach (var (filePath, hash) in files)
            {
                var document = new BsonDocument
                {
                    { "filePath", filePath },
                    { "hash", hash }
                };

                documents.Add(document);
            }

            var options = new InsertManyOptions
            {
                IsOrdered = false // Insert documents in parallel
            };            

            try
            {
                await collection.InsertManyAsync(documents, options);
                return true;
            }
            catch (Exception e)
            {
                Utility.Log(e.Message);
                return false;
            }
        }



    }
}
