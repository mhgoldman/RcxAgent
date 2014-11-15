using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Rcx
{
    class CommandManager
    {
        private Dictionary<string, Command> commands = new Dictionary<string, Command>();

        #region singleton
        private static CommandManager _default;
        public static CommandManager Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new CommandManager();
                }
                return _default;
            }
        }
        #endregion

        private CommandManager()
        {
            commands = new Dictionary<string, Command>();
        }

        public void AddCommand(string guid, string path, string[] args)
        {
            commands[guid] = new Command(path, args);
        }

        public Dictionary<string, Command> GetCommands()
        {
            return commands;
        }

        public Command GetCommand(string guid)
        {
            if (commands[guid] == null)
            {
                throw new ArgumentException(String.Format("Command with id {0} was not found.", guid));
            }

            return commands[guid].Update();
        }

        public void KillCommand(string guid)
        {
            if (commands[guid] == null)
            {
                throw new ArgumentException(String.Format("Command with id {0} was not found.", guid));
            }

            commands[guid].Kill();
        }
    }
}
