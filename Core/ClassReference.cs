/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Text;
using Dexer.Metadata;

namespace Dexer.Core
{
    public class ClassReference : TypeReference
    {
        const char NamespaceSeparator = '.';
        const char InternalNamespaceSeparator = '/';

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Fullname
        {
            get
            {
                StringBuilder result = new StringBuilder(Namespace);
                if (result.Length > 0)
                    result.Append(NamespaceSeparator);
                result.Append(Name);
                return result.ToString();
            }
            set
            {
                value = value.Replace(InternalNamespaceSeparator, NamespaceSeparator);
                string[] items = value.Split(NamespaceSeparator);
                if (items.Length > 0)
                {
                    Name = items[items.Length - 1];
                    Array.Resize(ref items, items.Length - 1);
                    Namespace = string.Join(NamespaceSeparator.ToString(), items);
                }
                else
                {
                    Name = string.Empty;
                    Namespace = string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return Fullname;
        }

        public ClassReference()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;
        }

        public ClassReference(string fullname) : this()
        {
            Fullname = fullname;
        }

        public override bool Equals(TypeReference other)
        {
            return (other is ClassReference) && (this.Fullname == (other as ClassReference).Fullname);
        }
    }
}
