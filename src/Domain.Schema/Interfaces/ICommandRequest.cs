﻿using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;

namespace OMP.Connector.Domain.Schema.Interfaces
{
    [JsonConverter(typeof(CommandRequestConverter))]
    public interface ICommandRequest { }
}