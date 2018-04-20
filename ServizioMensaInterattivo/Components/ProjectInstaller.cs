using ServizioMensaInterattivo.Components;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServizioMensaInterattivo
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        readonly ServiceProcessInstaller process;
        readonly ServiceInstaller service;

        // in VS2015 non c'è la possibilità di installare con installshield
        // questo serve per installare da cmd con installutil.exe
        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            service = new ServiceInstaller();

            // nome servizio
            service.ServiceName = WindowsService.SERVICE_NAME;
            service.DisplayName = WindowsService.SERVICE_DISPLAY_NAME;
            service.Description = WindowsService.SERVICE_DISPLAY_NAME;

            // utente e modalità di avvio del servizio
            process.Account = ServiceAccount.LocalSystem;
            service.StartType = ServiceStartMode.Manual;

            Installers.Add(process);
            Installers.Add(service);

        }


    }
}
