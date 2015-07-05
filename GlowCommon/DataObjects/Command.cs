using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlowCommon;

namespace GlowCommon.DataObjects
{
    public class Command
    {
        public const uint COMMAND_VERSION = 1;

        public const uint COMMAND_GET_SETTINGS = 300;
        public const uint COMMAND_RECIEVE_SETTINGS = 301;

        /// <summary>
        /// Indicates which program this command is for.
        /// </summary>
        public GlowPrograms Program = GlowPrograms.None;

        /// <summary>
        /// Used by the program to switch command types.
        /// </summary>
        public uint MessageId = 0;

        /// <summary>
        /// The actual command for the program.
        /// </summary>
        public string Message = "";

        /// <summary>
        /// Defines what version these commands are.
        /// </summary>
        public uint Version = COMMAND_VERSION;

        public Command()
        {}

        public Command(GlowPrograms program, uint messageId, string message)
        {
            Program = program;
            MessageId = messageId;
            Message = message;
        }
    }
}
