using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.APIArgument
{
    internal abstract class ArgumentBase : IArgs
    {
        public abstract bool SetValue(string input);

        public abstract string GetValue();
    }
}

