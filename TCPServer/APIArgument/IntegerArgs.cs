using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.APIArgument
{
    internal class IntegerArgs : ArgumentBase
    {
        private int _value;

        public IntegerArgs()
        {
            _value = 0;
        }

        public override string GetValue()
        {
            return _value.ToString();
        }

        public override bool SetValue(string input)
        {
            bool status = false;
            int result;
            if (int.TryParse(input, out result))
            {
                _value = result;
                status = true;
            }

            return status;
        }
    }
}
