using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cache.SimpleStoreSystem
{
    public class SimpleStore : IDisposable
    {
        private readonly Dictionary<string, byte[]> _cache;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private long _setCount, _getCount, _deleteCount;

        public SimpleStore()
        {
            _cache = new Dictionary<string, byte[]>();
            _setCount = 0;
            _getCount = 0;
            _deleteCount = 0;
        }
        public (long SetCount,long GetCount,long DeleteCount) GetStatistics()
        {
            long sets = Interlocked.Read(ref _setCount);
            long gets = Interlocked.Read(ref _getCount);
            long deletes = Interlocked.Read(ref _deleteCount);
            return (sets, gets, deletes);
        }
        public void Set(string key, byte[] value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    _lock.EnterWriteLock();
                    _cache[key] = value;
                    Interlocked.Increment(ref _setCount);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        public byte[]? Get(string key) 
        {
            try
            {
                _lock.EnterReadLock();

                if (_cache.TryGetValue(key, out var value))
                {
                    Interlocked.Increment(ref _getCount);
                    return value;
                }

            }
            finally
            {
                _lock.ExitReadLock();
            }

            return null;
        }
        public void Delete(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    _lock.EnterWriteLock();
                    _cache.Remove(key);
                    Interlocked.Increment(ref _deleteCount);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        #region IDisposable
        private bool _disposed = false;
        ~SimpleStore() 
        {
            Dispose(false);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _lock.Dispose();
                    _cache.Clear();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
