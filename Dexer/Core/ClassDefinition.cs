﻿/* Dexer Copyright (c) 2010-2019 Sebastien LEBRETON

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

using System.Collections.Generic;
using System.Globalization;
using Dexer.Metadata;
using System.Linq;
using Dexer.IO;
using Dexer.IO.Collectors;

namespace Dexer.Core
{
	public class ClassDefinition : ClassReference, IMemberDefinition
	{
		public AccessFlags AccessFlags { get; set; }
		public ClassReference SuperClass { get; set; }
		public List<ClassDefinition> InnerClasses { get; set; }
		public List<ClassReference> Interfaces { get; set; }
		public string SourceFile { get; set; }
		public List<Annotation> Annotations { get; set; }
		public List<FieldDefinition> Fields { get; set; }
		public List<MethodDefinition> Methods { get; set; }
		public ClassDefinition Owner { get; set; }

		internal ClassDefinition()
		{
			TypeDescriptor = TypeDescriptors.FullyQualifiedName;

			Interfaces = new List<ClassReference>();
			Annotations = new List<Annotation>();
			Fields = new List<FieldDefinition>();
			Methods = new List<MethodDefinition>();
			InnerClasses = new List<ClassDefinition>();
		}

		internal ClassDefinition(ClassReference cref) : this()
		{
			Fullname = cref.Fullname;
			Namespace = cref.Namespace;
			Name = cref.Name;
		}

		public IEnumerable<MethodDefinition> GetMethods(string name)
		{
			return Methods.Where(mdef => mdef.Name == name);
		}

		public MethodDefinition GetMethod(string name)
		{
			return GetMethods(name).FirstOrDefault();
		}

		public FieldDefinition GetField(string name)
		{
			return Fields.FirstOrDefault(fdef => fdef.Name == name);
		}

		// ReSharper disable ValueParameterNotUsed
		public bool IsPublic
		{
			get => (AccessFlags & AccessFlags.Public) != 0;
			set => AccessFlags |= AccessFlags.Public;
		}

		public bool IsPrivate
		{
			get => (AccessFlags & AccessFlags.Private) != 0;
			set => AccessFlags |= AccessFlags.Private;
		}

		public bool IsProtected
		{
			get => (AccessFlags & AccessFlags.Protected) != 0;
			set => AccessFlags |= AccessFlags.Protected;
		}

		public bool IsStatic
		{
			get => (AccessFlags & AccessFlags.Static) != 0;
			set => AccessFlags |= AccessFlags.Static;
		}

		public bool IsFinal
		{
			get => (AccessFlags & AccessFlags.Final) != 0;
			set => AccessFlags |= AccessFlags.Final;
		}

		public bool IsInterface
		{
			get => (AccessFlags & AccessFlags.Interface) != 0;
			set => AccessFlags |= AccessFlags.Interface;
		}

		public bool IsAbstract
		{
			get => (AccessFlags & AccessFlags.Abstract) != 0;
			set => AccessFlags |= AccessFlags.Abstract;
		}

		public bool IsSynthetic
		{
			get => (AccessFlags & AccessFlags.Synthetic) != 0;
			set => AccessFlags |= AccessFlags.Synthetic;
		}

		public bool IsAnnotation
		{
			get => (AccessFlags & AccessFlags.Annotation) != 0;
			set => AccessFlags |= AccessFlags.Annotation;
		}

		public bool IsEnum
		{
			get => (AccessFlags & AccessFlags.Enum) != 0;
			set => AccessFlags |= AccessFlags.Enum;
		}
		// ReSharper restore ValueParameterNotUsed

		public bool Equals(ClassDefinition other)
		{
			// Should be enough (ownership)
			return base.Equals(other);
		}

		public override bool Equals(TypeReference other)
		{
			return other is ClassDefinition definition && Equals(definition);
		}

		internal static List<ClassDefinition> Flattenize(List<ClassDefinition> container)
		{
			var result = new List<ClassDefinition>();
			foreach (var cdef in container)
			{
				result.AddRange(Flattenize(cdef.InnerClasses));
				result.Add(cdef);
			}

			return result;
		}

		internal static List<ClassDefinition> SortAndFlattenize(List<ClassDefinition> container)
		{
			var tsorter = new TopologicalSorter();
			container.Sort(new ClassDefinitionComparer());
			container = new List<ClassDefinition>(tsorter.TopologicalSort(container, new ClassDefinitionComparer()));

			return Flattenize(container);
		}

		internal static List<ClassDefinition> Hierarchicalize(List<ClassDefinition> container, Dex dex)
		{
			var result = new List<ClassDefinition>();
			foreach (var cdef in container)
			{
				if (cdef.Fullname.Contains(DexConsts.InnerClassMarker.ToString(CultureInfo.InvariantCulture)))
				{
					var items = cdef.Fullname.Split(DexConsts.InnerClassMarker);
					var fullname = items[0];
					//var name = items[1];
					var owner = dex.GetClass(fullname);
					if (owner == null)
						continue;

					owner.InnerClasses.Add(cdef);
					cdef.Owner = owner;
				}
				else
				{
					result.Add(cdef);
				}
			}

			return result;
		}
	}
}
