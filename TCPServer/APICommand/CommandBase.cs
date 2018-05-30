using TCPServer.APIFeedback;
using System.Text.RegularExpressions;
using TCPServer.APIArgument;

namespace TCPServer.APICommand
{
    internal abstract class CommandBase : ICommand
    {
        protected IArgs[] Inputs;

        public abstract IArgs[] Arguments { get; }

        public virtual string CommandName
        {
            get
            {
                return GetType().Name;
            }
        }

        public abstract string Description { get; }

        public abstract IFeedBack ProcessCommand(TcpServer Server, string[] args);

        public static IFeedBack InvalidCommand()
        {
            return new InvalidFeedBack(InvalidFeedBackType.InvalidCommand);
        }

        public static IFeedBack InvalidInputParameter()
        {
            return new InvalidFeedBack(InvalidFeedBackType.InvalidInputParameter);
        }

        public static string GetAPICommand(string input, out string[] args)
        {
            args = new string[0];
            string command = null;
            string argument = null;

            Regex matcher = new Regex(@"(\(.*\))");
            Match inputMatch = matcher.Match(input);

            if (inputMatch.Success)
            {
                command = input.Substring(0, inputMatch.Index);
                argument = inputMatch.Value;
                argument = argument.Substring(1, argument.Length - 2);

                if (argument.Length > 0)
                {
                    args = argument.Split(',');
                }
            }
            return command;
        }

        public bool ArgumentsValidation(string[] arguments)
        {
            bool status;
            Inputs = Arguments;

            if (Inputs.Length == arguments.Length)
            {
                status = true;
                for (int i = 0; i < Inputs.Length; i++)
                {
                    if (!Inputs[i].SetValue(arguments[i]))
                    {
                        status = false;
                    }
                }
            }
            else
            {
                status = false;
            }

            return status;
        }
    }
}



