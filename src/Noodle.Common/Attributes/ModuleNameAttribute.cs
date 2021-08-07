using System;

namespace Noodle.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleNameAttribute : Attribute
    {
        public ModuleNameAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }
    }
}