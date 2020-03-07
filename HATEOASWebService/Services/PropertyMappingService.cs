using System;
using System.Collections.Generic;
using System.Linq;
using HATEOASWebService.Data.Entities;
using HATEOASWebService.Data.Models;

namespace HATEOASWebService.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private static readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = new PropertyMappingValue(new List<string>() { "Id" }),
                ["MainCategory"] = new PropertyMappingValue(new List<string>() { "MainCategory" }),
                ["Age"] = new PropertyMappingValue(new List<string>() { "DateOfBirth" }, true),
                ["Name"] = new PropertyMappingValue(new List<string>() { "FirstName", "LastName" }),
            };


        IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // get matching mapping
            var matchingMappping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMappping.Count() == 1)
            {
                return matchingMappping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            var fieldsParts = fields.Split(',');

            foreach(var field in fieldsParts)
            {
                var trimField = field.Trim();

                // remove everything after the first " ". if the fields are coming from an orderBy string,
                // this part must be ignored
                var indexOfFirstSpace = trimField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimField : trimField.Remove(indexOfFirstSpace);

                // check if there is matching property
                if(!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
