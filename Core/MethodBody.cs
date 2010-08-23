using System.Collections.Generic;

namespace Dexer.Core
{
	public class MethodBody
	{
        public ushort RegistersSize { get; set; }
        public DebugInfo DebugInfo { get; set; }
        public IEnumerable<Instruction> Instructions { get; set; }
		public IEnumerable<ExceptionHandler> Exceptions { get; set; }
	}
}
