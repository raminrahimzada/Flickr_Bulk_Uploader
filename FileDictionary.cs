using System;
using System.Collections.Generic;
using System.IO;

namespace Flickr_Bulk_Uploader
{
    public class FileDictionary<TKey,TValue>
    {
        private readonly string _filename;
        private Dictionary<TKey, TValue> _db = new Dictionary<TKey, TValue>();
        public FileDictionary(string filename)
        {
            _filename = filename;
            Load();
        }
        public void Set(TKey key, TValue value)
        {
            if (_db.ContainsKey(key))
            {
                _db[key] = value;
            }
            else
            {
                _db.Add(key, value);
            }
            Save();
        }
        public TValue Get(TKey key)
        {
            return _db.ContainsKey(key) ? _db[key] : default(TValue);
        }
        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            if (_db.ContainsKey(key)) _db.Remove(key);
            Save();
        }
        /// <summary>
        /// Try Loading Data From File
        /// </summary>
        private void Load()
        {
            if (!File.Exists(_filename))
            {
                _db = new Dictionary<TKey, TValue>();
            }
            else
            try
            {
                _db = Extensions.Deserialize<Dictionary<TKey, TValue>>(File.ReadAllBytes(_filename));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception.OnLoad:" + e.Message);
                _db = new Dictionary<TKey, TValue>();
            }
            Save();
        }
        /// <summary>
        /// Saving Changes To DB
        /// </summary>
        private void Save()
        {
            var data = Extensions.Serialize(_db);
            File.WriteAllBytes(_filename, data);
        }
        /// <summary>
        /// Close Only
        /// </summary>
        public void Close()
        {
            _db = null;
        }
        /// <summary>
        /// Close and Delete File
        /// </summary>
        public void Dispose()
        {
            Close();
            if (!File.Exists(_filename)) return;
            File.Delete(_filename);
        }
    }
}
