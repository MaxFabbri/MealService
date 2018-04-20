
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace Logger
{
    public class CatchError:ICatchError 
    {

        ILog log;
        readonly Assembly assembly;

        /// <summary>
        /// Consente di gestire l'errore ricorsivamente scrive negli eventi nel log e può visualizzare messgebox
        /// </summary>
        /// <param name="errorException">Eccezione sollevata</param>
        /// <param name="displayError">settare true se si vuole visualizzare un MessageBox </param>
        /// <param name="ownerForm">form parente del messagebox</param>
        /// <param name="msgButton">default MessageBoxButtons </param>
        /// <param name="exceptionParams">parametri supplementari relativi all'eccezione sollevata</param>
        /// <returns>ritorna il bottone premuto</returns>
        public DialogResult Error(_Exception errorException, 
                                  bool displayError = false, 
                                  IWin32Window ownerForm = null, 
                                  MessageBoxButtons msgButton = MessageBoxButtons.OK, 
                                  Dictionary<string, string> exceptionParams=null,
                                  string caption="")  
        {

            Exception exception = (Exception)errorException;
            StringBuilder stringBuilder = new StringBuilder();

            int errorNumber=0;
            string message = exception.Message;

            stringBuilder.Append('\t');
            stringBuilder.Append(" | ");

            if (caption != "")
            {
                stringBuilder.Append("Internal debug: " + caption);
                AppendCrLnTabPipe(stringBuilder);
            }

            do
            {
                stringBuilder.Append("Error description: " + exception.Message);
                AppendCrLnTabPipe(stringBuilder);
                
                stringBuilder.Append("Source: " + exception.Source);

                if (exception.StackTrace != null)
                {
                    AppendCrLnTabPipe(stringBuilder);
                    stringBuilder.Append("Stack trace: " + exception.StackTrace.TrimStart().Replace("\r\n   ", "\r\n\t => "));
                }

                if (exception.TargetSite != null)
                {
                    AppendCrLnTabPipe(stringBuilder);
                    stringBuilder.Append("Target site: " + exception.TargetSite);
                }

                AppendCrLnTabPipe(stringBuilder);
                stringBuilder.Append("Exception type name: " + exception.GetType().Name);

                if (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    stringBuilder.Append("\r\n");
                    stringBuilder.Append('\t', ++errorNumber);
                }
                else
                    exception = null;
            }
            while (exception != null);

            if (exceptionParams != null)
            {
                foreach (KeyValuePair<string, string> extraData in exceptionParams)
                {
                    stringBuilder.Append(string.Format("{0}: {1}", extraData.Key, extraData.Value));
                }
            }

            DialogResult dialogResult = DialogResult.OK; 
            if (displayError)
                dialogResult = MessageBox.Show(ownerForm, stringBuilder.ToString().Replace("\r\n", "").Replace(" | ", "\r\n").Replace("\t", ""), caption, msgButton, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            // fisso sul log error!
            log.Append(stringBuilder.ToString(),true,true,true);

            try
            {

                //string Key = Application.ProductName + " " + Application.ProductVersion;
                var key = assembly.GetName() + " " + FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                var source = errorException.Source;

                if (EventLog.Exists(key))
                {
                    // per cancellare chiave 
                    //EventLog evento = new EventLog(Key);
                    //evento.Clear();
                    //EventLog.Delete(Key);
                    //EventLog.CreateEventSource(Source, Key);
                }
                else
                {
                    Debug.Print(key + source);
                    EventLog.CreateEventSource(source, key);
                }

                EventLog ev = new EventLog(key);
                ev.Source = source;
                ev.WriteEntry(stringBuilder.ToString(), EventLogEntryType.Error, 44);

            }
            catch (Exception)
            {
                // l'app potrebbe non avere i permessi per creare la chiave del log bypasso l'errore
            }

            return dialogResult;
        }

        void AppendCrLnTabPipe(StringBuilder stringBuilder)
        {
            stringBuilder.Append("\r\n");
            stringBuilder.Append('\t');
            stringBuilder.Append(" | ");
        }


        #region ICatchError Membri di

        public ILog Log
         {
             get
             {
                 return log;
             }
             set
             {
                 log = value; 
             }
         }

         #endregion


        public CatchError(Assembly Assembly)
         {
             assembly = Assembly;
         }

    }


}
