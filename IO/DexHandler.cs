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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.Instructions;
using Dexer.Metadata;

namespace Dexer.IO
{
    public class DexHandler : IBinaryReadable
    {
        private Dex Dex { get; set; }

        public DexHandler(Dex dex)
        {
            Dex = dex;
        }

        public void ReadFrom(BinaryReader reader)
        {
            new DexHeaderHandler(Dex.Header).ReadFrom(reader);
            new MapHandler(Dex).ReadFrom(reader);
            new StringHandler(Dex).ReadFrom(reader);

            PrefetchTypeReferences(reader);
            PrefetchClassDefinitions(reader, false);

            ReadTypesReferences(reader);
            ReadPrototypes(reader);
            ReadFieldReferences(reader);
            ReadMethodReferences(reader);

            PrefetchClassDefinitions(reader, true);
            ReadClassDefinitions(reader);

            Hierarchicalize();
        }

        private void Hierarchicalize()
        {
            var TopClasses = new List<ClassDefinition>();
            foreach (var cdef in Dex.Classes)
            {
                if (cdef.Fullname.Contains(DexConsts.InnerClassMarker.ToString()))
                {
                    String[] items = cdef.Fullname.Split(DexConsts.InnerClassMarker);
                    string fullname = items[0];
                    string name = items[1];
                    ClassDefinition owner = Dex.GetClass(fullname);
                    if (owner != null)
                    {
                        owner.InnerClasses.Add(cdef);
                        cdef.Owner = owner;
                    }
                }
                else
                {
                    TopClasses.Add(cdef);
                }
            }
            Dex.Classes = TopClasses;
        }

        #region " Prefetch "
        private void PrefetchTypeReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.TypeReferencesOffset, () =>
            {
                reader.BaseStream.Seek(Dex.Header.TypeReferencesOffset, SeekOrigin.Begin);

                for (int i = 0; i < Dex.Header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Dex.Strings[descriptorIndex];
                    Dex.TypeReferences.Add(TypeDescriptor.Allocate(descriptor));
                }

            });
        }

        private void PrefetchFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            int fieldIndex = 0;
            for (int i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                {
                    fieldIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    fieldIndex += (int)reader.ReadULEB128();
                }
                reader.ReadULEB128();
                FieldDefinition fdef = new FieldDefinition(Dex.FieldReferences[fieldIndex]);
                Dex.FieldReferences[fieldIndex] = fdef;
            }
        }

        private void PrefetchMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount)
        {
            int methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                if (i == 0)
                {
                    methodIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    methodIndex += (int)reader.ReadULEB128();
                }
                reader.ReadULEB128();
                reader.ReadULEB128();
                MethodDefinition mdef = new MethodDefinition(Dex.MethodReferences[methodIndex]);
                Dex.MethodReferences[methodIndex] = mdef;
            }
        }

        private void PrefetchClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                PrefetchFieldDefinitions(reader, classDefinition, staticFieldSize);
                PrefetchFieldDefinitions(reader, classDefinition, instanceFieldSize);
                PrefetchMethodDefinitions(reader, classDefinition, directMethodSize);
                PrefetchMethodDefinitions(reader, classDefinition, virtualMethodSize);
            });
        }

        private void PrefetchClassDefinitions(BinaryReader reader, bool prefetchMembers)
        {
            reader.PreserveCurrentPosition(Dex.Header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < Dex.Header.ClassDefinitionsSize; i++)
                {
                    int classIndex = reader.ReadInt32();

                    ClassDefinition cdef;
                    if (Dex.TypeReferences[classIndex] is ClassDefinition)
                    {
                        cdef = (ClassDefinition)Dex.TypeReferences[classIndex];
                    }
                    else
                    {
                        cdef = new ClassDefinition((ClassReference)Dex.TypeReferences[classIndex]);
                        Dex.TypeReferences[classIndex] = cdef;
                        Dex.Classes.Add(cdef);
                    }

                    reader.ReadInt32(); // skip access_flags
                    reader.ReadInt32(); // skip superclass_idx
                    reader.ReadInt32(); // skip interfaces_off
                    reader.ReadInt32(); // skip source_file_idx
                    reader.ReadInt32(); // skip annotations_off

                    uint classDataOffset = reader.ReadUInt32();
                    if ((classDataOffset > 0) && prefetchMembers)
                    {
                        PrefetchClassDefinition(reader, cdef, classDataOffset);
                    }

                    reader.ReadInt32(); // skip static_values_off
                }
            });
        }
        #endregion

        #region " Read "
        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < Dex.Header.ClassDefinitionsSize; i++)
                {
                    uint classIndex = reader.ReadUInt32();

                    ClassDefinition cdef = (ClassDefinition)Dex.TypeReferences[(int)classIndex];
                    cdef.AccessFlag = (AccessFlags)reader.ReadUInt32();

                    uint superClassIndex = reader.ReadUInt32();
                    if (superClassIndex != DexConsts.NoIndex)
                        cdef.SuperClass = (ClassReference)Dex.TypeReferences[(int)superClassIndex];

                    uint interfaceOffset = reader.ReadUInt32();
                    uint sourceFileIndex = reader.ReadUInt32();
                    uint annotationOffset = reader.ReadUInt32();
                    uint classDataOffset = reader.ReadUInt32();
                    uint staticValuesOffset = reader.ReadUInt32();

                    if (interfaceOffset > 0)
                        ReadInterfaces(reader, cdef, interfaceOffset);

                    if (sourceFileIndex != DexConsts.NoIndex)
                        cdef.SourceFile = Dex.Strings[(int)sourceFileIndex];

                    if (classDataOffset > 0)
                        ReadClassDefinition(reader, cdef, classDataOffset);

                    if (annotationOffset > 0)
                        ReadAnnotationDirectory(reader, cdef, annotationOffset);

                    if (staticValuesOffset > 0)
                    {
                        ReadStaticValues(reader, cdef, staticValuesOffset);
                    }
                }
            });
        }

        private void ReadStaticValues(BinaryReader reader, ClassDefinition classDefinition, uint staticValuesOffset)
        {
            reader.PreserveCurrentPosition(staticValuesOffset, () =>
            {
                object[] values = ReadValues(reader);
                for (int j = 0; j < values.Length; j++)
                {
                    classDefinition.Fields[j].Value = values[j];
                }
            });
        }

        private void ReadInterfaces(BinaryReader reader, ClassDefinition classDefinition, uint interfaceOffset)
        {
            reader.PreserveCurrentPosition(interfaceOffset, () =>
            {
                int size = reader.ReadInt32();
                ushort index = reader.ReadUInt16();
                classDefinition.Interfaces.Add((ClassReference)Dex.TypeReferences[index]);
            });
        }

        private void ReadAnnotationDirectory(BinaryReader reader, ClassDefinition classDefinition, uint annotationOffset)
        {
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint classAnnotationOffset = reader.ReadUInt32();
                uint annotatedFieldsSize = reader.ReadUInt32();
                uint annotatedMethodsSize = reader.ReadUInt32();
                uint annotatedParametersSize = reader.ReadUInt32();

                if (classAnnotationOffset > 0)
                    classDefinition.Annotations = ReadAnnotationSet(reader, classAnnotationOffset);

                if (annotatedFieldsSize > 0)
                    (Dex.FieldReferences[reader.ReadInt32()] as FieldDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                if (annotatedMethodsSize > 0)
                    (Dex.MethodReferences[reader.ReadInt32()] as MethodDefinition).Annotations = ReadAnnotationSet(reader, reader.ReadUInt32());

                if (annotatedParametersSize > 0)
                {
                    int methodIndex = reader.ReadInt32();
                    uint offset = reader.ReadUInt32();
                    var annotations = ReadAnnotationSetRefList(reader, offset);
                    MethodDefinition mdef = (Dex.MethodReferences[methodIndex] as MethodDefinition);

                    for (int i = 0; i < annotations.Count; i++)
                    {
                        if (annotations[i].Count > 0)
                        {
                            mdef.Prototype.Parameters[i].Annotations = annotations[i];
                        }
                    }
                }
            });
        }

        private IList<IList<Annotation>> ReadAnnotationSetRefList(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<IList<Annotation>>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
                    result.Add(ReadAnnotationSet(reader, offset));
                }
            });
            return result;
        }

        private IList<Annotation> ReadAnnotationSet(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<Annotation>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
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
                byte visibility = reader.ReadByte();
                annotation = ReadEncodedAnnotation(reader);
                annotation.Visibility = (AnnotationVisibility)visibility;
            });
            return annotation;
        }

        private Annotation ReadEncodedAnnotation(BinaryReader reader)
        {
            int typeIndex = (int) reader.ReadULEB128();
            int elementSize = (int) reader.ReadULEB128();

            Annotation annotation = new Annotation();
            annotation.Type = (ClassReference) Dex.TypeReferences[typeIndex];

            for (int i=0; i<elementSize; i++) {
                AnnotationArgument argument = new AnnotationArgument();
                int nameIndex = (int) reader.ReadULEB128();
                argument.Name = Dex.Strings[nameIndex];
                argument.Value = ReadValue(reader);
                annotation.Arguments.Add(argument);
            }

            return annotation;
        }

        private object[] ReadValues(BinaryReader reader)
        {
            uint size = reader.ReadULEB128();
            ArrayList array = new ArrayList();
            for (uint i = 0; i < size; i++)
            {
                array.Add(ReadValue(reader));
            }
            return array.ToArray();
        }

        private object ReadValue(BinaryReader reader)
        {
            int data = reader.ReadByte();
            int valueFormat = data & 0x1F;
            int valueArgument = data >> 5;

            switch ((ValueFormats)valueFormat)
            {
                case ValueFormats.Byte:
                    return reader.ReadSByte();
                case ValueFormats.Short:
                    return (short) reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Char:
                    return (char)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Int:
                    return (int)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Long:
                    return (long)reader.ReadValueByTypeArgument(valueArgument);
                case ValueFormats.Float:
                    return reader.ReadSingle();
                case ValueFormats.Double:
                    return reader.ReadDouble();
                case ValueFormats.String:
                    return Dex.Strings[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Type:
                    return Dex.TypeReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Field:
                case ValueFormats.Enum:       
                    return Dex.FieldReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Method:
                    return Dex.MethodReferences[(int)reader.ReadValueByTypeArgument(valueArgument)];
                case ValueFormats.Array:
                    return ReadValues(reader);
                case ValueFormats.Annotation:
                    return ReadEncodedAnnotation(reader);
                case ValueFormats.Null:
                    return null;
                case ValueFormats.Boolean:
                    return reader.ReadBoolean();
                default:
                    throw new ArgumentException();
            }
        }

        private void ReadFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            int fieldIndex=0;
            for (int i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                {
                    fieldIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    fieldIndex += (int)reader.ReadULEB128();
                }

                uint accessFlags = reader.ReadULEB128();

                FieldDefinition fdef = (FieldDefinition)Dex.FieldReferences[fieldIndex];
                fdef.AccessFlags = (AccessFlags)accessFlags;
                fdef.Owner = classDefinition;

                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount)
        {
            int methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                if (i == 0)
                {
                    methodIndex = (int)reader.ReadULEB128();
                }
                else
                {
                    methodIndex += (int)reader.ReadULEB128();
                }

                uint accessFlags = reader.ReadULEB128();
                uint codeOffset = reader.ReadULEB128();

                MethodDefinition mdef = (MethodDefinition)Dex.MethodReferences[methodIndex];
                mdef.AccessFlags = (AccessFlags)accessFlags;
                mdef.Owner = classDefinition;

                classDefinition.Methods.Add(mdef);

                if (codeOffset > 0)
                    ReadMethodBody(reader, mdef, codeOffset);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition methodDefinition, uint codeOffset)
        {
            reader.PreserveCurrentPosition(codeOffset, () =>
            {
                ushort registersSize = reader.ReadUInt16();
                ushort incomingArgsSize = reader.ReadUInt16();
                ushort outgoingArgsSize = reader.ReadUInt16();
                ushort triesSize = reader.ReadUInt16();
                uint debugOffset = reader.ReadUInt32();

                methodDefinition.Body = new MethodBody(registersSize);
                InstructionHandler ihandler = new InstructionHandler(Dex, methodDefinition);
                ihandler.ReadFrom(reader);

                if ((triesSize != 0) && (ihandler.Codes.Length % 2 != 0))
                    reader.ReadUInt16(); // padding (4-byte alignment)

                if (triesSize != 0)
                {
                    for (int i = 0; i < triesSize; i++)
                    {
                        uint startOffset = reader.ReadUInt32();
                        uint insCount = reader.ReadUInt16();
                        uint endOffset = startOffset + insCount;
                        uint handlerOffset = reader.ReadUInt16();
                        
                        ExceptionHandler handler = new ExceptionHandler();
                        methodDefinition.Body.Exceptions.Add(handler);
                        //handler.TryStart = ihandler.Lookup[(int)startOffset];
                        //handler.TryEnd = ihandler.Lookup[(int)endOffset];

                        // TODO handlers
                    }
                }


            });

        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                ReadFieldDefinitions(reader, classDefinition, staticFieldSize);
                ReadFieldDefinitions(reader, classDefinition, instanceFieldSize);
                ReadMethodDefinitions(reader, classDefinition, directMethodSize);
                ReadMethodDefinitions(reader, classDefinition, virtualMethodSize);
            });
        }

        private void ReadMethodReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.MethodReferencesOffset, () =>
            {
                for (int i = 0; i < Dex.Header.MethodReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int prototypeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    MethodReference mref = new MethodReference();
                    mref.Owner = (ClassReference)Dex.TypeReferences[classIndex];
                    mref.Prototype = Dex.Prototypes[prototypeIndex];
                    mref.Name = Dex.Strings[nameIndex];

                    Dex.MethodReferences.Add(mref);
                }
            });
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.FieldReferencesOffset, () =>
            {
                for (int i = 0; i < Dex.Header.FieldReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int typeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    FieldReference fref = new FieldReference();

                    fref.Owner = (ClassReference)Dex.TypeReferences[classIndex];
                    fref.Type = Dex.TypeReferences[typeIndex];
                    fref.Name = Dex.Strings[nameIndex];

                    Dex.FieldReferences.Add(fref);
                }
            });
        }

        private void ReadPrototypes(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.PrototypesOffset, () =>
            {
                for (int i = 0; i < Dex.Header.PrototypesSize; i++)
                {
                    int shortyIndex = reader.ReadInt32();
                    int returnTypeIndex = reader.ReadInt32();
                    uint parametersOffset = reader.ReadUInt32();

                    Prototype prototype = new Prototype();
                    prototype.ReturnType = Dex.TypeReferences[returnTypeIndex];

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
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    Parameter parameter = new Parameter();
                    int typeIndex = reader.ReadUInt16();
                    parameter.Type = Dex.TypeReferences[typeIndex];
                    prototype.Parameters.Add(parameter);
                }
            });
        }
        
        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.TypeReferencesOffset, () =>
            {
                for (int i = 0; i < Dex.Header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = Dex.Strings[descriptorIndex];
                    TypeDescriptor.Fill(descriptor, Dex.TypeReferences[i], Dex);
                }
            });
        }
        #endregion

    }
}
