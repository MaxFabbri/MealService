using System;
using APInterMngTypeLib;
using PRP;
using Logger;

namespace ServizioMensaInterattivo
{
    internal static class InteropObject
    {
        public static IRemoteProvider RP;
        public static IAPInteractionMng Interaction;

        public static void ReloadObj(ICatchError Er, bool ResetPRP=true,bool ResetAP=true)
        {

            try
            {

                ReleaseObj(ResetPRP,ResetAP);

                if (ResetPRP)
                {
                    RP = (IRemoteProvider)Activator.CreateInstance(
                        Type.GetTypeFromProgID("PresentRemoteProvider.RemoteProvider"));


                }
                if (ResetAP)
                {
                    Interaction = (IAPInteractionMng)Activator.CreateInstance(
                        Type.GetTypeFromProgID("APInterMng.APInteractionMng"));
                }


            }
            catch (Exception ex)
            {

                Er.Error(ex);
                ReleaseObj();

            }
        }

        public static void ReleaseObj(bool ResetPRP = true, bool ResetAP = true)
        {
            if (ResetPRP) 
            {
                RP = null;
            }
            if (ResetAP)
            {
                Interaction = null;
            }
        }

    }

}

 
