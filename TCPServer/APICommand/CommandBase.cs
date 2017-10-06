using System;
using TCPServer.APIFeedback;
using System.Text.RegularExpressions;

namespace TCPServer.APICommand
{
    internal abstract class CommandBase : ICommand
    {
        public string Command
        {
            get
            {
                return GetType().Name;
            }
        }

        public abstract string Description { get; }

        public abstract IFeedBack ProcessCommand(TcpServer Server);

        public static IFeedBack InvalidCommand()
        {
            return new InvalidFeedBack();
        }

        public static string GetAPICommand(string input)
        {
            string command = null;
            Regex matcher = new Regex(@"\(\s*\)");
            Match inputMatch = matcher.Match(input);

            if (inputMatch.Success)
            {
                command = input.Substring(0, inputMatch.Index);
            }
            return command;
        }
    }
}



