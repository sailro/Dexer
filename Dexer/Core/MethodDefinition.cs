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

using System.Collections.Generic;
using Dexer.Instructions;

namespace Dexer.Core
{
	public class MethodDefinition : MethodReference, IMemberDefinition
	{
		public AccessFlags AccessFlags { get; set; }

		public new ClassDefinition Owner
		{
			get => base.Owner as ClassDefinition;
			set => base.Owner = value;
		}

		public List<Annotation> Annotations { get; set; }
		public MethodBody Body { get; set; }

		public MethodDefinition()
		{
			Annotations = new List<Annotation>();
		}

		// for prefetching
		internal MethodDefinition(MethodReference mref) : this()
		{
			Owner = mref.Owner as ClassDefinition;
			Name = mref.Name;
			Prototype = mref.Prototype;
		}

		public MethodDefinition(ClassDefinition owner, string name, Prototype prototype) : this()
		{
			Owner = owner;
			Name = name;
			Prototype = prototype;
		}

		public bool IsVirtual
		{
			get;
			set;
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

		public bool IsSynchronized
		{
			get => (AccessFlags & AccessFlags.Synchronized) != 0;
			set => AccessFlags |= AccessFlags.Synchronized;
		}

		public bool IsBridge
		{
			get => (AccessFlags & AccessFlags.Bridge) != 0;
			set => AccessFlags |= AccessFlags.Bridge;
		}

		public bool IsVarArgs
		{
			get => (AccessFlags & AccessFlags.VarArgs) != 0;
			set => AccessFlags |= AccessFlags.VarArgs;
		}

		public bool IsNative
		{
			get => (AccessFlags & AccessFlags.Native) != 0;
			set => AccessFlags |= AccessFlags.Native;
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

		public bool IsStrictFp
		{
			get => (AccessFlags & AccessFlags.StrictFp) != 0;
			set => AccessFlags |= AccessFlags.StrictFp;
		}

		public bool IsConstructor
		{
			get => (AccessFlags & AccessFlags.Constructor) != 0;
			set => AccessFlags |= AccessFlags.Constructor;
		}

		public bool IsDeclaredSynchronized
		{
			get => (AccessFlags & AccessFlags.DeclaredSynchronized) != 0;
			set => AccessFlags |= AccessFlags.DeclaredSynchronized;
		}
		// ReSharper restore ValueParameterNotUsed

		public bool Equals(MethodDefinition other)
		{
			// Should be enough (ownership)
			return base.Equals(other);
		}

		public override bool Equals(IMemberReference other)
		{
			return other is MethodDefinition definition && Equals(definition);
		}
	}
}
