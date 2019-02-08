using System;

namespace DataTables.NetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTDataSource : Attribute
    {
        public Type Source { get; set; }

        public DTDataSource(Type source)
        {
            Source = source;
        }
    }
}
