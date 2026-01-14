using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

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

        //constructor

        public SimpleDatabase(string Path)
        {
            _index = new Dictionary<string, long>();
            
            _file = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            LoadIndex();
        }
        private void LoadIndex()
        {
            _file.Seek(0, SeekOrigin.Begin);
            while(_file.Position < _file.Length)
            {
                long recordOffset = _file.Position;
                 
                // Read record header
                int KeyLen = ReadInt32();
                int ValueLen = ReadInt32();
                byte Flags = ReadByte();

                string key = ReadString(KeyLen);

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
        public string Get(string key)
        {
            if(!_index.TryGetValue(key, out long offset))
            {
                return "this entry doesn't exist!";
            }
            _file.Seek(offset, SeekOrigin.Begin);
            int keyLen = ReadInt32();
            int valueLen = ReadInt32();
            ReadByte(); // flags
            ReadString(keyLen);
            return ReadString(valueLen);
        }
        //put(updates that keys offset with a new variable)
        public void Put(string key, string value)
        {
            long offset = _file.Seek(0, SeekOrigin.End);
            //writes the record header
            WriteInt32(key.Length);
            WriteInt32(value.Length);
            WriteByte(0); // 0 = active record
            
            //writes the recorded data
            WriteString(key);
            WriteString(value);
            //enmsures data is writen to the disk
            _file.Flush();
            //updates in-memory index 
            _index[key] = offset;
        }

        //delete
        public void Delete(string key)
        {
            //long offset = _file.Seek(0, SeekOrigin.End); can be ommited as we dont need the offset for delete
            // but can be used later for a restore function
            _file.Seek(0, SeekOrigin.End);
            WriteInt32(key.Length);
            WriteInt32(0);
            WriteByte(1); // deletes the information
            WriteString(key);

            _file.Flush();
            _index.Remove(key);
        }

        
        
        
        
        
        //helpers
        //Every byte written must be read back in the exact same order, size, and encoding.
        //otherwise risk a corrupted db
        private int ReadInt32()// reads 4 bytes then converts to a int then advances the file pointer by 4 bytes
        {
            byte[] buf = new byte[4];
            _file.ReadExactly(buf);
            return BitConverter.ToInt32(buf, 0);
        }
        
        private void WriteInt32(int value)//converts int to 4 bytes and advances file pointer by 4 bytes
        {
            //Every integer is written as exactly 4 bytes, in the same byte order.
            byte[] bytes = BitConverter.GetBytes(value);
            _file.Write(bytes, 0, 4);
        }


        private void WriteByte(byte value)//this writes exactly one byte
        {
            _file.WriteByte(value);
        }

        private byte ReadByte()//this reads exactly one byte
        {
            return (byte)_file.ReadByte();
        }

        private void WriteString(string value)// Converts string -> UTF-8 bytes and moves the file pointer forward
        {
            byte[] buf = Encoding.UTF8.GetBytes(value);
            _file.Write(buf, 0, buf.Length);
        }

        private string ReadString(int length) // Reads length bytes then converts bytes using UTF8 to a string
        {
            byte[] buf = new byte[length];
            _file.ReadExactly(buf);
            return Encoding.UTF8.GetString(buf);
        }
    }

}