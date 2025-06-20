using System;
using TaskAPI.Helpers;
using Xunit;

namespace TaskAPI.Tests
{
    public class MemoizerTests
    {
        [Fact]
        public void MemoizerTest()
        {
            int callCount = 0;

            Func<int, int> slowFunc = x =>
            {
                callCount++;
                return x * 2;
            };

            var memoized = Memoizer.Memoize(slowFunc);

            var result1 = memoized(5);
            var result2 = memoized(5); 

            Assert.Equal(10, result1);
            Assert.Equal(10, result2);
            Assert.Equal(1, callCount); 
        }
    }
}
