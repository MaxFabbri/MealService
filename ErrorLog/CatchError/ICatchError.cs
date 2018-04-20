using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Logger
{
    public interface ICatchError
    {
        /// <summary>
        /// Consente di gestire l'errore ricorsivamente scrive negli eventi nel log e può visualizzare messgebox
        /// </summary>
        /// <param name="Ex">Eccezione sollevata</param>
        /// <param name="View">settare true se si vuole visualizzare un MessageBox </param>
        /// <param name="Owner">form parente del messagebox</param>
        /// <param name="MsgButton">default MessageBoxButtons </param>
        /// <param name="Params">parametri supplementari relativi all'eccezione sollevata</param>
        /// <returns>ritorna il bottone premuto</returns>

        DialogResult Error(_Exception Ex, bool View = false, IWin32Window Owner = null, MessageBoxButtons MsgButton = MessageBoxButtons.OK , Dictionary<string, string> Params=null, string Caption="");

        ILog Log
        {
            get;
            set;
        }

    }
}
