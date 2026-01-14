using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Threading;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
namespace SimpleDataBase 
{
    public class SimpleDatabase 
    {
        // its the indexing for the database, not the actual db
        //points to the offset
        //the offset is where the data lives 
        private Dictionary<string, long> _index;
        //the file backing the database
        private  FileStream _file ; 
        //The lock must be a field of the database class, shared by all operations.
        //is used for compaction as well as read and write operations
        private readonly ReaderWriterLockSlim rws = new ReaderWriterLockSlim();
        //constructor
        private string _dbPath;
        public SimpleDatabase(string Path)
        {
            _index = new Dictionary<string, long>();
            
            _file = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _dbPath = Path;
            LoadIndex();
        }
        private void LoadIndex()
        {
            _file.Seek(0, SeekOrigin.Begin);
            while(_file.Position < _file.Length)
            {
                long recordOffset = _file.Position;
                 
                // Read record header
                int KeyLen = ReadInt32(_file);
                int ValueLen = ReadInt32(_file);
                byte Flags = ReadByte(_file);

                string key = ReadString(_file, KeyLen);

                if(Flags == 1) //deleted
                {
                    _index.Remove(key);
                    _file.Seek(ValueLen, SeekOrigin.Current);

                }
                else
                {
                    _index[key] = recordOffset;
                    _file.Seek(ValueLen, SeekOrigin.Current);
                }

            }
        }
        
        //db requests
        
        //get
        public string? Get(string key)
        {
            rws.EnterReadLock();
            try
            {
                if(!_index.TryGetValue(key, out long offset))
                {
                    return null;
                }
                _file.Seek(offset, SeekOrigin.Begin);
                int keyLen = ReadInt32(_file);
                int valueLen = ReadInt32(_file);
                ReadByte(_file); // flags
                ReadString(_file, keyLen);
                return ReadString(_file, valueLen);
            }
            finally
            {
                rws.ExitReadLock();
            }
        }
        //put(updates that keys offset with a new variable)
        public void Put(string key, string value)
        {
            rws.EnterWriteLock();
            try
            {
                long offset = _file.Seek(0, SeekOrigin.End);
                //writes the record header
                WriteInt32(_file, key.Length);
                WriteInt32(_file, value.Length);
                WriteByte(_file, 0); // 0 = active record
                
                //writes the recorded data
                WriteString(_file, key);
                WriteString(_file,value);
                //enmsures data is writen to the disk
                _file.Flush();
                //updates in-memory index 
                _index[key] = offset;
            }
            finally
            {
                rws.ExitWriteLock();
            }
        }

        //delete
        public void Delete(string key)
        {
            rws.EnterWriteLock();
            try
            {
                //long offset = _file.Seek(0, SeekOrigin.End); can be ommited as we dont need the offset for delete
                // but can be used later for a restore function
                _file.Seek(0, SeekOrigin.End);
                WriteInt32(_file, key.Length);
                WriteInt32(_file, 0);
                WriteByte(_file, 1); // deletes the information
                WriteString(_file,key);

                _file.Flush();
                _index.Remove(key);
            }
            finally
            {
                rws.ExitWriteLock();
            }
        }

        
        
        //compacter(removes deleted entries from the db file basically tombstones)
        public void Compact()
        {
            
            // Stop writes (shared lock defined at class level)
            rws.EnterWriteLock();
            try
            {
                var newIndex = new Dictionary<string, long>();
                string tempPath = _dbPath + ".compact"; //creates a new temporary path 
                //store old path
                _file.Seek(0, SeekOrigin.Begin);
                var oldFile = _file; //never open 2 of the same files at the same time, do file.seek and just store it as a diff variable
                //set up new path
                using (var tempFile = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                //now heres where we copy over the 'live records' the stuff with stuff in them
                    foreach (KeyValuePair<string, long> key in _index)  //KeyValuePair<int,string> item in dictionaryobject
                    {
                        oldFile.Seek(key.Value, SeekOrigin.Begin);
                        //read record header from old file
                        int keyLen = ReadInt32(oldFile);
                        int valueLen = ReadInt32(oldFile);
                        ReadByte(oldFile);
                        //now its time to read the keys and values of the bytes
                        string oldKey = ReadString(oldFile, keyLen);
                        string value = ReadString(oldFile, valueLen);

                        //appends to the new file
                        long newOffSet = tempFile.Position;

                        WriteInt32(tempFile, keyLen);
                        WriteInt32(tempFile, valueLen);
                        
                        WriteByte(tempFile, 0); // 0 = active record
                        WriteString(tempFile, oldKey);
                        WriteString(tempFile, value);

                        //now its time to update the new index into the compacted file
                        newIndex[oldKey] = newOffSet; 
                    }
                }

                //replaces old file with new compacted file
                _file.Close();
                File.Replace(tempPath, _dbPath, null); //atomic operation definintion: it happens or it doesnt
                
                _file = new FileStream(_dbPath, FileMode.Open, FileAccess.ReadWrite);
                _index = newIndex;
            }
            finally
            {
            //now we can leave the write mode
            rws.ExitWriteLock();
            }
        }
        
        
        
        
        //helpers
        //Every byte written must be read back in the exact same order, size, and encoding.
        //otherwise risk a corrupted db
        private int ReadInt32(FileStream file)// reads 4 bytes then converts to a int then advances the file pointer by 4 bytes
        {
            byte[] buf = new byte[4];
            file.ReadExactly(buf);
            return BitConverter.ToInt32(buf, 0);
        }

        private void WriteInt32(FileStream file, int value)//converts int to 4 bytes and advances file pointer by 4 bytes
        {
            //Every integer is written as exactly 4 bytes, in the same byte order.
            byte[] bytes = BitConverter.GetBytes(value);
            file.Write(bytes, 0, 4);
        }


        private void WriteByte(FileStream file, byte value)//this writes exactly one byte
        {
            file.WriteByte(value);
        }

        private byte ReadByte(FileStream file)//this reads exactly one byte
        {
            int b = file.ReadByte();
            if(b == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)b;
        }

        private void WriteString(FileStream file, string value)// Converts string -> UTF-8 bytes and moves the file pointer forward
        {
            byte[] buf = Encoding.UTF8.GetBytes(value);
            file.Write(buf, 0, buf.Length);
        }

        private string ReadString(FileStream file,int length) // Reads length bytes then converts bytes using UTF8 to a string
        {
            byte[] buf = new byte[length];
            file.ReadExactly(buf);
            return Encoding.UTF8.GetString(buf);
        }
    }

}