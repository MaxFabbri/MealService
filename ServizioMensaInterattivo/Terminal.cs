using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServizioMensaInterattivo
{
    class Terminal
    {
        // Stato della sequenza
        public Constants.StateSeq StateSeq { get; set; }
        //Indirizzo Terminale
        public int IndirTerm { get; set; }
        //Tipologia marcatura (0=Prenotazione,1=Consumazione)
        //dipende dal nome transazione (RESER=Prenotazione,MENSA=Consumazione)
        public byte Tipo{ get; set; }
        //BITMAP - Giorno della settimana della Prenotazione (valido solo per le prenotazioni)
        public byte GiornoPrenotaz{ get; set; }
        //Data Prenotazione (valido solo per le prenotazioni)
        public DateTime DataPrenotaz { get; set; }
        // NO BITMAP - Tipologia pasto (Pranzo,Cena,Colaz.)
        public int TipoPasto { get; set; }
        //BITMAP - Tipologia menu (Pasto completo,Piatto unico,Cestino)
        public byte TipoMenu { get; set; }
        //Numero di form Pietanze
        public int NumFormPietanze { get; set; }
        //Numero di form Pietanze gia' richiesti
        public int CurrNumFormPietanze { get; set; }
        //Elenco Pietanze del form corrente
        private string[] currPietanze = new string[16];
        public string[] CurrPietanze
        {
            get { return currPietanze; }
            set { currPietanze = value; }
        }
        //Numero di Pietanze presenti nel form corrente
        public int CurrNumPietanze { get; set; }

    }
}
