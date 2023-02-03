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


    }