using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServizioInterattivoMenu
{
    public partial class PresentServizioMensa : ServiceBase
    {
        
        ServizioInterattivoMenu Service = null;

        public PresentServizioMensa()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Service = new ServizioInterattivoMenu();
        }

        protected override void OnStop()
        {
            Service.Dispose();
            Service = null;
        }
    }
}
