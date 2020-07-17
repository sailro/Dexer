/* Dexer Copyright (c) 2010-2019 Sebastien LEBRETON

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
using System.Collections.Generic;
using System.IO;
using Dexer.IO;

namespace Dexer.Core
{
	public class Dex
	{
		public List<ClassDefinition> Classes { get; internal set; }

		internal List<TypeReference> TypeReferences { get; set; }
		internal List<FieldReference> FieldReferences { get; set; }
		internal List<MethodReference> MethodReferences { get; set; }

		internal List<string> Strings { get; set; }
		internal List<Prototype> Prototypes { get; set; }

		internal MemoryStream buffer { get; set; }

		/*
		 * well,a DexReader parse a 8MB dex file will take more than 200MB memory.
		 * but our function here is asking "shall we load the 8MB dex into the memory?", which is so stupid.
		 * I think we should just load all dex into memory,and lazy init some infrequently used data, such as DebugInstructions,
		 * in order to reduce the memory cost.
		 * 
		 * We don’t need to worry if the dex is too big. Full-loaded Dexer Dex object is much more bigger.
		 * And due to the DexIndexOverflow problem, a single dex file won't be too big to load.(it will be multidexed by developer)
		 */

		public static Dex Read(string filename)
		{
			return Read(File.OpenRead(filename));
		}

		public static Dex Read(Stream stream)
		{
			Dex result = new Dex(stream);

			var buffer = result.buffer;
			var binaryReader = new BinaryReader(buffer);
			DexReader dexReader = new DexReader(result);
			dexReader.ReadFrom(binaryReader);

			return result;
		}
		
		public void Write(string filename)
		{
			Write(File.Open(filename, FileMode.Create));
		}

		public void Write(Stream stream)
		{
			using var binaryWriter = new BinaryWriter(stream);
			DexWriter dexWriter = new DexWriter(this);
			dexWriter.WriteTo(binaryWriter);
		}
		

		public Dex(Stream stream)
		{
			Classes = new List<ClassDefinition>();
			TypeReferences = new List<TypeReference>();
			FieldReferences = new List<FieldReference>();
			MethodReferences = new List<MethodReference>();

			Prototypes = new List<Prototype>();
			Strings = new List<string>();
			
			buffer = new MemoryStream((int)stream.Length);
			stream.CopyTo(buffer);
			
			buffer.Position = 0;
		}

		public ClassDefinition GetClass(string fullname)
		{
			return GetClass(fullname, Classes);
		}

		internal ClassDefinition GetClass(string fullname, List<ClassDefinition> container)
		{
			foreach (var item in container)
			{
				if (fullname.Equals(item.Fullname))
					return item;

				var inner = GetClass(fullname, item.InnerClasses);
				if (inner != null)
					return inner;
			}

			return null;
		}

		internal TypeReference Import(TypeReference tref, bool add)
		{
			foreach (var item in TypeReferences)
			{
				if (tref.Equals(item))
				{
					return item;
				}
			}

			if (add)
			{
				// if !add see TypeDescriptor comment 
				TypeReferences.Add(tref);
			}

			return tref;
		}

		public ClassReference Import(ClassReference cref)
		{
			return (ClassReference)Import(cref, true);
		}

		public TypeReference Import(TypeReference tref)
		{
			return Import(tref, true);
		}

		public MethodReference Import(MethodReference mref)
		{
			foreach (var item in MethodReferences)
			{
				if (mref.Equals(item))
				{
					return item;
				}
			}

			MethodReferences.Add(mref);
			return mref;
		}
	}
}
