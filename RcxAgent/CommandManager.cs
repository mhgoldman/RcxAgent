using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rcx
{
    class CommandManager
    {
        private ConcurrentDictionary<string, Command> commands;

        private bool Mayday
        {
            get; set;
        }

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
            Mayday = false;
        }

        public Command AddCommand(string guid, string path, string[] args, string callbackUrl = null, string callbackToken = null)
        {
            if (Mayday)
            {
                throw new Exception("Cannot add command because the agent is shutting down");
            }

            Command c = new Command(guid, path, args, callbackUrl, callbackToken);

            if (!commands.TryAdd(guid, c))
            {
                throw new ArgumentException(String.Format("Command {0} already exists", guid));
            }

            return c;
        }

        public ConcurrentDictionary<string, Command> GetCommands()
        {
            return commands;
        }

        public Command GetCommand(string guid)
        {
            Command c = null;
            
            if (!commands.TryGetValue(guid, out c))
            {
                throw new ArgumentException(String.Format("Command with id {0} was not found.", guid));
            }

            return c;
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

        public void OnMayday()
        {
            Mayday = true;

            //TODO - it'd be good to only send mayday message if final callback wasn't already sent.
            //unfortunately, we can't identify which callback is the final one, so there's no real way to do this.
            //as a result, this will send mayday messages for every command that's run since the service started.
            foreach (Command c in commands.Values)
            {
                c.OnMayday();
            }
        }
    }
}
