﻿using NJsonSchema.Annotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes
{
    public class TimeStampAttribute : JsonSchemaExtensionDataAttribute
    {
        public TimeStampAttribute() : base("pattern", RegexConstants.Timestamp)
        {
        }
    }
}