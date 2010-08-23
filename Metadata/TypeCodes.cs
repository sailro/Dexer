namespace Dexer.Metadata
{
    public enum TypeCodes
    {
        Header = 0x0000,
        StringId = 0x0001,
        TypeId = 0x0002,
        ProtoId = 0x0003,
        FieldId = 0x0004,
        MethodId = 0x0005,
        ClassDef = 0x0006,
        MapList = 0x1000,
        TypeList = 0x1001,
        AnnotationSetRefList = 0x1002,
        AnnotationSet = 0x1003,
        ClassData = 0x2000,
        Code = 0x2001,
        StringData = 0x2002,
        DebugInfo = 0x2003,
        Annotation = 0x2004,
        EncodedArray = 0x2005,
        AnnotationDirectory = 0x2006
    }
}
