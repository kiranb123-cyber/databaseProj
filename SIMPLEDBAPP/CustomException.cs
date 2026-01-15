using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class StringException : Exception
{
    public StringException(string message)
        : base(message) { }

}


