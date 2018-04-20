using System;
using System.Reflection;

namespace Logger
{
    public interface ILog
    {

        void Append(string Message = "", Boolean Date = false, Boolean User = false, Boolean Release = false, Boolean CRLFAfterUser = true);
        //void Append(string Message, Boolean Date, Boolean User, Boolean Release);
        void CheckFolder(string LOGFile);
        string GetFileLogName();
        void RenameFileLog(int NumberOfCopies);


    }
}
