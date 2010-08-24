namespace Dexer.Metadata
{
    public enum ValueFormats : byte
    {
        Byte = 0x00,
        Short = 0x02,
        Char = 0x03,
        Int = 0x04,
        Long = 0x06,
        Float = 0x10,
        Double = 0x11,
        String = 0x17,
        Type = 0x18,
        Field = 0x19,
        Method = 0x1a,
        Enum = 0x1b,
        Array = 0x1c,
        Annotation = 0x1d,
        Null = 0x1e,
        Boolean = 0x1f
    }
}
