using CDR.Register.IdentityServer.Caching;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class IdentityServerUnitTests
    {
        [Fact]
        public void Lru_Cache_Simultaneous_Calls()
        {
            //Arrange
            int maxIterations = 1000;
            string tempCacheFileName = Path.GetTempFileName();
            var lruCache = new LruCache(tempCacheFileName);

            //Action
            var result = Parallel.For(0, maxIterations, (i, state) =>
            {
                lruCache.AddCache(Guid.NewGuid().ToString(), DateTime.Now.AddMilliseconds(200));
            });

            var lruCacheCopy = new LruCache(tempCacheFileName);

            // Assert
            Assert.True(result.IsCompleted);
            Assert.Equal(lruCache.Cache.Count, lruCacheCopy.Cache.Count);

            // Clean-up
            File.Delete(tempCacheFileName);
        }
    }
}
