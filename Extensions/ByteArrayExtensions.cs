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

using System.Security.Cryptography;
using System.Text;

namespace Dexer.Extensions
{
    public static class ByteArrayExtensions
    {

        public static string ToHexString(this byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString();
        }

        public static bool Match(this byte[] array, byte[] item, int offset)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (i >= array.Length || (array[i + offset] != item[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static int IndexOf(this byte[] array, byte[] item)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (Match(array, item, i))
                {
                    return i;
                }
            }
            return -1;
        }


    }
}
