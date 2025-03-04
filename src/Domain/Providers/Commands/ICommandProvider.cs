﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Providers.Commands
{
    public interface ICommandProvider
    {
        Task<IEnumerable<ICommandResponse>> ExecuteAsync();
    }
}