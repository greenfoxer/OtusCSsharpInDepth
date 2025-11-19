using cache.SimpleStoreSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cache.UnitTests
{
    public class HomeWork2 : IAsyncLifetime
    {
        public Task DisposeAsync()
        {
            _store.Dispose();
            return Task.CompletedTask;
        }
        private SimpleStore _store;
        public Task InitializeAsync()
        {
            _store = new SimpleStore();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task SimpleStoreShouldMakeCorrectSetOperationStatisticsWithParallelWork()
        {
            var workTasks = new List<Task>();
            var iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                workTasks.Add(Task.Run(() => RunAddRequest(i)));
            }
            for (int i = 0; i < iterations; i++)
            {
                workTasks.Add(Task.Run(() => RunGetRequest(i)));
            }
            for (int i = 0; i < iterations; i++)
            {
                workTasks.Add(Task.Run(() => RunDelRequest(i)));
            }

            await Task.WhenAll(workTasks);
            
            var statistic = _store.GetStatistics();
            Assert.Equal(iterations, statistic.SetCount);
            Assert.Equal(iterations, statistic.GetCount);
            Assert.Equal(iterations, statistic.DeleteCount);
        }
        private byte[] _testPrefix = { 0xD, 0xE, 0xA, 0xD, 0xB, 0xE, 0xE, 0xF };
        private void RunAddRequest(int iteration)
        {
            var key = $"user:{iteration}";
            var value = _testPrefix.Concat(BitConverter.GetBytes(iteration)).ToArray();
            _store.Set(key, value);
        }
        private void RunGetRequest(int iteration)
        {
            var key = $"user:{iteration}";
            _ = _store.Get(key);
        }
        private void RunDelRequest(int iteration)
        {
            var key = $"user:{iteration}";
            _store.Delete(key);
        }
    }
}
