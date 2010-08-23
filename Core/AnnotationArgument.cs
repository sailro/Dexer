using Dexer.Metadata;

namespace Dexer.Core
{
    public class AnnotationArgument
    {
        public string Name { get; set; }
        public ValueFormats Format { get; set; }
        public byte[] Value { get; set; }
    }
}
