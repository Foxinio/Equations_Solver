using System;
using System.Linq;

namespace Interpreter_Równań {
    public enum CommandType { CleanScreen, Help };

    public static class CommandHandler {
        public static Result Execute(CommandType command, string[] args = null) {
            switch (command) {
            case CommandType.CleanScreen:
                return Commands.CleanScreen();
            case CommandType.Help:
                return Commands.Help();
            default:
                return new Result(false);
            }
        }
        public static string CommandTypeToString(CommandType commandType) {
            return Commands.array[(int)commandType];
        }
        public static CommandType? StringToCommandType(string input) {
            foreach (var item in Commands.array) {
                if (item == input) {
                    return (CommandType)(Commands.array.ToList().IndexOf(item));
                }
            }
            return null;
        }
    }

    static class Commands {
        public static string[] array = new string[] { "cls", "help" };

        public static Result CleanScreen() {
            Console.Clear();
            return new Result(true);
        }

        public static Result Help() {
            string message = "Sorry, too lazy to implement it.";
            //implement it
            return new Result(true, message);
        }
    }
}
