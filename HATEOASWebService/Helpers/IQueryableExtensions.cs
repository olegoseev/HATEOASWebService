using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using HATEOASWebService.Services;

namespace HATEOASWebService.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = mappingDictionary ?? throw new ArgumentNullException(nameof(mappingDictionary));

            if(string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            // split orderBy string 
            var orderByParts = orderBy.Split(',');

            // apply each orderby clasue in reverse order to get the right order
            foreach(var orderByClause in orderByParts.Reverse())
            {
                // trim the clause, as it maght contain leading or trailing spaces
                var trimOrderByClause = orderByClause.Trim();

                // check if order requested asc or desc
                var orderDescending = trimOrderByClause.EndsWith("desc");

                // remove order direction ("asc" or "desc") from the clause to extract the property name
                var indexOfFirstSpace = trimOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimOrderByClause : trimOrderByClause.Remove(indexOfFirstSpace);
                

                // check matching property
                if(!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mappinf for {propertyName} not found");
                }

                // getting property mapping value
                var propertyMappingValue = mappingDictionary[propertyName];
                _ = propertyMappingValue ?? throw new ArgumentNullException("propertyMappingValue");

                // Run through the property names in reverse order that orderby clauses are applied in the correct order
                foreach(var destinationProperty in propertyMappingValue.DestinationProperty.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}
