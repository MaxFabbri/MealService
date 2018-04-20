using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServizioMensaInterattivo
{
    static class ExtDateTime
    {
        public static int GetMinutes(this DateTime d)
        {
            return (int)d.Hour * 60 + d.Minute;
        }

        public static DateTime GetMinuteToTime(this int Minutes)
        {
            return new DateTime().Add(new TimeSpan(0, Minutes, 0)); 
        }
    }
}

