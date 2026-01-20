using Xunit;
using Simpledb;
using SimpleDbIndexes;
using SimpleDbIndexes;
using System;
using System.IO;



namespace SimpleDatabase.Tests
{
    public class HashIndexTests : IndexContractTests
    {
        protected override IIndex<string, int> Create()
        {
            return new HashIndex<string, int>();
        }
    }
}