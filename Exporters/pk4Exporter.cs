using pkuManager.Alerts;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pkuManager.Exporters
{
    public class pk4Exporter : Exporter
    {
        public pk4Exporter(PKUObject pku) : base(pku) { }

        public override string formatName { get { return "Gen 4"; } }

        public override string formatExtension { get { return "pk4"; } }

        public override bool canExport()
        {
            int? natDex = pkCommons.GetNationalDex(pku);
            if (natDex.HasValue && natDex <= 493) //Arceus (493) and below
                return true;
            return false;
        }

        protected override void processAlerts()
        {
            throw new System.Exception("pk4 toFile not implemented...");
        }

        protected override byte[] toFile()
        {
            throw new System.Exception("pk4 toFile not implemented...");
        }
    }
}
