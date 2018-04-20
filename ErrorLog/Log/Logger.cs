
using System.Collections.Generic;
using System.Reflection;

namespace Logger
{
    public sealed class Logger

    {
        // inizializza il log il log errori e il catcher degli errori 
        static Dictionary<string,ILog> logs;
        static ILog log;
        static ILog logError;
        static ICatchError CatchError;
        static readonly object Locker = new object();
        static Logger logger;

        public void InitLog(string logReferenceName, 
                            string logFileName, Assembly Assembly)
        {

            log = new Log(logFileName,Assembly);

            if (logs == null)
                logs = new Dictionary<string,ILog >();

            logs.Add(logReferenceName, log);

        }

        /// <summary>
        /// Consente di definire il nome per la gestione degli errori e l'assembly di riferimento
        /// </summary>
        /// <param name="logFileName">Nome file gestione log</param>
        /// <param name="assembly">Assembly di riferimento gestione errori</param>
        public void InitErrorLog(string logFileName, Assembly assembly)
        {
            logError = new Log(logFileName,assembly);
            if (CatchError == null) CatchError = new CatchError(assembly);
            CatchError.Log = logError;
        }

        public static Logger GetInstance
        {
            get
            {
                if (logger == null)
                {
                    lock (Locker)
                    {
                        // singleton eager loading e lo lasciamo così...
                        if (logger == null)
                        {
                            logger = new Logger();
                        }
                    }
                }
                return logger;
            }
        }

        public Dictionary<string, ILog> GetLogs
        {
            get
            {
                return logs;
            }
            
        }

        public ICatchError GetErrorLog
        {
            get
            {
                return CatchError;
            }
        }

        //public void ReleaseObj()
        //{
        //    logs = null;
        //    CatchError = null;
        //    logError = null;
        //}

    }

}
