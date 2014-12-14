using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rcx
{
    class CommandManager
    {
        private ConcurrentDictionary<string, Command> commands;

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
            commands = new ConcurrentDictionary<string, Command>();
        }

        public Command AddCommand(string guid, string path, string[] args, string callbackUrl = null)
        {
            Command c = new Command(guid, path, args, callbackUrl);

            if (!commands.TryAdd(guid, c))
            {
                throw new ArgumentException(String.Format("Command {0} already exists", guid));
            }

            return c;
        }

        public ConcurrentDictionary<string, Command> GetCommands()
        {
            Update();
            return commands;
        }

        public Command GetCommand(string guid)
        {
            Command c = null;
            
            if (!commands.TryGetValue(guid, out c))
            {
                throw new ArgumentException(String.Format("Command with id {0} was not found.", guid));
            }

            return c.Update();
        }

        public void KillCommand(string guid)
        {
            Command c = null;

            if (!commands.TryGetValue(guid, out c))
            {
                throw new ArgumentException(String.Format("Command with id {0} was not found.", guid));
            }

            c.Kill();
        }

        #region helpers
        private void Update()
        {
            foreach (KeyValuePair<string, Command> kvp in commands)
            {
                kvp.Value.Update();
            }
        }
        #endregion
    }
}
