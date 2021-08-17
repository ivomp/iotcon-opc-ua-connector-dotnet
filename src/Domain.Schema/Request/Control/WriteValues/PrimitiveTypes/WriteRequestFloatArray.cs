﻿using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestFloatArray : WriteRequestPrimitiveArray<float>
    {
        public WriteRequestFloatArray(IEnumerable<float> items) : base(items) { }
    }
}