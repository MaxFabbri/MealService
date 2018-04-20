using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Logger;


namespace ServizioMensaInterattivo
{
    class ServizioMensaInterattivo:IDisposable
    {
        static Interaction interaction;

        internal ServizioMensaInterattivo()
        {

            #region Init LOG

            var logger = Logger.Logger.GetInstance;

            // Inizializza il percorso di destinazione dei file di LOG
            var path = Path.GetDirectoryName(Application.ExecutablePath) + @"\LOGs\" +
                Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetName().Name);
            logger.InitLog("InterattivoMensa", Path.GetDirectoryName(Application.ExecutablePath) + @"\LOGs\InterattivoMensa.log", System.Reflection.Assembly.GetExecutingAssembly());
            logger.InitErrorLog(path + "_Error.Log", Assembly.GetExecutingAssembly());

            #endregion

            interaction = new Interaction();

            interaction.StartProcess();

            logger.GetLogs["InterattivoMensa"].Append("Present - Servizio mensa interattivo avviato", true, true,true,false);

        }
        
        public void Dispose()
        {
            var logger = Logger.Logger.GetInstance;
            interaction.EndProcess();
            logger.GetLogs["InterattivoMensa"].Append("Present - Servizio mensa interattivo fermato", true, true, true,false );
            //logger.ReleaseObj();
        }

    }
}
