using Xunit;
using SimpleDataBase;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Data.Common;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Runtime;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace UnitTest1.Tests;



public class DatabaseBasicTests
{
    private static string NewTempPath()//ensures that we dont reuse test.db which may have data from previous runs                                    
    { //each test gets its own temp file and prevents file locking issues
        return Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString() + ".db"
        );
    }


    [Fact]
    public void DoPutThenDoGetReturnsValue()
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
    public void Delete_RemovesKey()
    {
        string path = NewTempPath();
        using (var db = new SimpleDatabase(path))
        {
            db.Put("name","kiran");
            db.Delete("name");
            var value = db.Get("name");
            Assert.Null(value);
        }

        File.Delete(path);
    }
    
    [Fact]
    public void DataPersistsAcrossRestart()
    {
        
        string path = NewTempPath();
        {
            using(var db = new SimpleDatabase(path))
            {
                db.Put("name", "kiran");
            }
            
        }
        {
            using(var db = new SimpleDatabase(path))
            {
                var value = db.Get("name");
                Assert.Equal("kiran", value);  
            }
            

        }
        File.Delete(path);
        
    }

      
}

