/* Dexer Copyright (c) 2010 Sebastien LEBRETON

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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.Instructions;
using Dexer.IO.Collector;
using Dexer.IO.Markers;
using Dexer.Metadata;

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
            Map.Add(TypeCodes.Header, new MapItem(TypeCodes.Header, 1, (uint) writer.BaseStream.Position));
            writer.EnsureAlignment(4, () =>
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

            var strings = new List<string>(collector.Items.Keys);
            strings.Sort(new Dexer.IO.Collector.StringComparer());
            Dex.Strings = strings;

            var lookup = new Dictionary<string, int>();
            for (int i = 0; i < strings.Count; i++)
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

            for (int i = 0; i < Dex.Strings.Count; i++)
                StringMarkers.Add(writer.MarkUInt());
        }
        #endregion

        #region " StringData "
        private void WriteStringData(BinaryWriter writer)
        {
            if (Dex.Strings.Count > 0)
                Map.Add(TypeCodes.StringData, new MapItem(TypeCodes.StringData, (uint)Dex.Strings.Count, (uint)writer.BaseStream.Position));

            for (int i = 0; i < Dex.Strings.Count; i++)
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

            Dex.Prototypes = collector.ToList(new PrototypeComparer());

            var lookup = new Dictionary<Prototype, int>();
            for (int i = 0; i < Dex.Prototypes.Count; i++)
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

            foreach (Prototype prototype in Dex.Prototypes)
            {
                writer.Write((uint)StringLookup[TypeDescriptor.Encode(prototype)]);
                writer.Write((uint)Dex.TypeReferences.IndexOf(prototype.ReturnType));
                PrototypeTypeListMarkers.Add(writer.MarkUInt());
            }
        }
        #endregion

        #region " TypeList "
        private void WriteTypeList(BinaryWriter writer, ushort[] typelist, UIntMarker marker)
        {
            if (typelist.Length > 0)
            {
                String key = GetTypeListAsString(typelist);
                if (!TypeLists.ContainsKey(key))
                {
                    writer.EnsureAlignment(4, () =>
                    {
                        TypeLists.Add(key, (uint)writer.BaseStream.Position);

                        writer.Write((uint)typelist.Length);

                        foreach (ushort item in typelist)
                            writer.Write(item);
                    });
                }
                if (marker != null)
                    marker.Value = TypeLists[key];
            }

        }

        private void WriteTypeLists(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;

            for (int i=0; i< FlatClasses.Count; i++)
                WriteTypeList(writer, ComputeTypeList(FlatClasses[i].Interfaces), ClassDefinitionsMarkers[i].InterfacesMarker);

            for (int i=0; i< Dex.Prototypes.Count; i++)
                WriteTypeList(writer, ComputeTypeList(Dex.Prototypes[i].Parameters), PrototypeTypeListMarkers[i]);
    
            if (TypeLists.Count > 0)
                Map.Add(TypeCodes.TypeList, new MapItem(TypeCodes.TypeList, (uint)TypeLists.Count, offset));
        }

        private string GetTypeListAsString(ushort[] typelist)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ushort item in typelist)
                builder.AppendLine(item.ToString());
            return builder.ToString();
        }

        private ushort[] ComputeTypeList(List<Parameter> parameters)
        {
            List<ushort> result = new List<ushort>();
            foreach (Parameter parameter in parameters)
                result.Add((ushort)TypeLookup[parameter.Type]);
            return result.ToArray();
        }

        private ushort[] ComputeTypeList(List<ClassReference> classes)
        {
            List<ushort> result = new List<ushort>();
            foreach (ClassReference @class in classes)
                result.Add((ushort)TypeLookup[@class]);
            return result.ToArray();
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

            foreach (FieldReference field in Dex.FieldReferences)
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

            foreach (MethodReference method in Dex.MethodReferences)
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

            foreach (TypeReference tref in Dex.TypeReferences)
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

            foreach (ClassDefinition @class in FlatClasses)
            {
                writer.EnsureAlignment(4, () =>
                {
                    ClassDefinitionMarkers markers = new ClassDefinitionMarkers();
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
            uint offset = (uint)writer.BaseStream.Position;

            foreach (ClassDefinition @class in FlatClasses)
            {
                writer.EnsureAlignment(4, () =>
                {
                    foreach (MethodDefinition mdef in @class.Methods)
                    {
                        if ((mdef.Prototype.ContainsAnnotation()))
                        {
                            AnnotationSetRefLists.Add(mdef, (uint)writer.BaseStream.Position);
                            writer.Write(mdef.Prototype.Parameters.Count);
                            foreach (Parameter parameter in mdef.Prototype.Parameters)
                                AnnotationSetRefListMarkers.Add(parameter, writer.MarkUInt());
                        }
                    }
                });
            }

            if (AnnotationSetRefLists.Count > 0)
                Map.Add(TypeCodes.AnnotationSetRefList, new MapItem(TypeCodes.AnnotationSetRefList, (uint)AnnotationSetRefLists.Count, offset));
        }
        #endregion

        #region " AnnotationSet "
        private void WriteAnnotationSet(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;

            foreach (ClassDefinition @class in FlatClasses)
            {
                WriteAnnotationSet(writer, @class, false);

                foreach (FieldDefinition field in @class.Fields)
                    WriteAnnotationSet(writer, field, false);

                foreach (MethodDefinition method in @class.Methods)
                    WriteAnnotationSet(writer, method, false);

            }

            foreach (ClassDefinition @class in FlatClasses)
            {
                foreach (MethodDefinition method in @class.Methods)
                    if (method.Prototype.ContainsAnnotation())
                        foreach (Parameter parameter in method.Prototype.Parameters)
                            AnnotationSetRefListMarkers[parameter].Value = WriteAnnotationSet(writer, parameter, true);
            }

            if (AnnotationSets.Count > 0)
                Map.Add(TypeCodes.AnnotationSet, new MapItem(TypeCodes.AnnotationSet, (uint)AnnotationSets.Count, offset));
        }

        private uint WriteAnnotationSet(BinaryWriter writer, IAnnotationProvider provider, bool writezero)
        {
            var key = new AnnotationSet(provider);
            uint offset = 0;

            if (!AnnotationSets.ContainsKey(key))
            {
                writer.EnsureAlignment(4, () =>
                {
                    offset = (uint)writer.BaseStream.Position;

                    if (provider.Annotations.Count > 0 || writezero)
                        writer.Write(provider.Annotations.Count);

                    foreach (Annotation annotation in provider.Annotations)
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

            foreach (AnnotationArgument argument in annotation.Arguments)
            {
                writer.WriteULEB128((uint)StringLookup[argument.Name]);
                WriteValue(writer, argument.Value);
            }
        }

        private int getBytesNeeded(long value)
        {
            int result = 0;
            do
            {
                value >>= 8;
                result++;
            } while (value > 0);
            return result;
        }

        private void WriteValue(BinaryWriter writer, object value)
        {
            int valueArgument = 0;

            ValueFormats format = ValueFormat.GetFormat(value);
            switch (format)
            {
                case ValueFormats.Short:
                case ValueFormats.Char:
                case ValueFormats.Int:
                case ValueFormats.Long:
                    valueArgument = getBytesNeeded(Convert.ToInt64(value)) - 1;
                    break;
                case ValueFormats.String:
                    valueArgument = getBytesNeeded(StringLookup[(String)value]) - 1;
                    break;
                case ValueFormats.Type:
                    valueArgument = getBytesNeeded(TypeLookup[(TypeReference)value]) - 1;
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    valueArgument = getBytesNeeded(FieldLookup[(FieldReference)value]) - 1;
                    break;
                case ValueFormats.Method:
                    valueArgument = getBytesNeeded(MethodLookup[(MethodReference)value]) - 1;
                    break;
            }

            byte data = (byte)(valueArgument << 5);
            data |= (byte)format;
            writer.Write(data);

            switch (format)
            {
                case ValueFormats.Byte:
                    writer.Write(Convert.ToSByte(value));
                    break;
                case ValueFormats.Short:
                case ValueFormats.Char:
                case ValueFormats.Int:
                case ValueFormats.Long:
                    writer.WriteByByteLength(Convert.ToInt64(value), valueArgument + 1);
                    break;
                case ValueFormats.Float:
                    writer.Write(Convert.ToSingle(value));
                    break;
                case ValueFormats.Double:
                    writer.Write(Convert.ToDouble(value));
                    break;
                case ValueFormats.String:
                    writer.WriteByByteLength(StringLookup[(String)value], valueArgument + 1);
                    break;
                case ValueFormats.Type:
                    writer.WriteByByteLength(TypeLookup[(TypeReference)value], valueArgument + 1);
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    writer.WriteByByteLength(FieldLookup[(FieldReference)value], valueArgument + 1);
                    break;
                case ValueFormats.Method:
                    writer.WriteByByteLength(MethodLookup[(MethodReference)value], valueArgument + 1);
                    break;
                case ValueFormats.Array:
                    WriteValues(writer, (object[])value);
                    break;
                case ValueFormats.Annotation:
                    WriteEncodedAnnotation(writer, value as Annotation);
                    break;
                case ValueFormats.Null:
                    break;
                case ValueFormats.Boolean:
                    writer.Write(Convert.ToBoolean(value));
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void WriteValues(BinaryWriter writer, object[] values)
        {
            writer.WriteULEB128((uint)values.Length);
            foreach (Object value in values)
                WriteValue(writer, value);
        }

        private void WriteAnnotations(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;
            
            Dictionary<Annotation, uint> annotations = new Dictionary<Annotation, uint>();
            foreach (AnnotationSet annotationset in AnnotationSets.Keys)
            {
                foreach (Annotation annotation in annotationset)
                {
                    if (!annotations.ContainsKey(annotation))
                    {
                        uint annoffset = (uint)writer.BaseStream.Position;
                        WriteAnnotation(writer, annotation);
                        annotations.Add(annotation, annoffset);

                        AnnotationSetMarkers[annotation].Value = annoffset;
                    }
                }
            }

            if (annotations.Count > 0)
                Map.Add(TypeCodes.Annotation, new MapItem(TypeCodes.Annotation, (uint) annotations.Count, offset));
        }
        #endregion

        #region " Collect and sort "
        public Dictionary<T, int> Collect<T>(List<T> container, IComparer<T> comparer)
        {
            Dictionary<T, int> result = new Dictionary<T, int>();
            container.Sort(comparer);

            for (int i = 0; i < container.Count; i++)
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

        private List<ClassDefinition> Flattenize(List<ClassDefinition> container)
        {
            List<ClassDefinition> result = new List<ClassDefinition>();
            foreach (ClassDefinition @class in container)
            {
                result.Add(@class);
                result.AddRange(Flattenize(@class.InnerClasses));
            }
            return result;
        }
        #endregion

        #region " AnnotationsDirectory "
        private void WriteAnnotationsDirectory(BinaryWriter writer)
        {
            uint offset = (uint) writer.BaseStream.Position;
            uint count = 0;

            Dictionary<AnnotationSet, uint> classAnnotationSets = new Dictionary<AnnotationSet, uint>();

            for (int i = 0; i < FlatClasses.Count; i++ )
            {
                ClassDefinition @class = FlatClasses[i];
                writer.EnsureAlignment(4, () =>
                {
                    List<FieldDefinition> annotatedFields = new List<FieldDefinition>();
                    List<MethodDefinition> annotatedMethods = new List<MethodDefinition>();
                    List<MethodDefinition> annotatedParametersList = new List<MethodDefinition>();

                    foreach (FieldDefinition field in @class.Fields)
                        if (field.Annotations.Count > 0)
                            annotatedFields.Add(field);

                    foreach (MethodDefinition method in @class.Methods)
                    {
                        if (method.Annotations.Count > 0)
                            annotatedMethods.Add(method);
                        if (method.Prototype.ContainsAnnotation())
                            annotatedParametersList.Add(method);
                    }

                    int total = @class.Annotations.Count + annotatedFields.Count + annotatedMethods.Count + annotatedParametersList.Count;
                    if (total > 0)
                    {
                        // all datas except class annotations are specific.
                        if (total == @class.Annotations.Count)
                        {
                            AnnotationSet set = new AnnotationSet(@class);
                            if (classAnnotationSets.ContainsKey(set))
                            {
                                ClassDefinitionsMarkers[i].AnnotationsMarker.Value = classAnnotationSets[set];
                                return;
                            }
                            else
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

                        foreach (FieldDefinition field in annotatedFields)
                        {
                            writer.Write(FieldLookup[field]);
                            writer.Write(AnnotationSets[new AnnotationSet(field)]);
                        }

                        foreach (MethodDefinition method in annotatedMethods)
                        {
                            writer.Write(MethodLookup[method]);
                            writer.Write(AnnotationSets[new AnnotationSet(method)]);
                        }

                        foreach (MethodDefinition method in annotatedParametersList)
                        {
                            writer.Write(MethodLookup[method]);
                            writer.Write(AnnotationSetRefLists[method]);
                        }
                    }

                });
            }

            if (count > 0)
                Map.Add(TypeCodes.AnnotationDirectory, new MapItem(TypeCodes.AnnotationDirectory, count, offset));
        }
        #endregion

        #region " Code "
        private void WriteCode(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            foreach (ClassDefinition @class in FlatClasses)
            {
                foreach (MethodDefinition method in @class.Methods)
                {
                    Codes.Add(method, 0);
                    MethodBody body = method.Body;
                    if (body != null)
                    {
                        writer.EnsureAlignment(4, () => 
                        {
                            Codes[method] = (uint)writer.BaseStream.Position;
                            count++;

                            writer.Write((ushort)body.Registers.Count);
                            writer.Write(body.IncomingArguments);
                            writer.Write(body.OutgoingArguments);
                            writer.Write((ushort)body.Exceptions.Count);
                            DebugMarkers.Add(method, writer.MarkUInt());

                            InstructionWriter iwriter = new InstructionWriter(this, method);
                            iwriter.WriteTo(writer);

                            if ((body.Exceptions.Count != 0) && (iwriter.Codes.Length % 2 != 0))
                                writer.Write((ushort)0); // padding (tries 4-byte alignment)

                            Dictionary<CatchSet, List<ExceptionHandler>> catchHandlers = new Dictionary<CatchSet, List<ExceptionHandler>>();
                            Dictionary<ExceptionHandler, UShortMarker> ExceptionsMarkers = new Dictionary<ExceptionHandler, UShortMarker>();
                            foreach (ExceptionHandler handler in body.Exceptions)
                            {
                                writer.Write(handler.TryStart.Offset);
                                writer.Write((ushort)(iwriter.LookupLast[handler.TryEnd] - handler.TryStart.Offset + 1));
                                ExceptionsMarkers.Add(handler, writer.MarkUShort());

                                CatchSet set = new CatchSet(handler);
                                if (!catchHandlers.ContainsKey(set))
                                    catchHandlers.Add(set, new List<ExceptionHandler>());

                                catchHandlers[set].Add(handler);
                            }

                            List<CatchSet> catchSets = catchHandlers.Keys.ToList();
                            catchSets.Sort(new CatchSetComparer());

                            if (catchSets.Count > 0)
                            {
                                long baseOffset = writer.BaseStream.Position;
                                writer.WriteULEB128((uint)catchSets.Count);
                                foreach (CatchSet set in catchSets)
                                {
                                    long itemoffset = writer.BaseStream.Position - baseOffset;

                                    if (set.CatchAll != null)
                                        writer.WriteSLEB128(-set.Count);
                                    else
                                        writer.WriteSLEB128(set.Count);

                                    foreach (ExceptionHandler handler in catchHandlers[set])
                                        ExceptionsMarkers[handler].Value = (ushort)itemoffset;

                                    foreach (Catch @catch in set)
                                    {
                                        writer.WriteULEB128((uint)TypeLookup[@catch.Type]);
                                        writer.WriteULEB128((uint)@catch.Instruction.Offset);
                                    }

                                    if (set.CatchAll != null)
                                        writer.WriteULEB128((uint)set.CatchAll.Offset);
                                }
                            }
                        });
                    }
                }
            }

            if (Codes.Count > 0)
                Map.Add(TypeCodes.Code, new MapItem(TypeCodes.Code, count, offset));
        }
        #endregion

        #region " DebugInfo "
        private void CheckOperand(DebugInstruction ins, int operandCount, params Type[] types)
        {
            if (ins.Operands.Count != operandCount)
                throw new DebugInstructionException(ins, string.Format("Expecting {0} operands", operandCount));

            for (int i = 0; i < ins.Operands.Count; i++)
            {
                try
                {
                    if (types[i].IsAssignableFrom(ins.Operands[i].GetType()))
                        continue;
                    Convert.ChangeType(ins.Operands[i], types[i]);
                }
                catch (Exception)
                {
                    throw new DebugInstructionException(ins, string.Format("Expecting '{0}' Type (or compatible) for Operands[{1}]", types[i], i));
                }
            }
        }

        private void WriteDebugInfo(BinaryWriter writer)
        {
            uint offset = (uint) writer.BaseStream.Position;
            uint count = 0;

            foreach (ClassDefinition @class in FlatClasses)
            {
                foreach (MethodDefinition method in @class.Methods)
                {
                    MethodBody body = method.Body;
                    if (body != null && body.DebugInfo != null)
                    {
                        DebugMarkers[method].Value = (uint)writer.BaseStream.Position;
                        count++;

                        // byte aligned
                        DebugInfo debugInfo = body.DebugInfo;
                        writer.WriteULEB128(debugInfo.LineStart);

                        if (debugInfo.Parameters.Count != method.Prototype.Parameters.Count)
                            throw new MalformedException("Unexpected parameter count in DebugInfo, must match with prototype");

                        writer.WriteULEB128((uint)debugInfo.Parameters.Count);
                        foreach (String parameter in debugInfo.Parameters)
                        {
                            if (string.IsNullOrEmpty(parameter))
                                writer.WriteULEB128p1(DexConsts.NoIndex);
                            else
                                writer.WriteULEB128p1(StringLookup[parameter]);
                        }

                        foreach (DebugInstruction ins in debugInfo.DebugInstructions)
                        {
                            String name = null;
                            String signature = null;
                            TypeReference type = null;

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
                                        writer.WriteULEB128p1(DexConsts.NoIndex);
                                    else
                                        writer.WriteULEB128p1(StringLookup[name]);
                                    break;
                                case DebugOpCodes.StartLocalExtended:
                                case DebugOpCodes.StartLocal:
                                    // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
                                    // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
                                    Boolean isExtended = ins.OpCode == DebugOpCodes.StartLocalExtended;

                                    if (isExtended)
                                        CheckOperand(ins, 4, typeof(Register), typeof(String), typeof(TypeReference), typeof(String));
                                    else
                                        CheckOperand(ins, 3, typeof(Register), typeof(String), typeof(TypeReference));

                                    writer.WriteULEB128((uint)((Register)ins.Operands[0]).Index);

                                    name = (String)ins.Operands[1];
                                    if (string.IsNullOrEmpty(name))
                                        writer.WriteULEB128p1(DexConsts.NoIndex);
                                    else
                                        writer.WriteULEB128p1(StringLookup[name]);

                                    type = (TypeReference)ins.Operands[2];
                                    if (type == null)
                                        writer.WriteULEB128p1(DexConsts.NoIndex);
                                    else
                                        writer.WriteULEB128p1(TypeLookup[type]);

                                    if (isExtended)
                                    {
                                        signature = (String)ins.Operands[3];
                                        if (string.IsNullOrEmpty(signature))
                                            writer.WriteULEB128p1(DexConsts.NoIndex);
                                        else
                                            writer.WriteULEB128p1(StringLookup[signature]);
                                    }

                                    break;
                                case DebugOpCodes.EndSequence:
                                case DebugOpCodes.Special:
                                // between 0x0a and 0xff (inclusive)
                                case DebugOpCodes.SetPrologueEnd:
                                case DebugOpCodes.SetEpilogueBegin:
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if (count > 0)
                Map.Add(TypeCodes.DebugInfo, new MapItem(TypeCodes.DebugInfo, count, offset));
        }
        #endregion

        #region " EncodedArray "
        private void WriteEncodedArray(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            for (int c = 0; c < FlatClasses.Count; c++ )
            {
                ClassDefinition @class = FlatClasses[c];
                List<object> values = new List<object>();
                int lastNonNullIndex = -1;

                for (int i = 0; i < @class.Fields.Count; i++)
                {
                    FieldDefinition field = @class.Fields[i];
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
                            if (Convert.ToDouble(field.Value) != 0)
                                lastNonNullIndex = i;
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
                    count++;
                    ClassDefinitionsMarkers[c].StaticValuesMarker.Value = (uint) writer.BaseStream.Position;
                    WriteValues(writer, Enumerable.Take(values, lastNonNullIndex + 1).ToArray());
                }
            }

            if (count > 0)
                Map.Add(TypeCodes.EncodedArray, new MapItem(TypeCodes.EncodedArray, count, offset));
        }
        #endregion

        #region " ClassData "
        private void WriteClassData(BinaryWriter writer)
        {
            uint offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            for (int i = 0; i < FlatClasses.Count; i++)
            {
                ClassDefinition @class = FlatClasses[i];

                var staticFields = (@class.Fields.Where((field) => field.IsStatic).OrderBy((field) => FieldLookup[field])).ToList();
                var instanceFields = (@class.Fields.Except(staticFields).OrderBy((field) => FieldLookup[field])).ToList();
                var virtualMethods = (@class.Methods.Where((method) => method.IsVirtual).OrderBy((method) => MethodLookup[method])).ToList();
                var directMethods = (@class.Methods.Except(virtualMethods).OrderBy((method) => MethodLookup[method])).ToList();

                if ((staticFields.Count + instanceFields.Count + virtualMethods.Count + directMethods.Count) > 0)
                {
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
            }

            // File "global" alignment (EnsureAlignment is used for local alignment)
            while((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte) 0);

            if (count > 0)
                Map.Add(TypeCodes.ClassData, new MapItem(TypeCodes.ClassData, count, offset));
        }

        private void WriteFields(BinaryWriter writer, List<FieldDefinition> fields)
        {
            int fieldIndex = 0;
            int lastIndex = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                fieldIndex = FieldLookup[fields[i]];

                writer.WriteULEB128((uint)(fieldIndex - lastIndex));
                writer.WriteULEB128((uint)fields[i].AccessFlags);

                lastIndex = fieldIndex;
            }
        }

        private void WriteMethods(BinaryWriter writer, List<MethodDefinition> methods)
        {
            int methodIndex = 0;
            int lastIndex = 0;
            for (int i = 0; i < methods.Count; i++)
            {
                methodIndex = MethodLookup[methods[i]];

                writer.WriteULEB128((uint)(methodIndex - lastIndex));
                writer.WriteULEB128((uint)methods[i].AccessFlags);
                writer.WriteULEB128(Codes[methods[i]]);

                lastIndex = methodIndex;
            }
        }
        #endregion

        #region " MapList "
        private void WriteMapList(BinaryWriter writer)
        {
            HeaderMarkers.MapMarker.Value = (uint)writer.BaseStream.Position;
            Map.Add(TypeCodes.MapList, new MapItem(TypeCodes.MapList, 1, (uint)writer.BaseStream.Position));

            writer.EnsureAlignment(4, () =>
            {
                writer.Write(Map.Count);
                foreach (MapItem item in Map.Values)
                {
                    writer.Write((ushort)item.Type);
                    writer.Write((ushort)0); // unused
                    writer.Write(item.Size);
                    writer.Write(item.Offset);
                }
            });

            uint filesize = (uint)writer.BaseStream.Position;
            HeaderMarkers.FileSizeMarker.Value = filesize;

            TypeCodes lastEntry = TypeCodes.Header;
            foreach (TypeCodes type in Map.Keys)
            {
                if (lastEntry == TypeCodes.ClassDef)
                {
                    HeaderMarkers.DataMarker.Value = new SizeOffset(filesize - Map[type].Offset, Map[type].Offset);
                }
                lastEntry = type;
            }
        }
        #endregion

        #region " Signature & CheckSum "
        private byte[] ComputeSHA1Signature(BinaryWriter writer)
        {
            writer.Seek((int)HeaderMarkers.SignatureMarker.Positions[0] + DexConsts.SignatureSize, SeekOrigin.Begin);
            SHA1CryptoServiceProvider crypto = new SHA1CryptoServiceProvider();
            byte[] signature = crypto.ComputeHash(writer.BaseStream);
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
            uint checksum = (uint)(s2 << 16 | s1);
            HeaderMarkers.CheckSumMarker.Value = checksum;
            return checksum;
        }
        #endregion

        public void WriteTo(BinaryWriter writer)
        {
            new ModelSorter().Collect(Dex);
            Map.Clear();

            StringLookup = CollectStrings();
            TypeLookup = CollectTypes();
            MethodLookup = CollectMethods();
            FieldLookup = CollectFields();
            PrototypeLookup = CollectPrototypes();
            FlatClasses = Flattenize(Dex.Classes);
            FlatClasses.Sort(new ClassDefinitionComparer());

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
