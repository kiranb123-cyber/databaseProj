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
    
    [Fact]
    public void StringPath_TooLong()
    {
        string path = NewTempPath();
        using (var db = new SimpleDatabase(path)) 
        {
            Assert.Throws<StringException>(() => db.Put("name", "The old clock tower stood as a silent sentinel over the forgotten town of Oakhaven. Its gears, rusted and long since ceased moving, held the final moments of July 1957 captive in time. Elara, a curious young girl with a spirit as vibrant as the wildflowers that now carpeted the cobblestone streets, believed the clock's silence was a riddle waiting to be solved. Legend said a powerful secret was locked within its mechanisms, a secret only a pure heart could uncover. On a stormy evening, driven by an unyielding desire to breathe life back into her quiet home, Elara began her ascent up the precarious, winding staircase. Each creaking step echoed her determination, a small whisper in the face of time's powerful stillness."));
        }
        File.Delete(path);
    }
}

