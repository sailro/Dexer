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
using System.IO;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.Instructions;
using Dexer.Metadata;

namespace Dexer.IO
{
    public class DexReader
    {
        private Dex Dex { get; set; }
        internal Map Map { get; set; }
        internal DexHeader Header { get; set; }

        public DexReader(Dex dex)
        {
            Dex = dex;
            Map = new Map();
            Header = new DexHeader();
        }

        #region " Header "
        private void ReadHeader(BinaryReader reader)
        {
            Header.Magic = reader.ReadBytes(DexConsts.FileMagic.Length);

            if (!Header.Magic.Match(DexConsts.FileMagic, 0))
                throw new MalformedException("Unexpected Magic number");


            Header.CheckSum = reader.ReadUInt32();
            Header.Signature = reader.ReadBytes(DexConsts.SignatureSize);

            Header.FileSize = reader.ReadUInt32();
            Header.HeaderSize = reader.ReadUInt32();
            Header.EndianTag = reader.ReadUInt32();

            if (Header.EndianTag != DexConsts.Endian)
                throw new MalformedException("Only Endian-encoded files are supported");

            Header.LinkSize = reader.ReadUInt32();
            Header.LinkOffset = reader.ReadUInt32();

            Header.MapOffset = reader.ReadUInt32();

            Header.StringsSize = reader.ReadUInt32();
            Header.StringsOffset = reader.ReadUInt32();

            Header.TypeReferencesSize = reader.ReadUInt32();
            Header.TypeReferencesOffset = reader.ReadUInt32();

            Header.PrototypesSize = reader.ReadUInt32();
            Header.PrototypesOffset = reader.ReadUInt32();

            Header.FieldReferencesSize = reader.ReadUInt32();
            Header.FieldReferencesOffset = reader.ReadUInt32();

            Header.MethodReferencesSize = reader.ReadUInt32();
            Header.MethodReferencesOffset = reader.ReadUInt32();

            Header.ClassDefinitionsSize = reader.ReadUInt32();
            Header.ClassDefinitionsOffset = reader.ReadUInt32();

            Header.DataSize = reader.ReadUInt32();
            Header.DataOffset = reader.ReadUInt32();
        }
        #endregion

        #region " MapList "
        private void ReadMapList(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.MapOffset, () =>
            {
                uint mapsize = reader.ReadUInt32();
                for (int i = 0; i < mapsize; i++)
                {
                    var item = new MapItem {Type = (TypeCodes) reader.ReadUInt16()};
	                reader.ReadUInt16(); // unused
                    item.Size = reader.ReadUInt32();
                    item.Offset = reader.ReadUInt32();
                    Map.Add(item.Type, item);
                }
            });
        }

        #endregion

        #region " Strings "
        private void ReadStrings(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.StringsOffset, () =>
            {
                var stringsDataOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(stringsDataOffset, SeekOrigin.Begin);
                for (var i = 0; i < Header.StringsSize; i++)
                {
                    Dex.Strings.Add(reader.ReadMUTF8String());
                }
            });
        }
        #endregion

        #region " Prefetch "
        private void PrefetchTypeReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.TypeReferencesOffset, () =>
            {
                reader.BaseStream.Seek(Header.TypeReferencesOffset, SeekOrigin.Begin);

                for (var i = 0; i < Header.TypeReferencesSize; i++)
                {
                    var descriptorIndex = reader.ReadInt32();
                    var descriptor = Dex.Strings[descriptorIndex];
                    Dex.TypeReferences.Add(TypeDescriptor.Allocate(descriptor));
                }

            });
        }

        private void PrefetchFieldDefinitions(BinaryReader reader, uint fieldcount)
        {
            var fieldIndex = 0;
            for (var i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                    fieldIndex = (int)reader.ReadULEB128();
                else
                    fieldIndex += (int)reader.ReadULEB128();

				reader.ReadULEB128();
                var fdef = new FieldDefinition(Dex.FieldReferences[fieldIndex]);
                Dex.FieldReferences[fieldIndex] = fdef;
            }
        }

        private void PrefetchMethodDefinitions(BinaryReader reader, uint methodcount)
        {
            var methodIndex = 0;
            for (var i = 0; i < methodcount; i++)
            {
                if (i == 0)
                    methodIndex = (int)reader.ReadULEB128();
                else
                    methodIndex += (int)reader.ReadULEB128();

				reader.ReadULEB128();
                reader.ReadULEB128();
                var mdef = new MethodDefinition(Dex.MethodReferences[methodIndex]);
                Dex.MethodReferences[methodIndex] = mdef;
            }
        }

        private void PrefetchClassDefinition(BinaryReader reader, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                var staticFieldSize = reader.ReadULEB128();
                var instanceFieldSize = reader.ReadULEB128();
                var directMethodSize = reader.ReadULEB128();
                var virtualMethodSize = reader.ReadULEB128();

                PrefetchFieldDefinitions(reader, staticFieldSize);
                PrefetchFieldDefinitions(reader, instanceFieldSize);
                PrefetchMethodDefinitions(reader, directMethodSize);
                PrefetchMethodDefinitions(reader, virtualMethodSize);
            });
        }

        private void PrefetchClassDefinitions(BinaryReader reader, bool prefetchMembers)
        {
            reader.PreserveCurrentPosition(Header.ClassDefinitionsOffset, () =>
            {
                for (var i = 0; i < Header.ClassDefinitionsSize; i++)
                {
                    var classIndex = reader.ReadInt32();

	                var reference = Dex.TypeReferences[classIndex] as ClassDefinition;
	                if (reference == null)
                    {
                        var cdef = new ClassDefinition((ClassReference)Dex.TypeReferences[classIndex]);
                        Dex.TypeReferences[classIndex] = cdef;
                        Dex.Classes.Add(cdef);
                    }

                    reader.ReadInt32(); // skip access_flags
                    reader.ReadInt32(); // skip superclass_idx
                    reader.ReadInt32(); // skip interfaces_off
                    reader.ReadInt32(); // skip source_file_idx
                    reader.ReadInt32(); // skip annotations_off

                    var classDataOffset = reader.ReadUInt32();
                    if ((classDataOffset > 0) && prefetchMembers)
                    {
                        PrefetchClassDefinition(reader, classDataOffset);
                    }

                    reader.ReadInt32(); // skip static_values_off
                }
            });
        }
        #endregion

        #region " Annotations "
        private void ReadAnnotationDirectory(BinaryReader reader, IAnnotationProvider provider, uint annotationOffset)
        {
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint classAnnotationOffset = reader.ReadUInt32();
                uint annotatedFieldsSize = reader.ReadUInt32();
                uint annotatedMethodsSize = reader.ReadUInt32();
                uint annotatedParametersSize = reader.ReadUInt32();

                if (classAnnotationOffset > 0)
                    provider.Annotations = ReadAnnotationSet(reader, classAnnotationOffset);

                for (var j = 0; j < annotatedFieldsSize; j++)
                {
	                var fieldDefinition = Dex.FieldReferences[reader.ReadInt32()] as FieldDefinition;
	                if (fieldDefinition != null)
		                fieldDefinition.Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());
                }

	            for (var j = 0; j < annotatedMethodsSize; j++)
	            {
		            var methodDefinition = Dex.MethodReferences[reader.ReadInt32()] as MethodDefinition;
		            if (methodDefinition != null)
			            methodDefinition.Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());
	            }

	            for (var j = 0; j < annotatedParametersSize; j++)
                {
                    var methodIndex = reader.ReadInt32();
                    var offset = reader.ReadUInt32();
                    var annotations = ReadAnnotationSetRefList(reader, offset);
                    var mdef = (Dex.MethodReferences[methodIndex] as MethodDefinition);

	                if (mdef == null)
		                break;

                    for (var i = 0; i < annotations.Count; i++)
                    {
	                    if (annotations[i].Count <= 0) 
							continue;
	                    
						mdef.Prototype.Parameters[i].Annotations = annotations[i];
                    }
                }
            });
        }

        private List<List<Annotation>> ReadAnnotationSetRefList(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<List<Annotation>>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                var size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    var offset = reader.ReadUInt32();
                    result.Add(ReadAnnotationSet(reader, offset));
                }
            });
            return result;
        }

        private List<Annotation> ReadAnnotationSet(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<Annotation>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                var size = reader.ReadUInt32();
                for (var i = 0; i < size; i++)
                {
                    var offset = reader.ReadUInt32();
                    result.Add(ReadAnnotation(reader, offset));
                }
            });
            return result;
        }

        private Annotation ReadAnnotation(BinaryReader reader, uint annotationOffset)
        {
            Annotation annotation = null;
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                var visibility = reader.ReadByte();
                annotation = ReadEncodedAnnotation(reader);
                annotation.Visibility = (AnnotationVisibility)visibility;
            });
            return annotation;
        }

        private Annotation ReadEncodedAnnotation(BinaryReader reader)
        {
            var typeIndex = (int)reader.ReadULEB128();
            var elementSize = (int)reader.ReadULEB128();

            var annotation = new Annotation {Type = (ClassReference) Dex.TypeReferences[typeIndex]};

	        for (var i = 0; i < elementSize; i++)
            {
                var argument = new AnnotationArgument();
                var nameIndex = (int)reader.ReadULEB128();
                argument.Name = Dex.Strings[nameIndex];
                argument.Value = ReadValue(reader);
                annotation.Arguments.Add(argument);
            }

            return annotation;
        }
        #endregion

        #region " Prototypes "
        private void ReadPrototypes(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.PrototypesOffset, () =>
            {
                for (var i = 0; i < Header.PrototypesSize; i++)
                {
                    //var thisOffset = reader.BaseStream.Position;
                    /*var shortyIndex =*/ reader.ReadInt32();
                    var returnTypeIndex = reader.ReadInt32();
                    var parametersOffset = reader.ReadUInt32();

                    var prototype = new Prototype {ReturnType = Dex.TypeReferences[returnTypeIndex]};

	                if (parametersOffset > 0)
                    {
                        ReadParameters(reader, prototype, parametersOffset);
                    }

                    Dex.Prototypes.Add(prototype);
                }
            });
        }

        private void ReadParameters(BinaryReader reader, Prototype prototype, uint parametersOffset)
        {
            reader.PreserveCurrentPosition(parametersOffset, () =>
            {
                var typecount = reader.ReadUInt32();
                for (var j = 0; j < typecount; j++)
                {
                    var parameter = new Parameter();
                    var typeIndex = reader.ReadUInt16();
                    parameter.Type = Dex.TypeReferences[typeIndex];
                    prototype.Parameters.Add(parameter);
                }
            });
        }
        #endregion

        #region " Definitions "
        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.ClassDefinitionsOffset, () =>
            {
                for (var i = 0; i < Header.ClassDefinitionsSize; i++)
                {
                    var classIndex = reader.ReadUInt32();

                    var cdef = (ClassDefinition)Dex.TypeReferences[(int)classIndex];
                    cdef.AccessFlags = (AccessFlags)reader.ReadUInt32();

                    var superClassIndex = reader.ReadUInt32();
                    if (superClassIndex != DexConsts.NoIndex)
                        cdef.SuperClass = (ClassReference)Dex.TypeReferences[(int)superClassIndex];

                    var interfaceOffset = reader.ReadUInt32();
                    var sourceFileIndex = reader.ReadUInt32();
                    var annotationOffset = reader.ReadUInt32();
                    var classDataOffset = reader.ReadUInt32();
                    var staticValuesOffset = reader.ReadUInt32();

                    if (interfaceOffset > 0)
                        ReadInterfaces(reader, cdef, interfaceOffset);

                    if (sourceFileIndex != DexConsts.NoIndex)
                        cdef.SourceFile = Dex.Strings[(int)sourceFileIndex];

                    if (classDataOffset > 0)
                        ReadClassDefinition(reader, cdef, classDataOffset);

                    if (annotationOffset > 0)
                        ReadAnnotationDirectory(reader, cdef, annotationOffset);

                    if (staticValuesOffset > 0)
                        ReadStaticValues(reader, cdef, staticValuesOffset);
                }
            });
        }

        private void ReadStaticValues(BinaryReader reader, ClassDefinition classDefinition, uint staticValuesOffset)
        {
            reader.PreserveCurrentPosition(staticValuesOffset, () =>
            {
                var values = ReadValues(reader);
                for (var j = 0; j < values.Length; j++)
                    classDefinition.Fields[j].Value = values[j];
            });
        }

        private void ReadInterfaces(BinaryReader reader, ClassDefinition classDefinition, uint interfaceOffset)
        {
            reader.PreserveCurrentPosition(interfaceOffset, () =>
            {
                var typecount = reader.ReadUInt32();
                for (var j = 0; j < typecount; j++)
                {
                    var typeIndex = reader.ReadUInt16();
                    classDefinition.Interfaces.Add((ClassReference)Dex.TypeReferences[typeIndex]);
                }
            });
        }

        private object[] ReadValues(BinaryReader reader)
        {
            var size = reader.ReadULEB128();
            var array = new ArrayList();
            for (uint i = 0; i < size; i++)
            {
                array.Add(ReadValue(reader));
            }
            return array.ToArray();
        }

        private object ReadValue(BinaryReader reader)
        {
            int data = reader.ReadByte();
            var valueFormat = data & 0x1F;
            var valueArgument = data >> 5;

            switch ((ValueFormats)valueFormat)
            {
                case ValueFormats.Byte:
                    return (sbyte) reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Short:
                    return (short)reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Char:
                    return (char)reader.ReadUnsignedPackedNumber(valueArgument + 1);
                case ValueFormats.Int:
                    return (int)reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Long:
                    return reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Float:
                    return BitConverter.ToSingle(BitConverter.GetBytes((int)reader.ReadSignedPackedNumber(valueArgument + 1)), 0); 
                case ValueFormats.Double:
                    return BitConverter.Int64BitsToDouble(reader.ReadSignedPackedNumber(valueArgument + 1));
                case ValueFormats.String:
                    return Dex.Strings[(int)reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Type:
                    return Dex.TypeReferences[(int)reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    return Dex.FieldReferences[(int)reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Method:
                    return Dex.MethodReferences[(int)reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Array:
                    return ReadValues(reader);
                case ValueFormats.Annotation:
                    return ReadEncodedAnnotation(reader);
                case ValueFormats.Null:
                    return null;
                case ValueFormats.Boolean:
                    return valueArgument != 0;
                default:
                    throw new ArgumentException();
            }
        }

        private void ReadFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            uint fieldIndex = 0;
            for (var i = 0; i < fieldcount; i++)
            {
                fieldIndex += reader.ReadULEB128();
                var accessFlags = reader.ReadULEB128();

                var fdef = (FieldDefinition)Dex.FieldReferences[(int)fieldIndex];
                fdef.AccessFlags = (AccessFlags)accessFlags;
                fdef.Owner = classDefinition;

                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount, bool isVirtual)
        {
            uint methodIndex = 0;
            for (var i = 0; i < methodcount; i++)
            {
                methodIndex += reader.ReadULEB128();
                var accessFlags = reader.ReadULEB128();
                var codeOffset = reader.ReadULEB128();

                var mdef = (MethodDefinition)Dex.MethodReferences[(int)methodIndex];
                mdef.AccessFlags = (AccessFlags)accessFlags;
                mdef.Owner = classDefinition;
                mdef.IsVirtual = isVirtual;

                classDefinition.Methods.Add(mdef);

                if (codeOffset > 0)
                    ReadMethodBody(reader, mdef, codeOffset);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition mdef, uint codeOffset)
        {
            reader.PreserveCurrentPosition(codeOffset, () =>
            {
                var registersSize = reader.ReadUInt16();
                var incomingArgsSize = reader.ReadUInt16();
                var outgoingArgsSize = reader.ReadUInt16();
                var triesSize = reader.ReadUInt16();
                var debugOffset = reader.ReadUInt32();

                mdef.Body = new MethodBody(mdef, registersSize)
	            {
		            IncomingArguments = incomingArgsSize,
		            OutgoingArguments = outgoingArgsSize
	            };

	            var ireader = new InstructionReader(Dex, mdef);
                ireader.ReadFrom(reader);

                if ((triesSize != 0) && (ireader.Codes.Length % 2 != 0))
                    reader.ReadUInt16(); // padding (4-byte alignment)

                if (triesSize != 0)
                    ReadExceptionHandlers(reader, mdef, ireader, triesSize);

                if (debugOffset != 0)
                    ReadDebugInfo(reader, mdef, debugOffset);
            });

        }

        private void ReadDebugInfo(BinaryReader reader, MethodDefinition mdef, uint debugOffset)
        {
            reader.PreserveCurrentPosition(debugOffset, () =>
            {
                var debugInfo = new DebugInfo(mdef.Body);
                mdef.Body.DebugInfo = debugInfo;

                var lineStart = reader.ReadULEB128();
                debugInfo.LineStart = lineStart;

                var parametersSize = reader.ReadULEB128();
                for (var i = 0; i < parametersSize; i++)
                {
                    var index = reader.ReadULEB128P1();
                    string name = null;
                    if (index != DexConsts.NoIndex && index >= 0)
                        name = Dex.Strings[(int)index];
                    debugInfo.Parameters.Add(name);
                }

                while (true)
                {
                    var ins = new DebugInstruction {OpCode = (DebugOpCodes) reader.ReadByte()};
	                debugInfo.DebugInstructions.Add(ins);

                    uint registerIndex;
	                long nameIndex;
	                string name;

                    switch (ins.OpCode)
                    {
                        case DebugOpCodes.AdvancePc:
                            // uleb128 addr_diff
                            var addrDiff = reader.ReadULEB128();
                            ins.Operands.Add(addrDiff);
                            break;
                        case DebugOpCodes.AdvanceLine:
                            // sleb128 line_diff
                            var lineDiff = reader.ReadSLEB128();
                            ins.Operands.Add(lineDiff);
                            break;
                        case DebugOpCodes.EndLocal:
                        case DebugOpCodes.RestartLocal:
                            // uleb128 register_num
                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int)registerIndex]);
                            break;
                        case DebugOpCodes.SetFile:
                            // uleb128p1 name_idx
                            nameIndex = reader.ReadULEB128P1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = Dex.Strings[(int)nameIndex];
                            ins.Operands.Add(name);
                            break;
                        case DebugOpCodes.StartLocalExtended:
                        case DebugOpCodes.StartLocal:
                            // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
                            // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
                            var isExtended = ins.OpCode == DebugOpCodes.StartLocalExtended;

                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int)registerIndex]);

                            nameIndex = reader.ReadULEB128P1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = Dex.Strings[(int)nameIndex];
                            ins.Operands.Add(name);

                            var typeIndex = reader.ReadULEB128P1();
                            TypeReference type = null;
                            if (typeIndex != DexConsts.NoIndex && typeIndex >= 0)
                                type = Dex.TypeReferences[(int)typeIndex];
                            ins.Operands.Add(type);

                            if (isExtended)
                            {
                                var signatureIndex = reader.ReadULEB128P1();
                                string signature = null;
                                if (signatureIndex != DexConsts.NoIndex && signatureIndex >= 0)
                                    signature = Dex.Strings[(int)signatureIndex];
                                ins.Operands.Add(signature);
                            }

                            break;
						case DebugOpCodes.EndSequence:
							return;
                        //case DebugOpCodes.Special:
                        // between 0x0a and 0xff (inclusive)
                        //case DebugOpCodes.SetPrologueEnd:
                        //case DebugOpCodes.SetEpilogueBegin:
                        //default:
                        //    break;
                    }
                }
            });
        }

        private void ReadExceptionHandlers(BinaryReader reader, MethodDefinition mdef, InstructionReader instructionReader, ushort triesSize)
        {
            var exceptionLookup = new Dictionary<uint, List<ExceptionHandler>>();
            for (var i = 0; i < triesSize; i++)
            {
                var startOffset = reader.ReadUInt32();
                var insCount = reader.ReadUInt16();
                var endOffset = startOffset + insCount - 1;
                uint handlerOffset = reader.ReadUInt16();

                var ehandler = new ExceptionHandler();
                mdef.Body.Exceptions.Add(ehandler);
                if (!exceptionLookup.ContainsKey(handlerOffset))
                {
                    exceptionLookup.Add(handlerOffset, new List<ExceptionHandler>());
                }
                exceptionLookup[handlerOffset].Add(ehandler);
                ehandler.TryStart = instructionReader.Lookup[(int)startOffset];
                // The last code unit covered (inclusive) is start_addr + insn_count - 1
                ehandler.TryEnd = instructionReader.LookupLast[(int)endOffset];
            }

            var baseOffset = reader.BaseStream.Position;
            var catchHandlersSize = reader.ReadULEB128();
            for (var i = 0; i < catchHandlersSize; i++)
            {
                var itemoffset = reader.BaseStream.Position - baseOffset;
                var catchTypes = reader.ReadSLEB128();
                var catchAllPresent = catchTypes <= 0;
                catchTypes = Math.Abs(catchTypes);

                for (var j = 0; j < catchTypes; j++)
                {
                    var typeIndex = reader.ReadULEB128();
                    var offset = reader.ReadULEB128();
                    var @catch = new Catch
	                {
		                Type = Dex.TypeReferences[(int) typeIndex],
		                Instruction = instructionReader.Lookup[(int) offset]
	                };

	                // As catch handler can be used in several tries, let's clone the catch
                    foreach (var ehandler in exceptionLookup[(uint)itemoffset])
                        ehandler.Catches.Add(@catch.Clone());
                }

	            if (!catchAllPresent)
					continue;
	            
				var caOffset = reader.ReadULEB128();
	            foreach (var ehandler in exceptionLookup[(uint)itemoffset])
		            ehandler.CatchAll = instructionReader.Lookup[(int)caOffset];
            }
        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                var staticFieldSize = reader.ReadULEB128();
                var instanceFieldSize = reader.ReadULEB128();
                var directMethodSize = reader.ReadULEB128();
                var virtualMethodSize = reader.ReadULEB128();

                ReadFieldDefinitions(reader, classDefinition, staticFieldSize);
                ReadFieldDefinitions(reader, classDefinition, instanceFieldSize);
                ReadMethodDefinitions(reader, classDefinition, directMethodSize, false);
                ReadMethodDefinitions(reader, classDefinition, virtualMethodSize, true);
            });
        }
        #endregion

        #region " References "
        private void ReadMethodReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.MethodReferencesOffset, () =>
            {
                for (var i = 0; i < Header.MethodReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int prototypeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    var mref = new MethodReference
	                {
		                Owner = (CompositeType) Dex.TypeReferences[classIndex],
						// Clone the prototype so we can annotate & update it easily
						Prototype = Dex.Prototypes[prototypeIndex].Clone(),
		                Name = Dex.Strings[nameIndex]
	                };

	                Dex.MethodReferences.Add(mref);
                }
            });
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.FieldReferencesOffset, () =>
            {
                for (var i = 0; i < Header.FieldReferencesSize; i++)
                {
                    var classIndex = reader.ReadUInt16();
                    var typeIndex = reader.ReadUInt16();
                    var nameIndex = reader.ReadInt32();

                    var fref = new FieldReference
	                {
		                Owner = (ClassReference) Dex.TypeReferences[classIndex],
		                Type = Dex.TypeReferences[typeIndex],
		                Name = Dex.Strings[nameIndex]
	                };

	                Dex.FieldReferences.Add(fref);
                }
            });
        }

        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Header.TypeReferencesOffset, () =>
            {
                for (var i = 0; i < Header.TypeReferencesSize; i++)
                {
                    var descriptorIndex = reader.ReadInt32();
                    var descriptor = Dex.Strings[descriptorIndex];
                    TypeDescriptor.Fill(descriptor, Dex.TypeReferences[i], Dex);
                }
            });
        }
        #endregion

        public void ReadFrom(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadMapList(reader);
            ReadStrings(reader);

            PrefetchTypeReferences(reader);
            PrefetchClassDefinitions(reader, false);

            ReadTypesReferences(reader);
            ReadPrototypes(reader);
            ReadFieldReferences(reader);
            ReadMethodReferences(reader);

            PrefetchClassDefinitions(reader, true);
            ReadClassDefinitions(reader);

            Dex.Classes = ClassDefinition.Hierarchicalize(Dex.Classes, Dex);
        }

    }
}
