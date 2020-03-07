using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HATEOASWebService.Services
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperty { get; private set; }
        public bool Revert { get; private set; }

        public PropertyMappingValue(IEnumerable<string> destinationProperty, bool revert = false)
        {
            DestinationProperty = destinationProperty ?? throw new ArgumentNullException(nameof(destinationProperty));
            Revert = revert;
        }
    }
}
