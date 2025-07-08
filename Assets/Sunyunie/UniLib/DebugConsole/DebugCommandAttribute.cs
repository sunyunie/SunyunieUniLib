using System;

namespace Sunyunie.UniLib
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public class DebugCommandAttribute : Attribute
    {
        public string commandName;
        public string description;

        public DebugCommandAttribute(string name, string desc = "")
        {
            commandName = name;
            description = desc;
        }
    }
}