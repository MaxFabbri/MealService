using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServizioMensaInterattivo
{
    static class Utils
    {
        internal static string RINI(string Section, string Key, string ini="")
        {
            // se non specificato si riferisce al nome del programmma
            if (ini == "")
                ini = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini";

            return InteropObject.RP.GetSetting(ini, Section, Key, "");
        }

        internal static string DateToSQL(DateTime Date, Boolean RecordsetUse = false)
        {
            if (Date.GetMinutes() > 0)
                return RecordsetUse ? Date.ToString(@"#MM/dd/yyyy hh.mm.ss#") : Date.ToString(@"#dd/MM/yyyy hh.mm.ss#");
            else
                return RecordsetUse ? Date.ToString(@"#MM/dd/yyyy#") : Date.ToString(@"#dd/MM/yyyy#");
        }


    }
}
