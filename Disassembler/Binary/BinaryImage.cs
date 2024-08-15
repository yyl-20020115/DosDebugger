using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disassembler;

public class BinaryImage : BinaryBaseImage
{
    public override ArraySegment<byte> GetBytes(Address address, int count)
    {
        throw new NotImplementedException();
    }
}
