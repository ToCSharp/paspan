﻿namespace Paspan;

public class ParseException : Exception
{
    public ParseException(string message/*, in TextPosition position*/) : base(message)
    {
        //Position = position;
    }

    //public TextPosition Position { get; set; }
}
