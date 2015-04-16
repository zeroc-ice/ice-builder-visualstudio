using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ZeroC.IceVisualStudio
{
    //
    // This class is used to asynchronously read the output of a Slice compiler
    // process.
    //
    public class StreamReader
    {
        public void appendData(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
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
