
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using APInterMngTypeLib;
using System.Configuration;
using System.Windows.Forms;
using Logger;
using System.Timers;
using System.Threading;
using System.Data.OleDb;

namespace ServizioMensaInterattivo
{

    public class Interaction
    {

        // interattivo con autopro
        IInteraction newInter;

        ILog Log;
        ICatchError CatchError;

        string keyTerm;

        // legge il numero delle sequenze ed i piatti per ogni sequenza
        string[] TipoMenu;

        // tempo in millisecondi

        Stopwatch stopWatch = new Stopwatch();
        TimeSpan ts;

        Dictionary<string, Terminal> Terminals = new Dictionary<string, Terminal>();

        public bool Interrupt;

        string conn;

        // thread in cui girerà l'interattivo
        Thread checkThread;

        //PRP.IAnagConnection Conn=null;

        bool writeLog;
        bool useRecordset;
        bool sendCodeAndDescription;

        public Interaction()
        {
            var logger = Logger.Logger.GetInstance;
            Log = logger.GetLogs["InterattivoMensa"];
            CatchError = logger.GetErrorLog;
        }

        void Check()
        {

            while (!Interrupt)
            {
                try
                {

                    if (DateTime.Now.ToString("HHmmss") == "000000")
                    {
                        // a mezzanotte rinomina il file di log
                        Log.RenameFileLog(99);
                        // a mezzanotte rilascia le risorse del prp ...
                        //Trace.WriteLine("a mezzanotte rilascia le risorse del prp ...");
                        InteropObject.ReloadObj(CatchError, true, false);
                    }

                    try
                    {
                        // questo serve per capire se prp è attivo
                        Debug.Print(InteropObject.RP.LoggedMachineName);
                    }

                    catch (COMException Ex)
                    {
                        // ricarica gli oggetti COM
                        CatchError.Error(Ex, Params: new Dictionary<string, string>() { { "Ex.ErrorCode", Ex.ErrorCode.ToString() } });
                        Log.Append("Riavvio Present Remote Provider...", true, true, true,false);
                        //Trace.WriteLine("Riavvio Present Remote Provider...");
                        InteropObject.ReloadObj(CatchError,true,false);
                        Log.Append("Present Remote Provider riavviato", true, true, true,false);
                        //Trace.WriteLine("Present Remote Provider riavviato");
                    }

                    catch (Exception Ex)
                    {
                        CatchError.Error(Ex);
                        InteropObject.ReloadObj(CatchError);
                        Log.Append("Errore Present Remote Provider riavviato", true, true, true,false);
                    }

                    newInter = (IInteraction)InteropObject.Interaction.GetInteraction();

                    // dataset prova
                    //using (OleDbConnection co = new OleDbConnection(conn))
                    //{
                    //    co.Open();
                    //    co.Close();
                    //    Log.Append("co.Open()", true, true, true, false);
                    //}

                    // recordset prova
                    //ADODB.Recordset rsDip= InteropObject.RP.GetAnagConnection().OpenRecordset("SELECT * from dipendenti");
                    //rsDip = null;
                    //Log.Append("ADODB.Recordset ", true, true, true, false);

                    if (newInter != null)
                    {

                        // inizializzazione dati terminale numero keyTerm 
                        keyTerm = newInter.TermAddress.ToString();

                        switch (newInter.CheckNumber)
                        {
                            case 1: // inizio transazione mensa prenotazione/consumazione

                                //stopWatch.Start();

                                // inizializzazione dati terminale numero keyTerm 
                                //keyTerm = newInter.TermAddress.ToString();

                                if (keyTerm == null)
                                {
                                    newInter.ReplyStrings[1] = "                    ";
                                    newInter.ReplyEvent = 203;
                                    break;
                                }

                                // se esite lo elimina dall'elenco
                                if (Terminals.ContainsKey(keyTerm))
                                    Terminals.Remove(keyTerm);

                                Terminals.Add(keyTerm, new Terminal());

                                Terminals[keyTerm].IndirTerm = newInter.TermAddress;

                                if (newInter.TransactionName == Constants.TRANNAME_PRENOTAZIONE)
                                    Terminals[keyTerm].Tipo = Constants.T_PRENOTAZIONE;
                                else if (newInter.TransactionName == Constants.TRANNAME_CONSUMAZIONE)
                                    // qui non dovrebbe entrarci mai...
                                    Terminals[keyTerm].Tipo = Constants.T_CONSUMAZIONE;

                                Terminals[keyTerm].CurrNumFormPietanze = 0;
                                Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_ID;

                                //Log.Append("Inizio transazione mensa prenotazione terminale " + Terminals[keyTerm].IndirTerm);
                                //ClearStopWatch();

                                string Badge = newInter.IDValue.ToString().Trim();

                                if (Badge == null)
                                {
				                    newInter.ReplyStrings[1] = "                    ";
                                    newInter.ReplyEvent = 203;
                                    break;
                                }

                				// prosegue in ogni caso anche se non trova il badge  	
                                string Nome="", Cognome="";

                                try
                                {
                                    if (SearchName(Badge, out Nome, out Cognome))
                                    {
                                        newInter.ReplyStrings[1] = string.Format("{0,-20}", Nome.Substring(0, Math.Min(20, Nome.Length)));
                                        newInter.ReplyStrings[2] = string.Format("{0,-20}", Cognome.Substring(0, Math.Min(20, Cognome.Length)));
                                        newInter.ReplyStrings[3] = "                    ";
                                        newInter.ReplyEvent = 202;
                                    }
                                    else
                                    {
					                    for (byte i=1;i<= 3;i++)
					                    {
						                    newInter.ReplyStrings[i] = "                    ";
					                    }
                                        newInter.ReplyEvent = 202;
                                    }
                                }
                                catch
                                {
					                for (byte i=1;i<= 3;i++)
					                {
						                newInter.ReplyStrings[i] = "                    ";
					                }
                                    newInter.ReplyEvent = 202;
                                }

                                //Log.Append(Badge);

                                break;

                            case 20: // inserito giorno

                                //Log.Append("Inserito giorno");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {
                                    //stopWatch.Start();

                                    if (Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_ID)
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_SELWEEKDAY;
                                        newInter.ReplyEvent = 202;
                                    }
                                    else
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                        newInter.ReplyEvent = 203;
                                    }

                                    int id = 0;
                                    int.TryParse(newInter.IDValue, out id);

                                    switch (newInter.IDValue)
                                    {
                                        case Constants.GS_LUNEDI:
                                        case Constants.GS_MARTEDI:
                                        case Constants.GS_MERCOLEDI:
                                        case Constants.GS_GIOVEDI:
                                        case Constants.GS_VENERDI:
                                        case Constants.GS_SABATO:
                                        case Constants.GS_DOMENICA:
                                            Terminals[keyTerm].GiornoPrenotaz = GetBitmap(id);
                                            Terminals[keyTerm].DataPrenotaz = GetDateFromString(newInter.DateTime, newInter.IDValue);
                                            break;
                                        case Constants.G_OGGI:
                                            Terminals[keyTerm].GiornoPrenotaz = GetBitmap(GetWeekDay(newInter.DateTime, Constants.G_OGGI));
                                            Terminals[keyTerm].DataPrenotaz = GetDateFromString(newInter.DateTime, newInter.IDValue);
                                            break;
                                        case Constants.G_DOMANI:
                                            Terminals[keyTerm].GiornoPrenotaz = GetBitmap(GetWeekDay(newInter.DateTime, Constants.G_DOMANI));
                                            Terminals[keyTerm].DataPrenotaz = GetDateFromString(newInter.DateTime, newInter.IDValue);
                                            break;
                                        case Constants.G_ALTRO:
                                            // In questo case GiornoPrenotaz e DataPrenotaz vengono inseriti nel Case 21 delle check
                                            break;
                                        case Constants.GS_UNKNOWN:
                                            Terminals[keyTerm].GiornoPrenotaz = 0;
                                            Terminals[keyTerm].DataPrenotaz = DateTime.MinValue;
                                            break;
                                        default:
                                            Terminals[keyTerm].GiornoPrenotaz = 0;
                                            Terminals[keyTerm].DataPrenotaz = DateTime.MinValue;
                                            break;
                                    }

                                    //Log.Append(Terminals[keyTerm].DataPrenotaz.ToString());
                                    //ClearStopWatch();

                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                    newInter.ReplyEvent = 203;
                                }

                                break;

                            case 21: // inserita data

                                //Log.Append("Inserita data");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {
                                    //stopWatch.Start();

                                    if (Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_SELWEEKDAY)
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_DATE;
                                    else
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                        newInter.ReplyEvent = 203;
                                    }

                                    Terminals[keyTerm].DataPrenotaz = GetDateFromString(newInter.DateTime, Constants.G_ALTRO, newInter.IDValue);
                                    Terminals[keyTerm].GiornoPrenotaz = GetBitmap(GetWeekDay(Terminals[keyTerm].DataPrenotaz, Constants.G_ALTRO));
                                    newInter.ReplyEvent = 202;

                                    //Log.Append(Terminals[keyTerm].DataPrenotaz.ToString());
                                    //ClearStopWatch();
                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                    newInter.ReplyEvent = 203;
                                }

                                break;

                            case 30: // tipo pasto

                                //Log.Append("tipo pasto");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {
                                    //stopWatch.Start();

                                    if ((Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_SELWEEKDAY) ||
                                        (Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_DATE))
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_MEALTYPE;
                                    else
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;

                                    // non è una bitmap
                                    //Terminals[keyTerm].TipoPasto = GetBitmap(int.Parse(newInter.IDValue));
                                    Terminals[keyTerm].TipoPasto = int.Parse(newInter.IDValue);

                                    //Log.Append(Terminals[keyTerm].TipoPasto.ToString());
                                    //ClearStopWatch();
                                }
                                else
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;

                                break;

                            case 40: // tipo menu

                                //Log.Append("tipo menu");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {
                                    //stopWatch.Start();

                                    if (Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_MEALTYPE)
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_MENUTYPE;
                                    else
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;

                                    Terminals[keyTerm].TipoMenu = byte.Parse(newInter.IDValue);
                                    Terminals[keyTerm].NumFormPietanze = GetNumFormPietanzeByMenuType(int.Parse(newInter.IDValue));

                                    //Log.Append(Terminals[keyTerm].TipoMenu.ToString());

                                    //ClearStopWatch();

                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                }

                                break;

                            case 51: // richiesta pietanze da 9 a 16 

                                //Log.Append("richiesta pietanze da 9 a 16");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {

                                    if ((Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_MENUTYPE) ||
                                        (Terminals[keyTerm].StateSeq == Constants.StateSeq.SS_DISHES))
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_DISHES;
                                        Terminals[keyTerm].CurrNumFormPietanze += 1;

                                        if (RequestDishesPrenotazione(keyTerm))
                                        {
                                            if (Terminals[keyTerm].CurrNumPietanze <= 0)
                                            {
                                                newInter.ReplyEvent = 203;
                                            }
                                            else
                                            {
                                                if (Terminals[keyTerm].CurrNumPietanze > 8)
                                                {
                                                    int maxDishes = Math.Min(Terminals[keyTerm].CurrNumPietanze, 16);
                                                    for (int i = 9; i <= maxDishes ; i++)
                                                    {
                                                        string piatto = string.Format("{0,-20}", Terminals[keyTerm].CurrPietanze[i - 1].Substring(0, Math.Min(20, Terminals[keyTerm].CurrPietanze[i - 1].Length)));
                                                        newInter.ReplyStrings[(byte)(i - 8)] = piatto;
                                                        //Log.Append(piatto);
                                                    }

                                                    if (Terminals[keyTerm].CurrNumPietanze < 16)
                                                    {
                                                        newInter.ReplyStrings[(byte)((Terminals[keyTerm].CurrNumPietanze + 1) - 8)] = "                    ";
                                                    }

                                                    newInter.ReplyEvent = 202;
                                                }
                                            }

                                            //ClearStopWatch();
                                        }
                                    }
                                    else
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                        newInter.ReplyEvent = 203;
                                    }

                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                    newInter.ReplyEvent = 203;
                                }

                                break;

                            case 52: // richiesta pietanze da 1 a 8

                                //Log.Append("richiesta pietanze da 1 a 8");

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {

                                    if (Terminals[keyTerm].StateSeq != Constants.StateSeq.SS_DISHES)
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                        newInter.ReplyEvent = 203;
                                    }
                                    else
                                    {
                                        if (Terminals[keyTerm].CurrNumPietanze <= 0)
                                        {
                                            newInter.ReplyEvent = 203;
                                        }
                                        else
                                        {
                                            // spedisce solo i primi 8 piatti
                                            for (int i = 1; i <= Math.Min(Terminals[keyTerm].CurrNumPietanze, 8) ; i++)
                                            {
                                                string piatto = string.Format("{0,-20}", Terminals[keyTerm].CurrPietanze[i - 1].Substring(0, Math.Min(20, Terminals[keyTerm].CurrPietanze[i - 1].Length)));
                                                newInter.ReplyStrings[(byte)i] = piatto;
                                                //Log.Append(piatto);
                                            }

                                            if (Terminals[keyTerm].CurrNumPietanze < 8)
                                            {
                                                newInter.ReplyStrings[(byte)(Terminals[keyTerm].CurrNumPietanze + 1)] = "                    ";
                                            }

                                            newInter.ReplyEvent = 202;
                                        }
                                    }

                                    //ClearStopWatch();

                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                    newInter.ReplyEvent = 203;
                                }

                                break;

                            case 60: // richiede la data durante la stampa

                                if (keyTerm == null)
                                {
                                    break;
                                }

                                if (Terminals.ContainsKey(keyTerm))
                                {

                                    if (Terminals[keyTerm].StateSeq != Constants.StateSeq.SS_DISHES)
                                    {
                                        Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                        newInter.ReplyEvent = 203;
                                    }
                                    else
                                    {
                                        // spedisce la data risolta al terminale
                                        newInter.ReplyStrings[1] = string.Format("{0,-20}", Terminals[keyTerm].DataPrenotaz.Date.ToString("dd/MM/yyyy"));
                                        newInter.ReplyStrings[2] = "                    ";
                                        newInter.ReplyEvent = 202;
                                    }

                                }
                                else
                                {
                                    Terminals[keyTerm].StateSeq = Constants.StateSeq.SS_WRONGSEQ;
                                    newInter.ReplyEvent = 203;
                                }

                                break;
                        }

                        newInter.SendReply();

                    }

                }

                catch (Exception ex)
                {
                    CatchError.Error(ex);
                }
            
            }

        }

        private bool SearchName(string Badge, out string Nome,out string Cognome)
        {

            Nome = "";
            Cognome = "";
            bool state = false;

            string sql = "SELECT Nome, Cognome\r\n";
            sql += "FROM Dipendenti\r\n";

            if (!useRecordset)
            {
                sql += "WHERE Badge = @Badge";
            }
            else
            {
                sql += "WHERE Badge = '" + Badge + "'";
            }

            if (!useRecordset)
            {
                using (OleDbConnection co = new OleDbConnection(conn))
                {
                    co.Open();
                    OleDbCommand cmd = new OleDbCommand(sql, co);
                    cmd.Parameters.AddWithValue("@Badge", Badge);
                    OleDbDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        Nome = reader["Nome"].ToString();
                        Cognome = reader["Cognome"].ToString();
                        reader.Close();
                        state = true;
                    }
                }

            }
            else 
            { 
                // recordset
                ADODB.Recordset rsDip = InteropObject.RP.GetAnagConnection().OpenRecordset(sql);
                if (!rsDip.EOF)
                {
                    Nome = rsDip.Fields["Nome"].Value.ToString();
                    Cognome = rsDip.Fields["Cognome"].Value.ToString();
                    rsDip = null;
                    state = true;
                }

            }

            return state;
        
        }

        private void ClearStopWatch()
        {
            if (writeLog)
            {
                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                Log.Append("Transazione conclusa in ms. " + ts.Milliseconds);
                stopWatch.Reset();
            }

        }


        private bool RequestDishesPrenotazione(string key)
        {

            try{

                stopWatch.Start();
                string category="0";
                string[] cat = TipoMenu[Terminals[key].TipoMenu - 1].Split(',');

                if (Terminals[key].CurrNumFormPietanze <= cat.Length)
                {
                    category = cat[Terminals[key].CurrNumFormPietanze - 1];
                }

                string sql = "";
                if (sendCodeAndDescription)
                {
                    sql = "SELECT Course, Dishes.Dishcode + ' ' + Dishes.Description AS Description\r\n";
                }
                else
                {
                    sql = "SELECT Course, Dishes.Description AS Description\r\n";
                }

                sql += "FROM CoursePlanning INNER JOIN Dishes\r\n" +
                        "ON CoursePlanning.DishID = Dishes.DishID\r\n";

                if (useRecordset)
                {
                    sql += "WHERE CoursePlanning.PlanDate = " + Utils.DateToSQL(Terminals[key].DataPrenotaz.Date, true);
                }
                else
                {
                    sql += "WHERE CoursePlanning.PlanDate = @Data";
                }

                    //" AND ((CoursePlanning.MealTypes & " + Terminals[key].TipoPasto + ") <> 0)" +
                sql+=" AND (CoursePlanning.MealTypes = " + Terminals[key].TipoPasto + ")" +
                     " AND CoursePlanning.MenuType = " + Terminals[key].TipoMenu +
                     " AND Dishes.Category = " + category;
                    //" ORDER BY Course";
                    //(2 ^  bit  - 1 " ) Mod 2) <> 0)

                int numPiet = 0;

                if (!useRecordset)
                {
                    // dataset
                    using (OleDbConnection co = new OleDbConnection(conn))
                    {
                        co.Open();

                        OleDbCommand cmd = new OleDbCommand(sql, co);
                        cmd.Parameters.AddWithValue("@Data", Terminals[key].DataPrenotaz.Date);
                        OleDbDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (int.Parse(reader["Course"].ToString()) >= 1 && int.Parse(reader["Course"].ToString()) <= 16)
                                {
                                    Terminals[key].CurrPietanze[int.Parse(reader["Course"].ToString()) - 1] = reader["Description"].ToString();
                                    numPiet++;
                                }
                            }

                            reader.Dispose();

                            if (writeLog)
                            {
                                Log.Append(string.Format("prenotazione in data {0} query {1} Totale righe lette = {2}", Terminals[key].DataPrenotaz.Date, sql.Replace("\r\n"," "), numPiet.ToString()));
                            }

                            Terminals[key].CurrNumPietanze = numPiet;

                            ClearStopWatch();
                            return true;
                        }
                        else
                        {
                            reader.Dispose();
                            Terminals[key].CurrNumPietanze = 0;
                            if (writeLog)
                            {
                                Log.Append("0");
                            }
                            ClearStopWatch();
                            return false;
                        }
                    }
                }
                else
                {

                    // recordset
                    ADODB.Recordset rsPietanze = InteropObject.RP.GetAnagConnection().OpenRecordset(sql);

                    //DebugRecordSet(rsPietanze);

                    if (rsPietanze.EOF)
                    {
                        Terminals[key].CurrNumPietanze = 0;
                        if (writeLog)
                        {
                            Log.Append("0");
                        }
                        ClearStopWatch();
                        return false;
                    }

                    while (!rsPietanze.EOF)
                    {
                        if (int.Parse(rsPietanze.Fields["Course"].Value.ToString()) >= 1 && int.Parse(rsPietanze.Fields["Course"].Value.ToString()) <= 16)
                        {
                            Terminals[key].CurrPietanze[int.Parse(rsPietanze.Fields["Course"].Value.ToString())-1] = rsPietanze.Fields["Description"].Value.ToString();
                            numPiet ++;
                        }
                        rsPietanze.MoveNext();
                    }

                    rsPietanze = null;

                    if (writeLog)
                    {
                        Log.Append(string.Format("query {0} Totale righe lette = {1}", sql.Replace("\r\n"," "), numPiet.ToString()));
                    }
                    Terminals[key].CurrNumPietanze = numPiet;

                    ClearStopWatch();

                    return true;
                }

            }

            catch{

                Terminals[key].CurrNumPietanze = 0;
                return false;

            }
    
        }

        public void StartProcess()
        {

            try
            {

                // connessione al database
                conn = @System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ToString();
                // Scrive le query effettuate 
                bool.TryParse(@System.Configuration.ConfigurationManager.AppSettings["WriteLog"], out writeLog);
                // usa alla vecchissima il recordset
                bool.TryParse(@System.Configuration.ConfigurationManager.AppSettings["UseRecordset"], out useRecordset);
                // indica se spedire il codice e la descrizione o solo la descrizione
                bool.TryParse(@System.Configuration.ConfigurationManager.AppSettings["SendCodeAndDescription"], out sendCodeAndDescription);

                InteropObject.ReloadObj(CatchError);

                //Conn = (PRP.IAnagConnection)InteropObject.RP.GetAnagConnection();

                if (InteropObject.RP == null)
                {
                    Log.Append("Riferimento a Present Remote Provider fallito", true, true, true);
                }
                else
                {

                    //GLog.Logs["InterattivoMensa"].Append("Present - Servizio mensa interattivo avviato", true, true, true, false);
                    Log.Append("Present - Servizio mensa interattivo avviato", true, true, true, false);

                    if (Utils.RINI("MenuTypes", "Number", "WPMeals.ini") != "")
                    {

                        TipoMenu = new string[int.Parse(Utils.RINI("MenuTypes", "Number", "WPMeals.ini"))];

                        Log.Append("Totale piatti letti:");
                        Log.Append(TipoMenu.Length.ToString());

                        for (int i = 1; i <= TipoMenu.Length; i++)
                        {
                            // elimino la descrizione nella prima posizione
                            string t = Utils.RINI("MenuTypes", i.ToString(), "WPMeals.ini");
                            var cat = t.Split(',');
                            var catClone = cat.SubArray(1, cat.Length - 1);
                            TipoMenu[i - 1] = string.Join(",", catClone);
                            if (writeLog)
                            {
                                Log.Append(TipoMenu[i - 1].ToString());
                            }
                            
                        }

                        // parte il thread
                        checkThread = new Thread(this.Check);
                        checkThread.Start();

                    }
                    else
                    {
                        Log.Append("ATTENZIONE: WPMeals.ini MenuTypes Number non configurato", true, true, true);
                        //Trace.WriteLine("ATTENZIONE: WPMeals.ini MenuTypes Number non configurato");
                    }

                    Log.Append(string.Format("Connessione dati {0}",conn));
                    
                }
            }

            catch (Exception ex)
            {
                CatchError.Error(ex);
            }

        }

        public void EndProcess()
        {

            
            //Conn = null;
            Interrupt = true;
            // attende la fine del thread e termina
            if (checkThread != null)
            {
                checkThread.Join();
                checkThread = null;
            }
            InteropObject.ReleaseObj();
        }

        private byte GetBitmap(int value)
        {
            try
            {
                if ((value > 0) && (value < 7))
                {
                    return (byte)Math.Pow(2, value - 1);
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }


        private DateTime GetDateFromString(DateTime DataMarc, string GiornoPren, string DataPren = "")
        {

            try
            {
                switch (GiornoPren)
                {
                    case Constants.GS_LUNEDI:
                    case Constants.GS_MARTEDI:
                    case Constants.GS_MERCOLEDI:
                    case Constants.GS_GIOVEDI:
                    case Constants.GS_VENERDI:
                    case Constants.GS_SABATO:
                    case Constants.GS_DOMENICA:
                        int day = ((int)DataMarc.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek;
                        int delta = int.Parse(GiornoPren) - day;
                        if (delta >= 0)
                            return DataMarc.AddDays((double)delta);
                        else
                            return DataMarc.AddDays(7 + (double)delta);
                    case Constants.G_OGGI:
                        return DataMarc;
                    case Constants.G_DOMANI:
                        return DataMarc.AddDays(1);
                    case Constants.G_ALTRO:
                        //Altro. La data è nel formato DDMMYY
                        return new DateTime(2000 + int.Parse(DataPren.Substring(6, 2)), int.Parse(DataPren.Substring(3, 2)), int.Parse(DataPren.Substring(0, 2)));
                    default:
                        return DateTime.MinValue;

                }
            }
            catch {
                return DateTime.MinValue;
            }
        }

        private int GetWeekDay(DateTime Data, string Info)
        {
            try
            {
                DayOfWeek tDay;
                switch (Info)
                {
                    case Constants.G_OGGI:
                    case Constants.G_ALTRO:
                        tDay = Data.DayOfWeek;
                        break;
                    case Constants.G_DOMANI:
                        tDay = Data.AddDays(1).DayOfWeek;
                        break;
                    default:
                        tDay = 0;
                        break;
                }
                switch (tDay)
                {
                    case DayOfWeek.Monday:
                        return Constants.G_LUNEDI;
                    case DayOfWeek.Tuesday:
                        return Constants.G_MARTEDI;
                    case DayOfWeek.Wednesday:
                        return Constants.G_MERCOLEDI;
                    case DayOfWeek.Thursday:
                        return Constants.G_GIOVEDI;
                    case DayOfWeek.Friday:
                        return Constants.G_VENERDI;
                    case DayOfWeek.Saturday:
                        return Constants.G_SABATO;
                    case DayOfWeek.Sunday:
                        return Constants.G_DOMENICA;
                    default:
                        return Constants.G_UNKNOWN;
                }
            }
            catch {
                return Constants.G_UNKNOWN;
            }

        }

        private int GetNumFormPietanzeByMenuType(int index)
        {
            try
            {
                // restituisce il numero dei piatti per sequenza richiesta
                if (index >= TipoMenu.Length && index <= TipoMenu.Length)
                    return TipoMenu[index-1].Split(',').Length;
                else
                    return 0;
            }
            catch {
                return 0;
            }
        }

        public static void DebugRecordSet(ADODB.Recordset rs, Boolean Scroll = false)
        {
            if (rs.RecordCount > 0)
            {
                rs.MoveFirst();
                while (!rs.EOF)
                {
                    for (int i = 0; i < rs.Fields.Count; i++)
                    {
                        var name = rs.Fields[i].Name;
                        name = name.ToString();
                        object value = rs.Fields[i].Value;
                        if (value != null)
                            value = value.ToString();
                        object type = rs.Fields[i].Type;
                        type = type.ToString();
                        Debug.Print(string.Format("Nome => {0} valore => {1} tipo => {2}", name, value, type));

                    }
                    if (Scroll)
                    {
                        rs.MoveNext();
                    }
                    else break;
                }
            }

            object count = rs.RecordCount;
            count = count.ToString();
            Debug.Print(string.Format("Totale records {0}", rs.RecordCount));

        }

    }
}
