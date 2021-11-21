using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class Sqrt : CommandBase
    {
        public override IArgs[] Arguments
        {
            get { return new IArgs[] { new IntegerArgs()}; }
        }

        public override string Description
        {
            get
            {
                return "Sqrt of an interger, return an integer (its decimal is truncated)";
            }
        }

        public override IFeedBack ProcessCommand(TcpServer Server, string[] args)
        {
            if (ArgumentsValidation(args))
            {
                IntegerArgs arg = Inputs[0] as IntegerArgs;

                string result = Server.Sqrt(int.Parse(arg.GetValue())).ToString();
                return new ValidFeedBack(string.Format("Result is: {0}", result));
            }

            return InvalidInputParameter();
        }
    }
}
