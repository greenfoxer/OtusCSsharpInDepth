using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cache.SimpleStoreSystem
{
    public class SimpleStore
    {
        private readonly Dictionary<string, byte[]> _cache;

        public SimpleStore()
        {
            _cache = new Dictionary<string, byte[]>();
        }

        public void Set(string key, byte[] value)
        {
            _cache[key] = value;
        }
        public byte[] Get(string key) 
        {
            return _cache[key];
        }
        public void Delete(string key) 
        {
            _cache.Remove(key);
        }
    }
}
