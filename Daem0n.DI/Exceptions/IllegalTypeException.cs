using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public class IllegalTypeException : Exception
    {
        public Type SourceType { get; }
        public Type TargetType { get; }
        public override string Message => $"{TargetType.Name}不继承{SourceType.Name}或者{TargetType.Name}是不能实例化的类型";
        public IllegalTypeException(Type tSource, Type tTarget)
        {
            this.SourceType = tSource;
            this.TargetType = tTarget;
        }
    }
}
