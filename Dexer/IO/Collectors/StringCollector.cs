/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using System;
using System.Collections;
using System.Collections.Generic;
using Dexer.Core;
using Dexer.Instructions;
using Dexer.Metadata;

namespace Dexer.IO.Collector
{
    internal class StringCollector : BaseCollector<String>
    {

        public override void Collect(DebugInfo debugInfo)
        {
            base.Collect(debugInfo);

            if (debugInfo != null && debugInfo.Owner != null && debugInfo.Owner.Owner != null && !debugInfo.Owner.Owner.IsStatic)
                Collect("this");
        }

        public override void Collect(Prototype prototype)
        {
            base.Collect(prototype);

            // Shorty descriptor
            Collect(TypeDescriptor.Encode(prototype));
        }

        public override void Collect(ArrayType array)
        {
            // Do not 'over' collect String descriptors by iterating over array.ElementType
            Collect(array as TypeReference);
        }

        public override void Collect(TypeReference tref)
        {
            base.Collect(tref);
            Collect(TypeDescriptor.Encode(tref));
        }

        public override void Collect(string str)
        {
            base.Collect(str);

            if (str != null)
            {
                if (!Items.ContainsKey(str))
                    Items[str] = 0;

                Items[str]++;
            }
        }

    }
}
