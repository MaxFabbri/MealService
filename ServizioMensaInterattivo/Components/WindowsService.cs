using System.ServiceProcess;

namespace ServizioMensaInterattivo.Components
{
    class WindowsService : ServiceBase
    {
        public const string SERVICE_NAME = "PresentServizioMensa";
        public const string SERVICE_DISPLAY_NAME = "Present servizio mensa";
        ServizioMensaInterattivo Service;

        // Variabile di istanza principale 

        public WindowsService()
        {
            ServiceName = SERVICE_NAME;
        }


        protected override void OnStart(string[] args)
        {
            // TODO: inserire qui il codice necessario per avviare il servizio.
            Service = new ServizioMensaInterattivo();
        }

        protected override void OnStop()
        {
            // TODO: inserire qui il codice delle procedure di chiusura necessarie per arrestare il servizio.
            Service.Dispose();
            Service = null;

        }
    }
}
