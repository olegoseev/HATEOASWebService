﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HATEOASWebService.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>( this IEnumerable<TSource> source, string fields)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));

            // create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            // create a list with PropertyInfo objects on TSource. Reflection is expensive, so rather than doing it
            // for each object in the list, we do it once and reuse the results. After all, part of the reflection
            // is on the type of the object (TSource), not on the instance
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // splitting fields
                var fieldPatrs = fields.Split(',');

                foreach(var field in fieldPatrs)
                {
                    var propertyName = field.Trim();

                    var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    _ = propertyInfo ?? throw new Exception($"Property {propertyName} not found on {typeof(TSource)}");
                    
                    propertyInfoList.Add(propertyInfo);
                }
            }

            // run through the source objects
            foreach(TSource sourceObject in source)
            {
                // create an ExpandoObject that will hold the selected properties and values
                var dataShapedObject = new ExpandoObject();

                // Get the value of each property we have to return
                foreach(var propertyInfo in propertyInfoList)
                {
                    // getting value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);
                    // add the filed to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }
                expandoObjectList.Add(dataShapedObject);
            }
            return expandoObjectList;
        }
    }
}
