using System;
using System.Reflection;

namespace QueryHelper
{
    class PocoMappedProperties
    {
        public Type PocoType { get; set; }
        public PropertyInfo[] MappedProperties { get; set; }
    }
}