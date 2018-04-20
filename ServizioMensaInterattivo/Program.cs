
using ServizioMensaInterattivo.Components;
using System;
using System.Reflection;
using System.ServiceProcess;

namespace ServizioMensaInterattivo
{
    class Program
    {
        // entry point 
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            // Initialize array of service(s) to run
            ServicesToRun = new ServiceBase[]
            {
                new WindowsService()
            };

            
            if (Environment.UserInteractive)
                // se lanciato come console
                RunAsConsoleMethod(ServicesToRun);
            else
                // oppure lanciato come servizio
                ServiceBase.Run(ServicesToRun);

        }

        static void RunAsConsoleMethod(ServiceBase[] servicesToRun)
        {
            var onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.WriteLine("Waiting for starting {0}...", service.ServiceName);
                //onStartMethod.Invoke(service, new object[] { new string[] { } });
                onStartMethod.Invoke(service, new object[] { Environment.GetCommandLineArgs() });
            }

            var quit = "";
            while (quit != "quit")
            {
                Console.WriteLine("Please enter 'quit' to stop the service...");
                quit = Console.ReadLine();
            }

            var onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.WriteLine("Stopping {0}", service.ServiceName);
                onStopMethod.Invoke(service, null);
            }
        }

    }
}
