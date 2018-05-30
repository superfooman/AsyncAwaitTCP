using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.APIArgument
{
    internal class DoubleArgs : ArgumentBase
    {
        private double _value;

        public DoubleArgs()
        {
            _value = 0.0;
        }

        public override string GetValue()
        {
            return _value.ToString();
        }

        public override bool SetValue(string input)
        {
            bool status = false;
            double result;
            if (double.TryParse(input, out result))
            {
                _value = result;
                status = true;
            }

            return status;
        }
    }
}
