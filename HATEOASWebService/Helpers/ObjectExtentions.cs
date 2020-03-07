using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HATEOASWebService.Helpers
{
    public static class ObjectExtentions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields = null)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));

            var dataShapeObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all properties should be in the ExpandoObject
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                foreach(var propertyInfo in propertyInfos)
                {
                    // get the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)dataShapeObject).Add(propertyInfo.Name, propertyValue);
                }

                return dataShapeObject;
            }

            var fieldParts = fields.Split(',');

            foreach(var field in fieldParts)
            {
                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a 
                // binding flag overwrites the already-existing binding flags.

                var propertyName = field.Trim();

                var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                _ = propertyInfo ?? throw new Exception($"Property {propertyName} not found on {typeof(TSource)}");

                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>)dataShapeObject).Add(propertyName, propertyValue);
            }

            return dataShapeObject;
        }
    }
}
