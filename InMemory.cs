using System.Collections.Concurrent;
using System.Text.Json;

namespace Melissa.Api.Libs.InMemorylib
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

        public bool SetValue( string key, string value = "", string dbName = "init")
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
            File.WriteAllText(jsonStr, backupName);
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
                    throw new Exception("The memory file Structure is invalid")
                    
                }
            }
            

           
        }

        public void IncreaseNumber(string key , string dbName = "init")
        {
            var value = GetDB(dbName)[key];
            if(int.TryParse(value ,  out var val))
            {
                val += 1;
                SetValue(key , val.ToString() , dbName);

            }
            else
            {
                throw new Exception("the value can not parse to int datatype");
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



    }
}
