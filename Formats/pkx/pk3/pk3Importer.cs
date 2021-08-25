using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;

namespace pkuManager.Formats.pkx.pk3
{
    public class pk3Importer : Importer
    {
        public pk3Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(file, globalFlags, checkInMode) { }

        public override bool canImport()
        {
            if (file?.Length != pk3Object.FILE_SIZE_PC && file?.Length != pk3Object.FILE_SIZE_PARTY) //80 (+20) bytes makes a pokemon...
                return false;

            // Note that pk3Importer ignores cheksum mismatches (i.e. "Bad Eggs" are recovered)

            return true;
        }

        protected override void processAlerts()
        {

        }

        protected override pkuObject toPKU()
        {
            ByteArrayManipulator bam = new(file, false);

            pkuObject pku = new()
            {
                Game_Info = new pkuObject.Game_Info_Class(),
                Catch_Info = new pkuObject.Catch_Info_Class(),
                IVs = new pkuObject.IVs_Class(),
                EVs = new pkuObject.EVs_Class(),
                Trash_Bytes = new pkuObject.Trash_Bytes_Class()
            };

            pku.PID = bam.GetUInt(0); // PID: bytes 0-3
            pku.Game_Info.ID = bam.GetUInt(4); // ID: bytes 4-7

            //allow explicit language values (maybe a generic helper/wrapper method). ADD TONULLABLESTRING to datautil extension
            pku.Game_Info.Language = pk3Object.DecodeLanguage(bam.GetUShort(18)).ToString(); // Language: bytes 18-19



            // same problem with species, item, move, met location, game of origin, pokeball (index 0), 
            // maybe make unused bytes for gen 3 ribbons bye bits 27-30 and unknown 2 bytes...

            return pku;
        }


    }
}
