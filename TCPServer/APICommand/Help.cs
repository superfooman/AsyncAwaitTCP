using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.APIArgument;
using TCPServer.APIFeedback;

namespace TCPServer.APICommand
{
    internal class Help : CommandBase
    {
        public override string Description
        {
            get
            {
                return "Obtain the names and descritpions for each available API commands";
            }
        }

        public override IArgs[] Arguments
        {
            get { return new IArgs[0]; }
        }

        public override IFeedBack ProcessCommand(TcpServer Server, string[] args)
        {
            if (ArgumentsValidation(args))
            {
                var APICommandDictionary = Server.Help();
                const string parenthesis = "()";

                StringBuilder feedBackMessage = new StringBuilder();
                foreach (var APICommand in APICommandDictionary)
                {
                    feedBackMessage.Append(string.Format("{0}{1}{2}  \'{3}\'", Environment.NewLine, APICommand.Key, parenthesis, APICommand.Value));
                }
                feedBackMessage.Append(Environment.NewLine);

                return new ValidFeedBack(feedBackMessage.ToString());
            }

            return InvalidInputParameter();
        }
    }
}
