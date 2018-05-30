using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.APIArgument
{
    internal class StringArgs : ArgumentBase
    {
        private string _value;

        public StringArgs()
        {
            _value = "";
        }

        public override string GetValue()
        {
            return _value;
        }

        public override bool SetValue(string input)
        {
            _value = input;
            return true;
        }
    }
}
