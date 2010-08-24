using Dexer.Metadata;
using System.Collections.Generic;
using System;

namespace Dexer.Core
{
    public abstract class TypeReference : IEquatable<TypeReference>
    {
        internal TypeDescriptors TypeDescriptor { get; set; }

        public abstract bool Equals(TypeReference other);
    }
}
