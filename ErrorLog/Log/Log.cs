using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Logger
{
    
    public class Log:ILog 
    {
        readonly string logFile;
        StreamWriter streamWriter;
        Assembly assembly ;

        public Log(string LOGFileName = "" , Assembly assembly=null)
        {
            if (LOGFileName == "")
            {
                logFile = GetFileLogName();
            }
            else
            {
                logFile = LOGFileName;
            }

            if (assembly != null)
                this.assembly = assembly;

            CheckFolder();
        }

        public void Append(string Message="", bool Date =false, bool User =false, 
                           bool Release =false, bool CRLFAfterUser =false)
        {
            try
            {

                using (streamWriter = File.AppendText(logFile))
                {
                    WriteLog(Message, streamWriter, Date, User, Release,CRLFAfterUser);
                }
            }
            catch (Exception)
            {
                // l'app potrebbe non avere i permessi per creare la chiave del log bypasso l'errore
            }
        }

        public string GetFileLogName()
        {
            if (logFile == "")
            {
                // il log predefinito è posizionato nella sottocartella \Logs della dll e si chiama come la dll
                return Path.GetFullPath(Application.ExecutablePath) + @"\LOGs\" + Assembly.GetExecutingAssembly().GetName().Name;
            }
            return logFile;
        }

        public void CheckFolder(string LOGFile)
        {
            // se non c'è la cartella tenta di crearla
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(LOGFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LOGFile));
                }
            }
            
            catch (Exception)
            {
                // l'app potrebbe non avere i permessi per creare la chiave del log bypasso l'errore
            }
        }

        void CheckFolder()
        {
            CheckFolder(logFile);
        }

        // metodo interno per scrivere sul log
        void WriteLog(string logMessage, TextWriter textWriter,
                       bool writeDate, bool writeUser, bool writeRelease, 
                       bool CRLFAfterUser)
        {

            StringBuilder stringBuilder = new StringBuilder(7*500);
                        
            if (writeDate){
                var date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                stringBuilder.Append(date);
            }

            if (writeUser){
                if (stringBuilder.Length > 0) {
                    stringBuilder.Append(" | ");
                }
                stringBuilder.Append(Environment.UserName);
            }

            if (writeRelease){
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" | ");
                }
                if (assembly != null)
                    stringBuilder.Append(assembly.GetName() + " " + FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion);
                else
                    stringBuilder.Append( Application.ProductVersion);
            }

            if (stringBuilder.Length > 0)
            {
                if (!CRLFAfterUser) 
                    stringBuilder.Append(" | ");
                else 
                    stringBuilder.Append("\r\n");
            }

            if (logMessage != "")
                stringBuilder.Append(logMessage);

            textWriter.WriteLine(stringBuilder);
        }

        public void RenameFileLog(int NumberOfCopies)
        {
            if (logFile == "") {
                return;
            }

            if (!File.Exists(logFile)) {
                return;
            }

            // cerca tutti i files Copia ...
            string[] fs = Directory.GetFiles(Path.GetDirectoryName(logFile), 
                                           "Copia (*) di "+ Path.GetFileName(logFile));

            // finchè non arriva al valore massimo dei file fa la copia 
            if (fs.Length < NumberOfCopies) {
                string tmp = string.Format(Path.GetDirectoryName(logFile) + "\\Copia ({0}) di " + Path.GetFileName(logFile), fs.Length + 1);
                File.Copy(logFile, tmp,true);
                File.Delete(logFile);
                return;
            }
            
            // sovrascrive il file più vecchio 
            SortedList<DateTime, string> fa = new SortedList<DateTime, string>();
            foreach (string f in fs)
            {
                Debug.Print(f);
                Debug.Print (Directory.GetLastWriteTime(f).ToString("yyyyMMddHHmmss"));
                fa.Add(Directory.GetLastWriteTime(f), f);
            }

            if (fa.Count > 0)
            {
                Debug.Print(fa.Values[0]);
                File.Copy(logFile, fa.Values[0], true);
                File.Delete(logFile);
            }
        }

        bool OpenStreamWriterLog() 
        {

            var StreamWriterLog = true;

            try
            {
                streamWriter = File.AppendText(logFile);
            }
            catch
            {
                StreamWriterLog = false;
            }
            finally
            {
                streamWriter.Close();
            }
            return StreamWriterLog;
        }

    }
}
