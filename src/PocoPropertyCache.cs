using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace QueryHelper
{
    static class PocoPropertyCache
    {
        private static readonly List<PocoMappedProperties> PocoMetas = new List<PocoMappedProperties>();

        public static PocoMappedProperties GetPocoMeta(Type type)
        {
            var pocoMeta = PocoMetas.FirstOrDefault(pm => pm.PocoType == type);
            if (pocoMeta == null)
            {
                pocoMeta = CreatePocoMeta(type);
                PocoMetas.Add(pocoMeta);
            }
            return pocoMeta;
        }

        private static PocoMappedProperties CreatePocoMeta(Type type)
        {
            var pocoMeta = new PocoMappedProperties
            {
                PocoType = type,
                MappedProperties = type.GetProperties()
                    .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), false)).ToArray()
            };
            return pocoMeta;
        }
    }
}