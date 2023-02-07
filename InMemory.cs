using System.Collections.Concurrent;
using System.Text.Json;

namespace InMemorylib
{
    public class InMemory
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> db;
        private string defaultDb;

        public InMemory(string defultDbName = "init")
        {
            db = new();
            defaultDb = defultDbName;
            db.TryAdd(defultDbName, new());
        }

        /// <summary>
        /// Geting a db from memory
        /// </summary>
        /// <param name="dbName">geting a db with name if db name is null return defult db with name init</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, string>? GetDB(string dbName = null)
        {
            if (dbName == null)
            {
                return db[defaultDb];
            }
            db.TryGetValue(dbName, out var result);
            return result;
        }

        /// <summary>
        /// create a empty database in memory
        /// </summary>
        /// <param name="dbName">your db name example foo </param>
        /// <returns>return true if database created and return false if database exist</returns>
        public bool CreateEmptyDatabase(string dbName)
            => db.TryAdd(dbName, new());
       

        /// <summary>
        /// remoing a existing database from memory
        /// </summary>
        /// <param name="dbName">true if daabase where sucssesfuly deleted</param>
        /// <returns></returns>
        public bool RemoveDatabase(string dbName)
        {
            if (dbName == defaultDb)
            {
                throw new Exception("The default database is not allowed to be deleted");
            }
            return db.TryRemove(dbName, out _);
        }

        public bool SetValue(string key, string value = "", string dbName = "init")
        {
            var collection = GetDB(dbName);

            if (collection is null)
            {
                throw new Exception($"db with name {dbName} not found");
            }
            collection.AddOrUpdate(key, value, (e, x) => value);
            db.AddOrUpdate(dbName , collection, (e, x) => collection);
            return true;

        }
        /// <summary>
        /// Adding records to the database without the need for a key and as an automatic increase
        /// </summary>
        /// <param name="value">the value for you data</param>
        /// <param name="dbName">your database name</param>
        /// <returns>if true the seting value has ok</returns>
        /// <exception cref="Exception"></exception>
        public bool SetValueAsAutoIncrementKey(string value = "", string dbName = "init" , int incraseFrom = 1)
        {
            var collection = GetDB(dbName);

            if (collection is null)
            {
                throw new Exception($"db with name {dbName} not found");
            }
            else if(collection.Count == 0)
            {
                collection.AddOrUpdate($"{incraseFrom}", value, (e, x) => value);
                db.AddOrUpdate(dbName, collection, (e, x) => collection);
                return true;
            }
            else
            {
                var lastRecordId = int.Parse(collection.MaxBy(f=>f.Key).Key);
                lastRecordId += 1;
                collection.AddOrUpdate(lastRecordId.ToString(), value, (e, x) => value);
                db.AddOrUpdate(dbName, collection, (e, x) => collection);
                return true;

            }
            
        }


        /// <summary>
        /// Adding records to the database without the need for a key and as an Guid Text 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dbName"></param>
        /// <param name="guidKey">If true, the record key will be a guid automatically</param>
        /// <returns>return true if record created</returns>
        /// <exception cref="Exception"></exception>
        public bool SetValueAsGuidKey (string value = "", string dbName = "init")
        {
            var collection = GetDB(dbName);

            if (collection is null)
            {
                throw new Exception($"db with name {dbName} not found");
            }

            var recordGuid = Guid.NewGuid().ToString("N");

            collection.AddOrUpdate(recordGuid, value, (e, x) => value);
            db.AddOrUpdate(dbName, collection, (e, x) => collection);
            return true;
        }

        public bool SetIntValue(string key, int value, string dbName = "init")
            => SetValue(key, value.ToString(), dbName);


        public string? GetValue(string key , string dbName = "init")
        {
            db.TryGetValue(dbName, out var collection);
            if (collection is null)
            {
                throw new Exception($"db with name {dbName} not found");
            }
            collection.TryGetValue(key, out var result);
            return result;

        }

        public bool DBExist(string dbName = "init")
            => db.Any(f => f.Key == dbName);


        /// <summary>
        /// create a database if not exist
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>return true if db sucsses created</returns>
        public bool CreateDBIfNotExist(string dbName = "init")
        {
            if(!DBExist(dbName))
            {
                CreateEmptyDatabase(dbName);
                return true;
            }
            return false;
        }

        public void SaveMemoryChanges(string backupName = "appMemory")
        {
            var jsonStr = JsonSerializer.Serialize(db);
            File.WriteAllText(backupName , jsonStr);
        }
        public void LoadMemoryChanges(string backupName = "appMemory")
        {
            if(!File.Exists(backupName))
            {
                return;
            }
            else
            {
                var jsonStr = File.ReadAllText(backupName);
                try
                {
                    db = JsonSerializer.Deserialize<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(jsonStr);
                }
                catch
                {
                    throw new Exception("The memory file Structure is invalid");
                    
                }
            }
            

           
        }

        public void IncreaseNumber(string key , string dbName = "init")
        {
            var dbCol = GetDB(dbName);
            if(dbCol is not null && dbCol.Any(f=>f.Key == key))
            {
                var value = dbCol[key];

                if (value == "")
                {
                    value = "0";
                }
                if (int.TryParse(value, out var val))
                {
                    val += 1;
                    SetValue(key, val.ToString(), dbName);

                }
                else
                {
                    throw new Exception("the value can not parse to int datatype");
                }
            }
            else if(dbCol is null)
            {
                throw new Exception($"database with name {dbName} not found");
            }
            else
            {
                SetIntValue(key, 0, dbName);
            }
            
           

        }
        public void DecreaseNumber(string key, string dbName = "init")
        {
            var value = GetDB(dbName)[key];
            if (int.TryParse(value, out var val))
            {
                val -= 1;
                SetValue(key, val.ToString(), dbName);

            }
            else
            {
                throw new Exception("the value can not parse to int datatype");
            }
        }

        public bool KeyExist(string key , string dbName = "init")
        {
            var collection = GetDB(dbName);
            if(collection is null)
            {
                throw new Exception($"db with name {dbName} not found");
            }

            return collection.Any(f => f.Key == key);
           
        }

       



    }
}
