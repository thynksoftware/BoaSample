
using System;
using Microsoft.Exchange.WebServices.Data;

namespace Boa.Sample.Models
{
    public class EmailHeader
    {
        public EmailHeader(){}

        public EmailHeader(Guid guid, string name, string value)
        {
            Guid = guid;
            Name = name;
            Value = value;
        }
        
        public EmailHeader(ExtendedProperty prop)
        {
            Guid = prop?.PropertyDefinition?.PropertySetId;
            Name = prop?.PropertyDefinition?.Name;
            Value = prop?.Value?.ToString();
        }
        
        public Guid? Guid { get; set; }

        public string Name { get; set; }
        
        public string Value { get; set; }
    }
}