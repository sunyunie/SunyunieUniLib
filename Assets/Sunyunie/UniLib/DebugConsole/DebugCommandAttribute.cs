using System;

namespace Sunyunie.UniLib
{
    /// <summary>
    /// 디버그 명령어를 정의하는 어트리뷰트.
    /// 간단히 명령어 이름과 설명을 저장.
    /// </summary>
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