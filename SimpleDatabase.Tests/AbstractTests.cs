using Xunit;
using Simpledb;
using System;
using System.IO;
using SimpleDbIndexes;

namespace SimpleDatabase.Tests
{
//index contract tests
    public abstract class IndexContractTests
    {
        
        protected abstract IIndex<string, int> Create();

        [Fact]
        public void InsertAndGet()
        {
            var index = Create();
            index.Upsert("a", 1);
            Assert.True(index.TryGet("a", out var value));
            Assert.Equal(1, value);
        }
        
        [Fact]
        public void OverWriteReplacesValue()
        {
            var index = Create();
            index.Upsert("a", 1);
            index.Upsert("a", 2);
            Assert.True(index.TryGet("a", out var value));
            Assert.Equal(2, value);
        }

    }
}