using pkuManager.Common;
using pkuManager.pku;

namespace pkuManager.pkx.pk3
{
    public class pk3Importer : Importer
    {
        public pk3Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(file, globalFlags, checkInMode) { }

        public override bool canImport()
        {
            if (file?.Length != 80 && file?.Length != 100) //80 (+20) bytes makes a pokemon...
                return false;

            // Note that pk3Importer ignores cheksum mismatches (i.e. "Bad Eggs" are allowed)

            return true;
        }

        protected override void processAlerts()
        {

        }

        protected override pkuObject toPKU()
        {
            return new pkuObject();
        }
    }
}
