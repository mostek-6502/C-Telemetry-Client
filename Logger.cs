using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security;

namespace Namespace_Logger
{

    public class Logger
    {
        public StreamWriter Logger_Stream = null;
        public string Logger_Error_Message;

        public int OpenFile(string strFileName)
        {

            int iReturnCode = 0;

            try
            {
                Logger_Stream = new StreamWriter(strFileName);
            }
            catch (UnauthorizedAccessException eUAE)
            {
                Logger_Error_Message = eUAE.Message;
                iReturnCode = 10;
            }
            catch (ArgumentNullException aAE)
            {
                Logger_Error_Message = aAE.Message;
                iReturnCode = 11;
            }
            catch (ArgumentException aANE)
            {
                Logger_Error_Message = aANE.Message;
                iReturnCode = 12;
            }
            catch (DirectoryNotFoundException aDNFE)
            {
                Logger_Error_Message = aDNFE.Message;
                iReturnCode = 13;
            }
            catch (PathTooLongException aPTLE)
            {
                Logger_Error_Message = aPTLE.Message;
                iReturnCode = 14;
            }
            catch (IOException eIOE)
            {
                Logger_Error_Message = eIOE.Message;
                iReturnCode = 15;
            }
            catch (SecurityException eSE)
            {
                Logger_Error_Message = eSE.Message;
                iReturnCode = 16;
            }

            return iReturnCode;
        }



        public int LogIt(string strMessage)
        {
            int iReturnCode = 0;

            try
            {
                Logger_Stream.WriteLine(strMessage);
                //Logger_Stream.Flush();
            }
            catch (ObjectDisposedException eODE)
            {
                Logger_Error_Message = eODE.Message;
                iReturnCode = 20;
            }
            catch (IOException eIO)
            {
                Logger_Error_Message = eIO.Message;
                iReturnCode = 21;
            }

            catch (SecurityException eSE)
            {
                Logger_Error_Message = eSE.Message;
                iReturnCode = 22;
            }
            
            return iReturnCode;
        }


        public void FlushBuffer()
        {
            try
            {
                Logger_Stream.Flush();
            }
            catch (ObjectDisposedException eODE)
            {
                Logger_Error_Message = eODE.Message;
            }
            catch (IOException eIOE)
            {
                Logger_Error_Message = eIOE.Message;
            }
            catch (EncoderFallbackException eEFE)
            {
                Logger_Error_Message = eEFE.Message;
            }

            return;

        }



    }

}
