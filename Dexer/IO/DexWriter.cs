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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.IO.Collectors;
using Dexer.Instructions;
using Dexer.IO.Markers;
using Dexer.Metadata;
using StringComparer = Dexer.IO.Collectors.StringComparer;

namespace Dexer.IO
{
    internal class DexWriter
    {
        private Dex Dex { get; set; }
        internal Map Map { get; set; }
        private List<ClassDefinition> FlatClasses { get; set; }
        
        private HeaderMarkers HeaderMarkers { get; set; }
        private List<UIntMarker> StringMarkers { get; set; }
        private List<UIntMarker> PrototypeTypeListMarkers { get; set; }
        private List<ClassDefinitionMarkers> ClassDefinitionsMarkers { get; set; }
        private Dictionary<Parameter, UIntMarker> AnnotationSetRefListMarkers { get; set; }
        private Dictionary<Annotation, UIntMarker> AnnotationSetMarkers { get; set; }
        private Dictionary<MethodDefinition, UIntMarker> DebugMarkers { get; set; }

        private Dictionary<string, uint> TypeLists { get; set; }
        private Dictionary<AnnotationSet, uint> AnnotationSets { get; set; }
        private Dictionary<MethodDefinition, uint> AnnotationSetRefLists { get; set; }
        private Dictionary<MethodDefinition, uint> Codes { get; set; }

        internal Dictionary<string, int> StringLookup { get; set; }
        internal Dictionary<Prototype, int> PrototypeLookup { get; set; }
        internal Dictionary<TypeReference, int> TypeLookup { get; set; }
        internal Dictionary<MethodReference, int> MethodLookup { get; set; }
        internal Dictionary<FieldReference, int> FieldLookup { get; set; }

        internal byte[] Signature { get; set; }
        internal uint CheckSum { get; set; }

        public DexWriter(Dex dex)
        {
            Dex = dex;
            Map = new Map();

            HeaderMarkers = new HeaderMarkers();
            StringMarkers = new List<UIntMarker>();
            PrototypeTypeListMarkers = new List<UIntMarker>();
            ClassDefinitionsMarkers = new List<ClassDefinitionMarkers>();
            AnnotationSetRefListMarkers = new Dictionary<Parameter, UIntMarker>();
            AnnotationSetMarkers = new Dictionary<Annotation, UIntMarker>();
            DebugMarkers = new Dictionary<MethodDefinition, UIntMarker>();

            TypeLists = new Dictionary<string, uint>();
            AnnotationSets = new Dictionary<AnnotationSet, uint>();
            AnnotationSetRefLists = new Dictionary<MethodDefinition, uint>();
            Codes = new Dictionary<MethodDefinition, uint>();
        }

        #region " Header "
        private void WriteHeader(BinaryWriter writer)
        {
            var sectionOffset = (uint) writer.BaseStream.Position;
            Map.Add(TypeCodes.Header, new MapItem(TypeCodes.Header, 1, sectionOffset));

            writer.EnsureAlignment(sectionOffset, 4, () =>
            {
                writer.Write(DexConsts.FileMagic);
                HeaderMarkers.CheckSumMarker = writer.MarkUInt();
                HeaderMarkers.SignatureMarker = writer.MarkSignature();
                HeaderMarkers.FileSizeMarker = writer.MarkUInt();
                writer.Write(DexConsts.HeaderSize);
                writer.Write(DexConsts.Endian);
                HeaderMarkers.LinkMarker = writer.MarkSizeOffset();
                HeaderMarkers.MapMarker = writer.MarkUInt();

                HeaderMarkers.StringsMarker = writer.MarkSizeOffset();
                HeaderMarkers.TypeReferencesMarker = writer.MarkSizeOffset();
                HeaderMarkers.PrototypesMarker = writer.MarkSizeOffset();
                HeaderMarkers.FieldReferencesMarker = writer.MarkSizeOffset();
                HeaderMarkers.MethodReferencesMarker = writer.MarkSizeOffset();
                HeaderMarkers.ClassDefinitionsMarker = writer.MarkSizeOffset();
                HeaderMarkers.DataMarker = writer.MarkSizeOffset();
            });
        }
        #endregion

        #region " StringId "
        private Dictionary<string, int> CollectStrings()
        {
            var collector = new StringCollector();
            collector.Collect(Dex);
            collector.Collect(Dex.MethodReferences);
            collector.Collect(Dex.FieldReferences);
            collector.Collect(Dex.TypeReferences);

            var strings = new List<string>(collector.Items.Keys);
            strings.Sort(new StringComparer());
            Dex.Strings = strings;

            var lookup = new Dictionary<string, int>();
            for (var i = 0; i < strings.Count; i++)
                lookup.Add(strings[i], i);

            return lookup;
        }
        
        private void WriteStringId(BinaryWriter writer)
        {
            if (Dex.Strings.Count > 0)
            {
                HeaderMarkers.StringsMarker.Value = new SizeOffset((uint)Dex.Strings.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.StringId, new MapItem(TypeCodes.StringId, (uint)Dex.Strings.Count, (uint)writer.BaseStream.Position));
            }

            for (var i = 0; i < Dex.Strings.Count; i++)
                StringMarkers.Add(writer.MarkUInt());
        }
        #endregion

        #region " StringData "
        private void WriteStringData(BinaryWriter writer)
        {
            if (Dex.Strings.Count > 0)
                Map.Add(TypeCodes.StringData, new MapItem(TypeCodes.StringData, (uint)Dex.Strings.Count, (uint)writer.BaseStream.Position));

            for (var i = 0; i < Dex.Strings.Count; i++)
            {
                StringMarkers[i].Value = (uint)writer.BaseStream.Position;
                writer.WriteMUTF8String(Dex.Strings[i]);
            }
        }
        #endregion

        #region " PrototypeId " 
        private Dictionary<Prototype, int> CollectPrototypes()
        {
            var collector = new PrototypeCollector();
            collector.Collect(Dex);
            collector.Collect(Dex.MethodReferences);
            collector.Collect(Dex.FieldReferences);
            collector.Collect(Dex.TypeReferences);

            Dex.Prototypes = collector.ToList(new PrototypeComparer());

            var lookup = new Dictionary<Prototype, int>();
            for (var i = 0; i < Dex.Prototypes.Count; i++)
                lookup.Add(Dex.Prototypes[i], i);
            
            return lookup;
        }

        private void WritePrototypeId(BinaryWriter writer)
        {
            if (Dex.Prototypes.Count > 0)
            {
                HeaderMarkers.PrototypesMarker.Value = new SizeOffset((uint)Dex.Prototypes.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.ProtoId, new MapItem(TypeCodes.ProtoId, (uint)Dex.Prototypes.Count, (uint)writer.BaseStream.Position));
            }

            foreach (var prototype in Dex.Prototypes)
            {
                writer.Write((uint)StringLookup[TypeDescriptor.Encode(prototype)]);
                writer.Write((uint)Dex.TypeReferences.IndexOf(prototype.ReturnType));
                PrototypeTypeListMarkers.Add(writer.MarkUInt());
            }
        }
        #endregion

        #region " TypeList "
        private void WriteTypeList(BinaryWriter writer, uint sectionOffset, ushort[] typelist, UIntMarker marker)
        {
	        if (typelist.Length <= 0) 
				return;
	        
			var key = GetTypeListAsString(typelist);
	        if (!TypeLists.ContainsKey(key))
	        {

		        writer.EnsureAlignment(sectionOffset, 4, () =>
			    {
				    TypeLists.Add(key, (uint)writer.BaseStream.Position);

				    writer.Write((uint)typelist.Length);

				    foreach (var item in typelist)
					    writer.Write(item);
			    });
	        }
	        if (marker != null)
		        marker.Value = TypeLists[key];
        }

        private void WriteTypeLists(BinaryWriter writer)
        {
                var offset = (uint) writer.BaseStream.Position;

                for (var i = 0; i < FlatClasses.Count; i++)
                    WriteTypeList(writer, offset, ComputeTypeList(FlatClasses[i].Interfaces),
                                ClassDefinitionsMarkers[i].InterfacesMarker);

                for (var i = 0; i < Dex.Prototypes.Count; i++)
                    WriteTypeList(writer, offset, ComputeTypeList(Dex.Prototypes[i].Parameters),
                                PrototypeTypeListMarkers[i]);

                if (TypeLists.Count > 0)
                    Map.Add(TypeCodes.TypeList,
                            new MapItem(TypeCodes.TypeList, (uint) TypeLists.Count, offset));

        }

        private static string GetTypeListAsString(IEnumerable<ushort> typelist)
        {
            var builder = new StringBuilder();
            foreach (var item in typelist)
                builder.AppendLine(item.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private ushort[] ComputeTypeList(IEnumerable<Parameter> parameters)
        {
	        return parameters.Select(parameter => (ushort) TypeLookup[parameter.Type]).ToArray();
        }

        private ushort[] ComputeTypeList(IEnumerable<ClassReference> classes)
        {
	        return classes.Select(@class => (ushort) TypeLookup[@class]).ToArray();
        }
        #endregion

        #region " FieldId "
        private void WriteFieldId(BinaryWriter writer)
        {
            if (Dex.FieldReferences.Count > 0)
            {
                HeaderMarkers.FieldReferencesMarker.Value = new SizeOffset((uint)Dex.FieldReferences.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.FieldId, new MapItem(TypeCodes.FieldId, (uint)Dex.FieldReferences.Count, (uint)writer.BaseStream.Position));
            }

            foreach (var field in Dex.FieldReferences)
            {
                writer.Write((ushort)TypeLookup[field.Owner]);
                writer.Write((ushort)TypeLookup[field.Type]);
                writer.Write((uint)StringLookup[field.Name]);
            }
        }
        #endregion

        #region " MethodId "
        private void WriteMethodId(BinaryWriter writer)
        {
            if (Dex.MethodReferences.Count > 0)
            {
                HeaderMarkers.MethodReferencesMarker.Value = new SizeOffset((uint)Dex.MethodReferences.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.MethodId, new MapItem(TypeCodes.MethodId, (uint)Dex.MethodReferences.Count, (uint)writer.BaseStream.Position));
            }

            foreach (var method in Dex.MethodReferences)
            {
                writer.Write((ushort)TypeLookup[method.Owner]);
                writer.Write((ushort)PrototypeLookup[method.Prototype]);
                writer.Write((uint)StringLookup[method.Name]);
            }
        }
        #endregion

        #region " TypeId "
        private void WriteTypeId(BinaryWriter writer)
        {
            if (Dex.TypeReferences.Count > 0)
            {
                HeaderMarkers.TypeReferencesMarker.Value = new SizeOffset((uint)Dex.TypeReferences.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.TypeId, new MapItem(TypeCodes.TypeId, (uint)Dex.TypeReferences.Count, (uint)writer.BaseStream.Position));
            }

            foreach (var tref in Dex.TypeReferences)
                writer.Write((uint)StringLookup[TypeDescriptor.Encode(tref)]);
        }
        #endregion

        #region " ClassDef "
        private void WriteClassDef(BinaryWriter writer)
        {
            if (FlatClasses.Count > 0)
            {
                HeaderMarkers.ClassDefinitionsMarker.Value = new SizeOffset((uint)FlatClasses.Count, (uint)writer.BaseStream.Position);
                Map.Add(TypeCodes.ClassDef, new MapItem(TypeCodes.ClassDef, (uint)FlatClasses.Count, (uint)writer.BaseStream.Position));
            }

            var sectionOffset = (uint) writer.BaseStream.Position;
            foreach (var flatclass in FlatClasses)
            {
				var @class = flatclass; 
				writer.EnsureAlignment(sectionOffset, 4, () =>
	            {
                    var markers = new ClassDefinitionMarkers();
                    ClassDefinitionsMarkers.Add(markers);

                    writer.Write(TypeLookup[@class]);
                    writer.Write((uint)@class.AccessFlags);
                    writer.Write(@class.SuperClass == null ? DexConsts.NoIndex : (uint)TypeLookup[@class.SuperClass]);
                    markers.InterfacesMarker = writer.MarkUInt();
                    writer.Write(String.IsNullOrEmpty(@class.SourceFile) ? DexConsts.NoIndex : (uint)StringLookup[@class.SourceFile]);
                    markers.AnnotationsMarker = writer.MarkUInt();
                    markers.ClassDataMarker = writer.MarkUInt();
                    markers.StaticValuesMarker = writer.MarkUInt();
                });
            }
        }
        #endregion

        #region " AnnotationSetRefList "
        private void WriteAnnotationSetRefList(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;

            foreach (var flatclass in FlatClasses)
            {
	            var @class = flatclass;
                writer.EnsureAlignment(sectionOffset, 4, () =>
                {
                    foreach (var mdef in @class.Methods.Where(mdef => (mdef.Prototype.ContainsAnnotation())))
                    {
	                    AnnotationSetRefLists.Add(mdef, (uint)writer.BaseStream.Position);
	                    writer.Write(mdef.Prototype.Parameters.Count);
	                    foreach (var parameter in mdef.Prototype.Parameters)
		                    AnnotationSetRefListMarkers.Add(parameter, writer.MarkUInt());
                    }
                });
            }

            if (AnnotationSetRefLists.Count > 0)
                Map.Add(TypeCodes.AnnotationSetRefList, new MapItem(TypeCodes.AnnotationSetRefList, (uint)AnnotationSetRefLists.Count, sectionOffset));
        }
        #endregion

        #region " AnnotationSet "
        private void WriteAnnotationSet(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;

            foreach (var @class in FlatClasses)
            {
                WriteAnnotationSet(writer, sectionOffset, @class, false);

                foreach (FieldDefinition field in @class.Fields)
                    WriteAnnotationSet(writer, sectionOffset, field, false);

                foreach (MethodDefinition method in @class.Methods)
                    WriteAnnotationSet(writer, sectionOffset, method, false);

            }

            foreach (var @class in FlatClasses)
            {
                foreach (var method in @class.Methods)
                    if (method.Prototype.ContainsAnnotation())
                        foreach (var parameter in method.Prototype.Parameters)
                            AnnotationSetRefListMarkers[parameter].Value = WriteAnnotationSet(writer, sectionOffset, parameter, true);
            }

            if (AnnotationSets.Count > 0)
                Map.Add(TypeCodes.AnnotationSet, new MapItem(TypeCodes.AnnotationSet, (uint)AnnotationSets.Count, sectionOffset));
        }

        private uint WriteAnnotationSet(BinaryWriter writer, uint sectionOffset, IAnnotationProvider provider, bool writezero)
        {
            var key = new AnnotationSet(provider);
            uint offset = 0;

            if (!AnnotationSets.ContainsKey(key))
            {
                writer.EnsureAlignment(sectionOffset, 4, () =>
                {
                    offset = (uint)writer.BaseStream.Position;

                    if (provider.Annotations.Count > 0 || writezero)
                        writer.Write(provider.Annotations.Count);

                    foreach (var annotation in provider.Annotations)
                        if (AnnotationSetMarkers.ContainsKey(annotation))
                            AnnotationSetMarkers[annotation].CloneMarker();
                        else
                            AnnotationSetMarkers.Add(annotation, writer.MarkUInt());

                    if (provider.Annotations.Count > 0 || writezero)
                        AnnotationSets.Add(key, offset);
                    else
                        offset = DexConsts.NoIndex;
                });
            }
            else
                offset = AnnotationSets[key];

            return offset;
        }
        #endregion

        #region " Annotations "
        private void WriteAnnotation(BinaryWriter writer, Annotation annotation)
        {
            writer.Write((byte)annotation.Visibility);
            WriteEncodedAnnotation(writer, annotation);
        }

        private void WriteEncodedAnnotation(BinaryWriter writer, Annotation annotation)
        {
            writer.WriteULEB128((uint)TypeLookup[annotation.Type]);
            writer.WriteULEB128((uint)annotation.Arguments.Count);

            foreach (var argument in annotation.Arguments)
            {
                writer.WriteULEB128((uint)StringLookup[argument.Name]);
                WriteValue(writer, argument.Value);
            }
        }

        private void WriteValue(BinaryWriter writer, object value)
        {
            int valueArgument = 0;

            var format = ValueFormat.GetFormat(value);
            switch (format)
            {
                case ValueFormats.Char:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(Convert.ToInt64(value)) - 1;
                    break;
                case ValueFormats.Byte:
                case ValueFormats.Short:
                case ValueFormats.Int:
                case ValueFormats.Long:
                    valueArgument = writer.GetByteCountForSignedPackedNumber(Convert.ToInt64(value)) - 1;
                    break;
                case ValueFormats.Float:
                    valueArgument = writer.GetByteCountForSignedPackedNumber(BitConverter.ToInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0)) - 1;
                    break;
                case ValueFormats.Double:
                    valueArgument = writer.GetByteCountForSignedPackedNumber(BitConverter.DoubleToInt64Bits(Convert.ToDouble(value))) - 1;
                    break;
                case ValueFormats.String:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(StringLookup[(String)value]) - 1;
                    break;
                case ValueFormats.Type:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(TypeLookup[(TypeReference)value]) - 1;
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(FieldLookup[(FieldReference)value]) - 1;
                    break;
                case ValueFormats.Method:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(MethodLookup[(MethodReference)value]) - 1;
                    break;
                case ValueFormats.Boolean:
                    valueArgument = Convert.ToInt32(Convert.ToBoolean(value));
                    break;
            }

            var data = (byte)(valueArgument << 5);
            data |= (byte)format;
            writer.Write(data);

            switch (format)
            {
                case ValueFormats.Char:
                    writer.WriteUnsignedPackedNumber(Convert.ToInt64(value));
                    break;
                case ValueFormats.Short:
                case ValueFormats.Byte:
                case ValueFormats.Int:
                case ValueFormats.Long:
                    writer.WritePackedSignedNumber(Convert.ToInt64(value));
                    break;
                case ValueFormats.Float:
                    writer.WritePackedSignedNumber(BitConverter.ToInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0));
                    break;
                case ValueFormats.Double:
                    writer.WritePackedSignedNumber(BitConverter.DoubleToInt64Bits(Convert.ToDouble(value)));
                    break;
                case ValueFormats.String:
                    writer.WriteUnsignedPackedNumber(StringLookup[(String)value]);
                    break;
                case ValueFormats.Type:
                    writer.WriteUnsignedPackedNumber(TypeLookup[(TypeReference)value]);
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    writer.WriteUnsignedPackedNumber(FieldLookup[(FieldReference)value]);
                    break;
                case ValueFormats.Method:
                    writer.WriteUnsignedPackedNumber(MethodLookup[(MethodReference)value]);
                    break;
                case ValueFormats.Array:
                    WriteValues(writer, (object[])value);
                    break;
                case ValueFormats.Annotation:
                    WriteEncodedAnnotation(writer, value as Annotation);
                    break;
                case ValueFormats.Null:
                case ValueFormats.Boolean:
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void WriteValues(BinaryWriter writer, ICollection<object> values)
        {
            writer.WriteULEB128((uint)values.Count);
            foreach (var value in values)
                WriteValue(writer, value);
        }

        private void WriteAnnotations(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;
            var annotations = new Dictionary<Annotation, uint>();

            foreach (var annotationset in AnnotationSets.Keys.ToList())
            {
                foreach (var annotation in annotationset)
                {
	                if (annotations.ContainsKey(annotation)) 
						continue;
	                
					var annoffset = (uint)writer.BaseStream.Position;
	                WriteAnnotation(writer, annotation);
	                annotations.Add(annotation, annoffset);

	                AnnotationSetMarkers[annotation].Value = annoffset;
                }
            }

            if (annotations.Count > 0)
                Map.Add(TypeCodes.Annotation, new MapItem(TypeCodes.Annotation, (uint) annotations.Count, offset));
        }
        #endregion

        #region " Collect and sort "
        public Dictionary<T, int> Collect<T>(List<T> container, IComparer<T> comparer)
        {
            var result = new Dictionary<T, int>();
            container.Sort(comparer);

            for (var i = 0; i < container.Count; i++)
                result.Add(container[i], i);

            return result;
        }

        public Dictionary<TypeReference, int> CollectTypes()
        {
            return Collect(Dex.TypeReferences, new TypeReferenceComparer());
        }

        public Dictionary<MethodReference, int> CollectMethods()
        {
            return Collect(Dex.MethodReferences, new MethodReferenceComparer());
        }

        public Dictionary<FieldReference, int> CollectFields()
        {
            return Collect(Dex.FieldReferences, new FieldReferenceComparer());
        }
        #endregion

        #region " AnnotationsDirectory "
        private void WriteAnnotationsDirectory(BinaryWriter writer)
        {
            var sectionOffset = (uint) writer.BaseStream.Position;
            uint count = 0;

            var classAnnotationSets = new Dictionary<AnnotationSet, uint>();

            for (var index = 0; index < FlatClasses.Count; index++ )
            {
				var i = index; 
				var @class = FlatClasses[i];

                writer.EnsureAlignment(sectionOffset, 4, () =>
                {
	                var annotatedMethods = new List<MethodDefinition>();										// by method index
                    var annotatedParametersList = new List<MethodDefinition>();									// by method index
	                var annotatedFields = @class.Fields.Where(field => field.Annotations.Count > 0).ToList();	// by field index

	                foreach (var method in @class.Methods)
                    {
                        if (method.Annotations.Count > 0)
                            annotatedMethods.Add(method);
                        if (method.Prototype.ContainsAnnotation())
                            annotatedParametersList.Add(method);
                    }

                    var total = @class.Annotations.Count + annotatedFields.Count + annotatedMethods.Count + annotatedParametersList.Count;
	                if (total <= 0) 
						return;
	                
					// all datas except class annotations are specific.
	                if (total == @class.Annotations.Count)
	                {
		                var set = new AnnotationSet(@class);
		                if (classAnnotationSets.ContainsKey(set))
		                {
			                ClassDefinitionsMarkers[i].AnnotationsMarker.Value = classAnnotationSets[set];
			                return;
		                }

						classAnnotationSets.Add(set, (uint)writer.BaseStream.Position);
	                }

	                ClassDefinitionsMarkers[i].AnnotationsMarker.Value = (uint)writer.BaseStream.Position;
	                count++;

	                if (@class.Annotations.Count > 0)
		                writer.Write(AnnotationSets[new AnnotationSet(@class)]);
	                else
		                writer.Write((uint)0);

	                writer.Write(annotatedFields.Count);
	                writer.Write(annotatedMethods.Count);
	                writer.Write(annotatedParametersList.Count);

	                var fields = new List<FieldReference>(annotatedFields.Cast<FieldReference>());
	                fields.Sort(new FieldReferenceComparer());
	                foreach (FieldDefinition field in fields)
	                {
		                writer.Write(FieldLookup[field]);
		                writer.Write(AnnotationSets[new AnnotationSet(field)]);
	                }

	                var methods = new List<MethodReference>(annotatedMethods.Cast<MethodReference>());
	                methods.Sort(new MethodReferenceComparer());
	                foreach (MethodDefinition method in methods)
	                {
		                writer.Write(MethodLookup[method]);
		                writer.Write(AnnotationSets[new AnnotationSet(method)]);
	                }

	                methods = new List<MethodReference>(annotatedParametersList.Cast<MethodReference>());
	                methods.Sort(new MethodReferenceComparer());
	                foreach (MethodDefinition method in methods)
	                {
		                writer.Write(MethodLookup[method]);
		                writer.Write(AnnotationSetRefLists[method]);
	                }
                });
            }

            if (count > 0)
                Map.Add(TypeCodes.AnnotationDirectory, new MapItem(TypeCodes.AnnotationDirectory, count, sectionOffset));
        }
        #endregion

        #region " Code "
        private void WriteCode(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;
            uint count = 0;

            foreach (var loopmethod in FlatClasses.SelectMany(@class => @class.Methods))
            {
	            var method = loopmethod;

	            Codes.Add(method, 0);
	            var body = method.Body;
	            if (body != null)
	            {
		            writer.EnsureAlignment(sectionOffset, 4, () => 
			            {
				            Codes[method] = (uint)writer.BaseStream.Position;
				            count++;

				            writer.Write((ushort)body.Registers.Count);
				            writer.Write(body.IncomingArguments);
				            writer.Write(body.OutgoingArguments);
				            writer.Write((ushort)body.Exceptions.Count);
				            DebugMarkers.Add(method, writer.MarkUInt());

				            var iwriter = new InstructionWriter(this, method);
				            iwriter.WriteTo(writer);

				            if ((body.Exceptions.Count != 0) && (iwriter.Codes.Length % 2 != 0))
					            writer.Write((ushort)0); // padding (tries 4-byte alignment)

				            var catchHandlers = new Dictionary<CatchSet, List<ExceptionHandler>>();
				            var exceptionsMarkers = new Dictionary<ExceptionHandler, UShortMarker>();
				            foreach (var handler in body.Exceptions)
				            {
					            writer.Write(handler.TryStart.Offset);
					            writer.Write((ushort)(iwriter.LookupLast[handler.TryEnd] - handler.TryStart.Offset + 1));
					            exceptionsMarkers.Add(handler, writer.MarkUShort());

					            var set = new CatchSet(handler);
					            if (!catchHandlers.ContainsKey(set))
						            catchHandlers.Add(set, new List<ExceptionHandler>());

					            catchHandlers[set].Add(handler);
				            }

				            var catchSets = catchHandlers.Keys.ToList();
				            catchSets.Sort(new CatchSetComparer());

				            if (catchSets.Count <= 0) return;
				            var baseOffset = writer.BaseStream.Position;
				            writer.WriteULEB128((uint)catchSets.Count);
				            foreach (var set in catchSets)
				            {
					            var itemoffset = writer.BaseStream.Position - baseOffset;

					            if (set.CatchAll != null)
						            writer.WriteSLEB128(-set.Count);
					            else
						            writer.WriteSLEB128(set.Count);

					            foreach (var handler in catchHandlers[set])
						            exceptionsMarkers[handler].Value = (ushort)itemoffset;

					            foreach (var @catch in set)
					            {
						            writer.WriteULEB128((uint)TypeLookup[@catch.Type]);
						            writer.WriteULEB128((uint)@catch.Instruction.Offset);
					            }

					            if (set.CatchAll != null)
						            writer.WriteULEB128((uint)set.CatchAll.Offset);
				            }
			            });
	            }
            }

            if (Codes.Count > 0)
                Map.Add(TypeCodes.Code, new MapItem(TypeCodes.Code, count, sectionOffset));
        }
        #endregion

        #region " DebugInfo "
        private static void CheckOperand(DebugInstruction ins, int operandCount, params Type[] types)
        {
            if (ins.Operands.Count != operandCount)
                throw new DebugInstructionException(ins, string.Format("Expecting {0} operands", operandCount));

            for (int i = 0; i < ins.Operands.Count; i++)
            {
                try
                {
                    if (types[i].IsInstanceOfType(ins.Operands[i]))
                        continue;
					// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                    Convert.ChangeType(ins.Operands[i], types[i]);
					// ReSharper restore ReturnValueOfPureMethodIsNotUsed
                }
                catch (Exception)
                {
                    throw new DebugInstructionException(ins, string.Format("Expecting '{0}' Type (or compatible) for Operands[{1}]", types[i], i));
                }
            }
        }

        private void WriteDebugInfo(BinaryWriter writer)
        {
            var offset = (uint) writer.BaseStream.Position;
            uint count = 0;

            foreach (var @class in FlatClasses)
            {
                foreach (var method in @class.Methods)
                {
                    var body = method.Body;
	                if (body == null || body.DebugInfo == null)
						continue;
	                
					DebugMarkers[method].Value = (uint)writer.BaseStream.Position;
	                count++;

	                // byte aligned
	                var debugInfo = body.DebugInfo;
	                writer.WriteULEB128(debugInfo.LineStart);

	                if (debugInfo.Parameters.Count != method.Prototype.Parameters.Count)
		                throw new MalformedException("Unexpected parameter count in DebugInfo, must match with prototype");

	                writer.WriteULEB128((uint)debugInfo.Parameters.Count);
	                foreach (var parameter in debugInfo.Parameters)
	                {
		                if (string.IsNullOrEmpty(parameter))
			                writer.WriteULEB128P1(DexConsts.NoIndex);
		                else
			                writer.WriteULEB128P1(StringLookup[parameter]);
	                }

	                foreach (var ins in debugInfo.DebugInstructions)
	                {
		                String name;

		                writer.Write((byte)ins.OpCode);
		                switch (ins.OpCode)
		                {
			                case DebugOpCodes.AdvancePc:
				                // uleb128 addr_diff
				                CheckOperand(ins, 1, typeof(uint));
				                writer.WriteULEB128(Convert.ToUInt32(ins.Operands[0]));
				                break;
			                case DebugOpCodes.AdvanceLine:
				                // sleb128 line_diff
				                CheckOperand(ins, 1, typeof(int));
				                writer.WriteSLEB128(Convert.ToInt32(ins.Operands[0]));
				                break;
			                case DebugOpCodes.EndLocal:
			                case DebugOpCodes.RestartLocal:
				                // uleb128 register_num
				                CheckOperand(ins, 1, typeof(Register));
				                writer.WriteULEB128((uint)((Register)ins.Operands[0]).Index);
				                break;
			                case DebugOpCodes.SetFile:
				                // uleb128p1 name_idx
				                CheckOperand(ins, 1, typeof(String));
				                name = (String)ins.Operands[0];
				                if (string.IsNullOrEmpty(name))
					                writer.WriteULEB128P1(DexConsts.NoIndex);
				                else
					                writer.WriteULEB128P1(StringLookup[name]);
				                break;
			                case DebugOpCodes.StartLocalExtended:
			                case DebugOpCodes.StartLocal:
				                // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
				                // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
				                var isExtended = ins.OpCode == DebugOpCodes.StartLocalExtended;

				                if (isExtended)
					                CheckOperand(ins, 4, typeof(Register), typeof(String), typeof(TypeReference), typeof(String));
				                else
					                CheckOperand(ins, 3, typeof(Register), typeof(String), typeof(TypeReference));

				                writer.WriteULEB128((uint)((Register)ins.Operands[0]).Index);

				                name = (String)ins.Operands[1];
				                if (string.IsNullOrEmpty(name))
					                writer.WriteULEB128P1(DexConsts.NoIndex);
				                else
					                writer.WriteULEB128P1(StringLookup[name]);

				                var type = (TypeReference)ins.Operands[2];
				                if (type == null)
					                writer.WriteULEB128P1(DexConsts.NoIndex);
				                else
					                writer.WriteULEB128P1(TypeLookup[type]);

				                if (isExtended)
				                {
					                var signature = (String)ins.Operands[3];
					                if (string.IsNullOrEmpty(signature))
						                writer.WriteULEB128P1(DexConsts.NoIndex);
					                else
						                writer.WriteULEB128P1(StringLookup[signature]);
				                }

				                break;
			                //case DebugOpCodes.EndSequence:
			                //case DebugOpCodes.Special:
				                // between 0x0a and 0xff (inclusive)
			                //case DebugOpCodes.SetPrologueEnd:
			                //case DebugOpCodes.SetEpilogueBegin:
			                //default:
				            //    break;
		                }
	                }
                }
            }

            if (count > 0)
                Map.Add(TypeCodes.DebugInfo, new MapItem(TypeCodes.DebugInfo, count, offset));
        }
        #endregion

        #region " EncodedArray "
        // this is really test code, need to optimize
        private string GetByteArrayAsString(IEnumerable<byte> bytes)
        {
            var builder = new StringBuilder();
            foreach (var item in bytes)
                builder.AppendLine(item.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private void WriteEncodedArray(BinaryWriter writer)
        {
            var buffers = new Dictionary<string, uint>();
            var offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            var memoryStream = new MemoryStream();
            using (var memoryWriter = new BinaryWriter(memoryStream))
            {
                for (var c = 0; c < FlatClasses.Count; c++)
                {
                    var @class = FlatClasses[c];
                    var values = new List<object>();
                    var lastNonNullIndex = -1;

                    for (var i = 0; i < @class.Fields.Count; i++)
                    {
                        var field = @class.Fields[i];
                        switch (ValueFormat.GetFormat(field.Value))
                        {
                            case ValueFormats.Annotation:
                            case ValueFormats.Array:
                            case ValueFormats.Method:
                            case ValueFormats.Type:
                            case ValueFormats.String:
                            case ValueFormats.Enum:
                            case ValueFormats.Field:
                                // always set
                                lastNonNullIndex = i;
                                break;
                            case ValueFormats.Null:
                                // never set
                                break;
                            case ValueFormats.Double:
                            case ValueFormats.Float:
								// ReSharper disable CompareOfFloatsByEqualityOperator
                                if (Convert.ToDouble(field.Value) != 0)
                                    lastNonNullIndex = i;
								// ReSharper restore CompareOfFloatsByEqualityOperator
								break;
                            case ValueFormats.Boolean:
                            case ValueFormats.Byte:
                            case ValueFormats.Char:
                            case ValueFormats.Int:
                            case ValueFormats.Long:
                            case ValueFormats.Short:
                                if (Convert.ToInt64(field.Value) != 0)
                                    lastNonNullIndex = i;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        values.Add(field.Value);
                    }

                    if (lastNonNullIndex != -1)
                    {
                        memoryStream.Position = 0;
                        memoryStream.SetLength(0);

                        WriteValues(memoryWriter, values.Take(lastNonNullIndex + 1).ToArray());
                        var buffer = memoryStream.ToArray();
                        var key = GetByteArrayAsString(buffer);

                        if (!buffers.ContainsKey(key))
                        {
                            count++;
                            buffers.Add(key, (uint)writer.BaseStream.Position);
                            writer.Write(buffer);
                        }

                        ClassDefinitionsMarkers[c].StaticValuesMarker.Value = buffers[key];
                    }
                }
            }

            if (count > 0)
                Map.Add(TypeCodes.EncodedArray, new MapItem(TypeCodes.EncodedArray, count, offset));
        }
        #endregion

        #region " ClassData "
        private void WriteClassData(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            for (var i = 0; i < FlatClasses.Count; i++)
            {
                var @class = FlatClasses[i];

                var staticFields = (@class.Fields.Where(field => field.IsStatic).OrderBy(field => FieldLookup[field])).ToList();
                var instanceFields = (@class.Fields.Except(staticFields).OrderBy(field => FieldLookup[field])).ToList();
                var virtualMethods = (@class.Methods.Where(method => method.IsVirtual).OrderBy(method => MethodLookup[method])).ToList();
                var directMethods = (@class.Methods.Except(virtualMethods).OrderBy(method => MethodLookup[method])).ToList();

	            if ((staticFields.Count + instanceFields.Count + virtualMethods.Count + directMethods.Count) <= 0)
		            continue;

	            ClassDefinitionsMarkers[i].ClassDataMarker.Value = (uint)writer.BaseStream.Position;
	            count++;

	            writer.WriteULEB128((uint)staticFields.Count);
	            writer.WriteULEB128((uint)instanceFields.Count);
	            writer.WriteULEB128((uint)directMethods.Count);
	            writer.WriteULEB128((uint)virtualMethods.Count);

	            WriteFields(writer, staticFields);
	            WriteFields(writer, instanceFields);
	            WriteMethods(writer, directMethods);
	            WriteMethods(writer, virtualMethods);
            }

            // File "global" alignment (EnsureAlignment is used for local alignment)
            while((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte) 0);

            if (count > 0)
                Map.Add(TypeCodes.ClassData, new MapItem(TypeCodes.ClassData, count, offset));
        }

        private void WriteFields(BinaryWriter writer, IEnumerable<FieldDefinition> fields)
        {
	        var lastIndex = 0;
            foreach (var field in fields)
            {
	            var fieldIndex = FieldLookup[field];

	            writer.WriteULEB128((uint)(fieldIndex - lastIndex));
	            writer.WriteULEB128((uint)field.AccessFlags);

	            lastIndex = fieldIndex;
            }
        }

        private void WriteMethods(BinaryWriter writer, IEnumerable<MethodDefinition> methods)
        {
	        var lastIndex = 0;
	        foreach (var method in methods)
	        {
		        var methodIndex = MethodLookup[method];

		        writer.WriteULEB128((uint)(methodIndex - lastIndex));
		        writer.WriteULEB128((uint)method.AccessFlags);
		        writer.WriteULEB128(Codes[method]);

		        lastIndex = methodIndex;
	        }
        }

	    #endregion

        #region " MapList "
        private void WriteMapList(BinaryWriter writer)
        {
            var sectionOffset = (uint) writer.BaseStream.Position;
            HeaderMarkers.MapMarker.Value = sectionOffset;
            Map.Add(TypeCodes.MapList, new MapItem(TypeCodes.MapList, 1, sectionOffset));

            writer.EnsureAlignment(sectionOffset, 4, () =>
            {
                writer.Write(Map.Count);
                foreach (var item in Map.Values)
                {
                    writer.Write((ushort)item.Type);
                    writer.Write((ushort)0); // unused
                    writer.Write(item.Size);
                    writer.Write(item.Offset);
                }
            });

            var filesize = (uint)writer.BaseStream.Position;
            HeaderMarkers.FileSizeMarker.Value = filesize;

            var lastEntry = TypeCodes.Header;
            foreach (var type in Map.Keys)
            {
                if (lastEntry == TypeCodes.ClassDef)
                    HeaderMarkers.DataMarker.Value = new SizeOffset(filesize - Map[type].Offset, Map[type].Offset);

				lastEntry = type;
            }
        }
        #endregion

        #region " Signature & CheckSum "
        private byte[] ComputeSHA1Signature(BinaryWriter writer)
        {
            writer.Seek((int)HeaderMarkers.SignatureMarker.Positions[0] + DexConsts.SignatureSize, SeekOrigin.Begin);
            var crypto = new SHA1CryptoServiceProvider();
            var signature = crypto.ComputeHash(writer.BaseStream);
            HeaderMarkers.SignatureMarker.Value = signature;
            return signature;
        }

        private uint ComputeAdlerCheckSum(BinaryWriter writer)
        {
            writer.Seek((int)HeaderMarkers.SignatureMarker.Positions[0], SeekOrigin.Begin);
            ushort s1 = 1;
            ushort s2 = 0;
            int value;
            while ((value = writer.BaseStream.ReadByte()) != -1)
            {
                s1 = (ushort)((s1 + value) % 65521);
                s2 = (ushort)((s1 + s2) % 65521);
            }
            var checksum = (uint)(s2 << 16 | s1);
            HeaderMarkers.CheckSumMarker.Value = checksum;
            return checksum;
        }
        #endregion

        public void WriteTo(BinaryWriter writer)
        {
            new ModelSorter().Collect(Dex);
            //new ModelShuffler().Collect(Dex);
            Map.Clear();

            StringLookup = CollectStrings();
            TypeLookup = CollectTypes();
            MethodLookup = CollectMethods();
            FieldLookup = CollectFields();
            PrototypeLookup = CollectPrototypes();
            FlatClasses = ClassDefinition.Flattenize(Dex.Classes);

            // Standard sort then topological sort
            var tsorter = new TopologicalSorter();
            FlatClasses.Sort(new ClassDefinitionComparer());
            FlatClasses = new List<ClassDefinition>(tsorter.TopologicalSort(FlatClasses, new ClassDefinitionComparer()));

            WriteHeader(writer);
            WriteStringId(writer);
            WriteTypeId(writer);
            WritePrototypeId(writer);
            WriteFieldId(writer);
            WriteMethodId(writer);
            WriteClassDef(writer);
            
            WriteAnnotationSetRefList(writer);
            WriteAnnotationSet(writer);
            WriteCode(writer);
            WriteAnnotationsDirectory(writer);
            WriteTypeLists(writer);
            WriteStringData(writer);
            WriteDebugInfo(writer);
            WriteAnnotations(writer);
            WriteEncodedArray(writer);
            WriteClassData(writer);

            WriteMapList(writer);

            Signature = ComputeSHA1Signature(writer);
            CheckSum = ComputeAdlerCheckSum(writer);
        }
      
    }
}
