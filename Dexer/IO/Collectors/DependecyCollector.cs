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

using Dexer.Core;

namespace Dexer.IO.Collector
{
    internal class DependencyCollector: BaseCollector<ClassDefinition>
    {

        public override void Collect(TypeReference tref)
        {
            if (tref is ClassDefinition)
            {
                ClassDefinition @class = tref as ClassDefinition;
                if (!Items.ContainsKey(@class))
                    Items.Add(@class, 0);

                Items[@class]++;
            }
        }

        public override void Collect(ClassDefinition @class)
        {
            Collect(@class.InnerClasses);
            Collect(@class.Interfaces);
            Collect(@class.SuperClass);
            Collect(@class as ClassReference);
        }


    }
}
