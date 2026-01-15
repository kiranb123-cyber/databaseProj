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
using System.Threading.Tasks.Dataflow;
using System.Configuration.Assemblies;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Net;
using System.Xml;
using System.Net.NetworkInformation;

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
            db.Put("name","kiran");
            db.Delete("name");
            var value = db.Get("name");
            Assert.Null(value);
        }

        File.Delete(path);
    }
    
    [Fact]
    public void DataPersistsAcrossRestartTest()
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
    
    [Fact]
    public void StringPath_TooLongExceptionTest()
    {
        string path = NewTempPath();
        using (var db = new SimpleDatabase(path)) 
        {
            Assert.Throws<StringException>(() => db.Put("name", "The old clock tower stood as a silent sentinel over the forgotten town of Oakhaven. Its gears, rusted and long since ceased moving, held the final moments of July 1957 captive in time. Elara, a curious young girl with a spirit as vibrant as the wildflowers that now carpeted the cobblestone streets, believed the clock's silence was a riddle waiting to be solved. Legend said a powerful secret was locked within its mechanisms, a secret only a pure heart could uncover. On a stormy evening, driven by an unyielding desire to breathe life back into her quiet home, Elara began her ascent up the precarious, winding staircase. Each creaking step echoed her determination, a small whisper in the face of time's powerful stillness."));
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
            
            //going to check and see how large the file is before deletion
            long sizeBefore = new FileInfo(path).Length;
            db.Compact();
            long sizeAfter = new FileInfo(path).Length;
            bool IsSizeSmaller = false;
            if(sizeBefore > sizeAfter)
            {
                IsSizeSmaller = true;
            }
            Assert.True(IsSizeSmaller);
            
        }
        File.Delete(path);
    }
    [Fact]
    public void UpdateAppendWorksJustLikeAStackTest()
    {
        string path = NewTempPath();
        using (var db = new SimpleDatabase(path))
        {
            //gave name and age
            db.Put("name", "kiran");
            db.Put("age", "200");
            //but since this is like a stack for now this should update name and put it back on top of the stack and update 
            //its value to 200
            db.Put("name", "200");
            Assert.Equal("200", db.Get("name"));
        }
        File.Delete(path);
    }

    
    
}

