using System.IO;

namespace Dexer.IO
{
	interface IBinaryReadable
	{
        void ReadFrom(BinaryReader reader);
	}
}
