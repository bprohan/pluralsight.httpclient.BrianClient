using System;
using Xunit;

namespace BrianClientTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var x = 4;
            Assert.Equal(16, Squrare(x));

            Assert.Equal(16, square(x));

        }

        Func<int, int> square = x => { return x * x; };

        private int Squrare(int x) => x * x;
    }
}
