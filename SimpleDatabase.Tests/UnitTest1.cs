using Xunit;
using Simpledb;
using System;
using System.IO;

namespace Simpledb.Tests
{
    public class DatabaseBasicTests
    {
        private static string NewTempPath()
        {
            return Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".db"
            );
        }

        [Fact]
        public void DoPutThenDoGetReturnsValueTest()
        {
            string path = NewTempPath();
            using (var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
                Assert.Equal("kiran", db.Get("name"));
            }
            File.Delete(path);
        }

        [Fact]
        public void Delete_RemovesKeyTest()
        {
            string path = NewTempPath();
            using (var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
                db.Delete("name");
                Assert.Null(db.Get("name"));
            }
            File.Delete(path);
        }

        [Fact]
        public void DataPersistsAcrossRestartTest()
        {
            string path = NewTempPath();

            using (var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
            }

            using (var db = new SimpleDatabase(path))
            {
                Assert.Equal("kiran", db.Get("name"));
            }

            File.Delete(path);
        }

        [Fact]
        public void Compact_RemovesDeletedRecordsTest()
        {
            string path = NewTempPath();
            using (var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
                db.Put("age", "200");
                db.Delete("name");

                long sizeBefore = new FileInfo(path).Length;
                db.Compact();
                long sizeAfter = new FileInfo(path).Length;

                Assert.True(sizeAfter < sizeBefore);
            }
            File.Delete(path);
        }

        [Fact]
        public void UpdateAppendWorksJustLikeAStackTest()
        {
            string path = NewTempPath();
            using (var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
                db.Put("age", "200");
                db.Put("name", "200");

                Assert.Equal("200", db.Get("name"));
            }
            File.Delete(path);
        }
    }
}