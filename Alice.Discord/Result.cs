using System.Runtime.ConstrainedExecution;
using Discord.Commands;
using JetBrains.Annotations;

namespace Alice.Discord
{
    public class Result : RuntimeResult
    {
        private Result(CommandError? error, string reason) : base(error, reason)
        {}
        
        public static Result Successful => new Result(null, null);
        public static Result UnknownError => new Result(CommandError.Unsuccessful, null);
        
        [StringFormatMethod("message")]
        public static Result Error(string message, params object[] args)
        {
            return new Result(CommandError.Unsuccessful, string.Format(message, args));
        }
    }
}