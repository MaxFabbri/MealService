using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServizioMensaInterattivo
{
    static class Constants
    {
        internal enum StateSeq
        {
            SS_WRONGSEQ = -1,
            SS_ID = 0,
            SS_SELWEEKDAY = 1,
            SS_DATE = 2,
            SS_MEALTYPE = 3,
            SS_MENUTYPE = 4,
            SS_DISHES = 5
        };

        internal const string TRANNAME_PRENOTAZIONE = "RESER";
        internal const string TRANNAME_CONSUMAZIONE = "MENSA";

        internal const int T_PRENOTAZIONE = 0;
        internal const int T_CONSUMAZIONE = 1;

        internal const int G_UNKNOWN = 0;
        internal const int G_LUNEDI = 1;
        internal const int G_MARTEDI = 2;
        internal const int G_MERCOLEDI = 3;
        internal const int G_GIOVEDI = 4;
        internal const int G_VENERDI = 5;
        internal const int G_SABATO = 6;
        internal const int G_DOMENICA = 7;

        internal const string GS_UNKNOWN = "0";
        internal const string GS_LUNEDI = "1";
        internal const string GS_MARTEDI = "2";
        internal const string GS_MERCOLEDI = "3";
        internal const string GS_GIOVEDI = "4";
        internal const string GS_VENERDI = "5";
        internal const string GS_SABATO = "6";
        internal const string GS_DOMENICA = "7";

        internal const string G_OGGI = "O";
        internal const string G_DOMANI = "D";
        internal const string G_ALTRO = "A";

        //internal const int COUNT_TIPOPASTO = 3;

        //internal enum TipoPasto
        //{
        //    TP_UNKNOWN = 0,
        //    TP_PRANZO = 1,
        //    TP_CENA = 2,
        //    TP_COLAZIONE = 3
        //}

        //internal enum TipoMenu
        //{
        //    TM_UNKNOWN = 0,
        //    TM_PASTOCOMPLETO = 1,
        //    TM_PIATTOUNICO = 2,
        //    TM_CESTINO = 3
        //}


        //Numero di Form Pietanze da visualizzare per ogni Tipo Menu
        //internal const int NFORM_UNKNOWN = 0;
        //internal const int NFORM_PASTOCOMPLETO = 2;
        //internal const int NFORM_PIATTOUNICO = 1;
        //internal const int NFORM_CESTINO = 0;


    }
}
