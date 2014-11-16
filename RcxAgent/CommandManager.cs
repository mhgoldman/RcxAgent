using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

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

        public void AddCommand(string guid, string path, string[] args)
        {
            if (!commands.TryAdd(guid, new Command(path, args)))
            {
                throw new ArgumentException(String.Format("Command {0} already exists", guid));
            }
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
