using pkuManager.Common;
using System;

namespace pkuManager.Formats.pkx.pk3
{
    public class pk3Importer : Importer
    {
        protected override Type DataType { get => typeof(pk3Object); }

        /// <summary>
        /// <see cref="Exporter.Data"/> casted as a <see cref="pk3Object"/>.
        /// </summary>
        protected pk3Object pk3 { get => Data as pk3Object; }

        public pk3Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(file, globalFlags, checkInMode) { }

        public override (bool, string) CanImport()
        {
            //do species check here

            // Note that pk3Importer ignores cheksum mismatches (i.e. "Bad Eggs" are recovered)
            return (true, null);
        }
    }
}
