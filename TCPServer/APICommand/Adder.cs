using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class Adder : CommandBase
    {
        public override IArgs[] Arguments
        {
            get { return new IArgs[] { new DoubleArgs(), new DoubleArgs() }; }
        }

        public override string Description
        {
            get
            {
                return "Adding two double numbers";
            }
        }

        public override IFeedBack ProcessCommand(TcpServer Server, string[] args)
        {
            if (ArgumentsValidation(args))
            {
                DoubleArgs firstArg = Inputs[0] as DoubleArgs;
                DoubleArgs secondArg = Inputs[1] as DoubleArgs;

                string result = Server.Adder(double.Parse(firstArg.GetValue()), double.Parse(secondArg.GetValue())).ToString();
                return new ValidFeedBack(string.Format("Result is: {0}", result));
            }

            return InvalidInputParameter();
        }
    }
}
