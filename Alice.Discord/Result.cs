using System.Runtime.ConstrainedExecution;
using Discord.Commands;
using JetBrains.Annotations;

namespace Alice.Discord
{
    public class Des {
        public object[] _args;
        public string _message;
        public Des(string message, object[] args) {
            _message = message;
            _args = args;
        }
    }

    public class Result : RuntimeResult
    {
        private Result(CommandError? error, string reason) : base(error, reason)
        {}
        
        public static Result Successful => new Result(null, null);
        public static Result UnknownError => new Result(CommandError.Unsuccessful, null);
        
        [StringFormatMethod("message")]
        public static Result Error(Des des)
        {
            return new Result(CommandError.Unsuccessful, string.Format(des._message, des._args));
        }
    }
}