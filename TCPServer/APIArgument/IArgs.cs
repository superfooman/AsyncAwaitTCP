using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.APIArgument
{
    public interface IArgs
    {
        bool SetValue(string input);
        string GetValue();
    }
}
