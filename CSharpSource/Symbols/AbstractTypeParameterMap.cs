﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    internal abstract class AbstractTypeParameterMap : AbstractTypeMap
    {
        protected readonly SmallDictionary<TypeParameterSymbol, TypeSymbol> Mapping;

        protected AbstractTypeParameterMap(SmallDictionary<TypeParameterSymbol, TypeSymbol> mapping)
        {
            this.Mapping = mapping;
        }

        protected sealed override TypeSymbol SubstituteTypeParameter(TypeParameterSymbol typeParameter)
        {
            // It might need to be substituted directly.
            TypeSymbol result;
            if (Mapping.TryGetValue(typeParameter, out result))
            {
                return result;
            }

            return typeParameter;
        }

        private string GetDebuggerDisplay()
        {
            var result = new StringBuilder("[");
            result.Append(this.GetType().Name);
            foreach (var kv in Mapping)
            {
                result.Append(" ").Append(kv.Key).Append(":").Append(kv.Value);
            }

            return result.Append("]").ToString();
        }
    }
}
