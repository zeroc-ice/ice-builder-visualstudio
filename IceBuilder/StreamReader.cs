// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System.Diagnostics;

namespace IceBuilder
{
    //
    // This class is used to asynchronously read the output of a Slice compiler
    // process.
    //
    public class StreamReader
    {
        public void appendData(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if(outLine.Data != null)
            {
                _data += outLine.Data + "\n";
            }
        }

        public string data()
        {
            return _data;
        }

        private string _data = "";
    }
}
