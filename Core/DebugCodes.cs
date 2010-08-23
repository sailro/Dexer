namespace Dexer.Core
{
    public enum DebugCodes
    {
        EndSequence = 0x00,
        AdvancePc = 0x01,
        AdvanceLine = 0x02,
        StartLocal = 0x03,
        StartLocalExtended = 0x04,
        EndLocal = 0x05,
        RestartLocal = 0x06,
        SetPrologueEnd = 0x07,
        SetEpilogueBegin = 0x08,
        SetFile = 0x09,
        Firstpecial = 0x0a
    }
}
