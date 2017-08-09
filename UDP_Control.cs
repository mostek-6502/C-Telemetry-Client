// Project: Solar System
//
// Rick Faszold
//
// XEn, LLC
//
// August 9th, 2017
//
// The bulk of this code is to handle the incoming traffic from the controller, set data, color schemes and
// operational messages based on the incoming status codes fromt he controller.
//
// The magic starts happening around line 890.  This shows you the messages coming out of the controller
// and how they are handles.
//


using System;
using System.Collections.Generic;

using System.Drawing;

using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using System.Security;

using Namespace_Logger;
using Namespace_Form_Updates;
using Namespace_EEPROM_Update_Form;


namespace Namespace_UDP_Controls
{
    public class UDP_Controls
    {
        public Logger UDP_Log_Error_Messages = null;

        public Form_Updates UDP_FU = null;

        EEPROM_Update_Form UDP_EEPROM_Command_Updates = null;

        public Socket UDP_Socket_Main = null;

        public bool bReceivedData = false;
        public bool bReceivedData_S2 = false;

        public IPAddress UDP_IP_Local_Machine = null;
        
        public EndPoint UDP_IP_Endpoint;
        public EndPoint UDP_IP_Begin_Endpoint;

        public byte[] UDP_bReceivedData = new byte[1024];


        private string[] g_strDataArray = new string[132];

        public bool g_Received_Sent_S3_No_Data_Received = true;

        private string[] strMovementAction = {
                "MOVE_OK_TO_MOVE",
                "MOVE_H_RIGHT",
                "MOVE_H_LEFT",
                "MOVE_V_UP",
                "MOVE_V_DOWN",
                "MOVE_TRANSITION",
                "MOVE_ERROR_WIND_SPEED",
                "MOVE_H_ERROR_DUAL_PROXIMITY",
                "MOVE_V_ERROR_DUAL_PROXIMITY",
                "NO_H_MOVEMENT_NEEDED",
                "NO_V_MOVEMENT_NEEDED",
                "NO_MOVE_PROXIMITY_DETECT_RIGHT",
                "NO_MOVE_PROXIMITY_DETECT_LEFT",
                "NO_MOVE_PROXIMITY_DETECT_UP",
                "NO_MOVE_PROXIMITY_DETECT_DOWN",
                "NO_H_MOVEMENT_PERCENT_RANGE",
                "NO_V_MOVEMENT_PERCENT_RANGE",
                "NO_MOVEMENT_FAILSAFE",
                "NO_MOVEMENT_MOTORS_OFF",
                "FORCE_MOVE_H_RIGHT",
                "FORCE_MOVE_H_LEFT",
                "FORCE_MOVE_V_UP",
                "FORCE_MOVE_V_DOWN",
                "FORCE_MOVE_H_PAUSE",
                "FORCE_MOVE_V_PAUSE",
                "TEMP_TOO_HIGH_MOVE_AWAY_FROM_SUN_H",
                "TEMP_TOO_HIGH_MOVE_AWAY_FROM_SUN_V",
                "ERROR_HORIZONTAL_MOVEMENT_FLAG",
                "ERROR_VERTICAL_MOVEMENT_FLAG",
                "ERROR_PHOTO_RESISTOR_OUT_OF_RANGE" };

        

        public void Set_Logger(Logger Message_Logger)
        {
            UDP_Log_Error_Messages = Message_Logger;
        }

        public void Set_Form_Updates(Form_Updates FormUpdates)
        {
            UDP_FU = FormUpdates;
        }

        public void Set_EEPROM_Command_Updates(ref EEPROM_Update_Form EEPROM_Command_Update)
        {
            UDP_EEPROM_Command_Updates = EEPROM_Command_Update;
        }


        private void Log_Error_Message(string strLocation, int iErrorCode, string strErrorMessage)
        {
            string strMessage = String.Format("{0}  Error Code: {1}   {2}", strLocation, iErrorCode, strErrorMessage);

            if (UDP_Log_Error_Messages != null)
            {
                UDP_Log_Error_Messages.LogIt(strMessage);
            }

        }


        private void Log_General_Message(string strPlace, string strMessage)
        {
            string s1 = strMessage.Replace('\n', ' ');
            string s2 = s1.Replace('\r', ' ');
            string s3 = s2.Trim();
            
            
            if (UDP_Log_Error_Messages != null)
            {
                UDP_Log_Error_Messages.LogIt(strPlace + " " + s3);
            }

            UDP_FU.Set_Label_Display_Text(s3);
        }


        public int Initialize(Int32 iPortNumber)
        {
            string strPlace = "UDP.Initialize() ";

            Int32 iRtn = 0;

            Log_General_Message(strPlace, "Get Local Machine IP");

            IPHostEntry ipLocalHostInfo = null;
            iRtn = Get_Local_Machine_IPV4(ref ipLocalHostInfo);

            if (!iRtn.Equals(0))
            {
                Log_General_Message(strPlace, "Error: Get_Local_Machine_IPV4.");
                return iRtn;
            }



            // get local machine IP
            IPAddress ipAddress;
            for (int index = 0; index < ipLocalHostInfo.AddressList.Length; index++)
            {
                ipAddress = ipLocalHostInfo.AddressList[index];

                Byte[] abAddress = ipAddress.GetAddressBytes();

                if (abAddress.Length == 4)
                {
                    UDP_IP_Local_Machine = ipAddress;

                    string strLocalIP = String.Format("Local IP: {0}", UDP_IP_Local_Machine);
                    Log_General_Message(strPlace, strLocalIP);
                }
            }

            


            Log_General_Message(strPlace, "UDP.Set_Socket()");

            iRtn = Get_Socket();

            if (UDP_Socket_Main == null)
            {
                Log_General_Message(strPlace, "Error: UDP.Set_Socket()  (NULL)");
                return 11;
            }

            if (!iRtn.Equals(0))
            {
                Log_General_Message(strPlace, "Error: UDP.Set_Socket()");
                return iRtn;
            }





            Log_General_Message(strPlace, "UDP.Set_IPs()");

            iRtn = Set_IPs();
            if (!iRtn.Equals(0))
            {
                Log_General_Message(strPlace, "Error: UDP.Set_IPs()");
                return iRtn;
            }


            Log_General_Message(strPlace, "UDP.Bind()");


            IPEndPoint ip_LocalEndPoint = new IPEndPoint(UDP_IP_Local_Machine, iPortNumber);

            iRtn = Bind(ref ip_LocalEndPoint);

            if (!iRtn.Equals(0))
            {
                Log_General_Message(strPlace, "Error: UDP.Bind()");
                return iRtn;
            }

            Log_General_Message(strPlace, "UDP Initialization Complete!");


            return 0;
        }


        public int Get_Socket()
        {

            string strPlace = "Set_Socket()";

            int iReturnCode = 0;
           

            try
            {
                UDP_Socket_Main = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            catch (SocketException eSE)
            {
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                iReturnCode = eSE.ErrorCode;
            }

            return iReturnCode;
        }


        public void Close_Socket()
        {
            UDP_Socket_Main.Close();
        }


        public int Get_Local_Machine_IPV4(ref IPHostEntry ipLocalHostInfo)
        {
            string strPlace = "Get_Local_Machine_IPV4()";

            int iReturnCode = 0;

            // Get Local Machine IP

            try
            {
                ipLocalHostInfo = Dns.GetHostEntry("");
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 20;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);

            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 21;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }
            catch (SocketException eSE)
            {
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                iReturnCode = 22;
            }
            catch (ArgumentException eAE)
            {
                iReturnCode = 23;
                Log_Error_Message(strPlace, iReturnCode, eAE.Message);

            }

            return iReturnCode;
        }


        int Set_IPs()
        {

            string strPlace = "Set_IPs()";

            int iReturnCode = 0;

            try
            {
                UDP_IP_Endpoint = new IPEndPoint(IPAddress.Any, 0);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 30;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 31;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }


            if (iReturnCode != 0)
            {
                return iReturnCode;
            }


            try
            {
                UDP_IP_Begin_Endpoint = new IPEndPoint(IPAddress.Any, 0);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 32;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 33;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }

            return 0;
        }


        public int Bind(ref IPEndPoint ip_LocalEndPoint)
        {

            string strPlace = "Bind()";

            int iReturnCode = 0;


            try
            {
                UDP_Socket_Main.Bind(ip_LocalEndPoint);
            }
            catch (SocketException eSE)
            {
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                iReturnCode = 40;
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 41;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);

            }
            catch (ObjectDisposedException eODE)
            {
                iReturnCode = 42;
                Log_Error_Message(strPlace, iReturnCode, eODE.Message);
            }
            catch (SecurityException eSE)
            {
                iReturnCode = 43;
                Log_Error_Message(strPlace, iReturnCode, eSE.Message);
            }

            return iReturnCode;
        }



        public int Parse_IP(ref IPAddress ipa_RemoteAddress, string strIPAddress)
        {
            string strPlace = "Parse_IP()";

            int iReturnCode = 0;

            try
            {
                ipa_RemoteAddress = IPAddress.Parse(strIPAddress);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 50;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (FormatException eFE)
            {
                iReturnCode = 51;
                Log_Error_Message(strPlace, iReturnCode, eFE.Message);
            }

            return iReturnCode;
        }


        public int Compare_IPs(ref bool bTheSameContent, byte[] abRemoteIP, byte[] abLocalIP)
        {
            string strPlace = "Compare_IPs()";

            int iReturnCode = 0;

            try
            {
                bTheSameContent = Enumerable.SequenceEqual(abRemoteIP, abLocalIP);
            }
            catch (ArgumentException eANE)
            {
                iReturnCode = 60;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);

            }

            return iReturnCode;
        }


        public int GetIPEndPoint(ref IPEndPoint ipEndpoint, IPAddress ipa_RemoteAddress, int iPortNumber)
        {

            string strPlace = "GetIPEndPoint()";

            int iReturnCode = 0;

            try
            {
                ipEndpoint = new IPEndPoint(ipa_RemoteAddress, iPortNumber);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 70;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 71;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }


            return iReturnCode;

        }


        public int SendTo(byte[] sBuffer, int iSize, SocketFlags sfFlags, EndPoint ip_Endpoint, ref int iBytesSent)
        {
            string strPlace = "SendTo()";

            // the receive statement sets the end point.  
            // if we are initially searching, the use the input
            // if communications has been happening a while, the use UDP_ip_Endpoint.
            EndPoint Temp_ip_Endpoint = ip_Endpoint;
            if (ip_Endpoint == null)
            {
                Temp_ip_Endpoint = UDP_IP_Endpoint;
            }


            string strTempIP = String.Format("{0}", Temp_ip_Endpoint);
            if (strTempIP == "0.0.0.0")
            {
                Log_General_Message(strPlace, "Invalid IP Supplied to SendTo()  Return Without Processing.  IP: " + strTempIP);
                return 80;
            }

            string strData = System.Text.Encoding.ASCII.GetString(sBuffer, 0, sBuffer.Length);
            strData.TrimEnd(' ');


            //Log_General_Message(strPlace, "Sending: ->" + strData + "<- To: " + strTempIP);


            iBytesSent = 0;
            int iReturnCode = 0;

            try
            {
                iBytesSent = UDP_Socket_Main.SendTo(sBuffer, 0, iSize, sfFlags, Temp_ip_Endpoint);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 81;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 82;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }
            catch (SocketException eSE)
            {
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                iReturnCode = 83;
            }
            catch (ObjectDisposedException eODE)
            {
                iReturnCode = 84;
                Log_Error_Message(strPlace, iReturnCode, eODE.Message);
            }
            catch (SecurityException eSCE)
            {
                iReturnCode = 85;
                Log_Error_Message(strPlace, iReturnCode, eSCE.Message);
            }

            if (iBytesSent != iSize)
            {
                Log_Error_Message(strPlace, iReturnCode, "UDP Request to Send Does Not Match Actual UDP Send!");
            }

            return iReturnCode;
        }



        public int Send_Data_To_EndPoint(string strData, byte bSpecialChar)
        {

            string strPlace = "Send_Data_To_EndPoint()";

            Int32 iRtn = 0;
            Int32 iBytesSent = 0;

            byte[] bStringByteData = Encoding.ASCII.GetBytes(strData);

            int iArraySize = bStringByteData.Count();

            if (bSpecialChar > 0)
            {
                iArraySize++;
            }


            byte[] bByteData = new byte[iArraySize];

            Int32 iIndex = 0;
            for (iIndex = 0; iIndex < bStringByteData.Count(); iIndex++)
            {
                bByteData[iIndex] = bStringByteData[iIndex];
            }
            
            if (bSpecialChar > 0)
            {
                bByteData[iArraySize - 1] = bSpecialChar;
            }


            if (strData == "SYNCH_3 ")
            {
                iIndex = 0;
            }

            iRtn = SendTo(bByteData, bByteData.Count(), SocketFlags.None, UDP_IP_Endpoint, ref iBytesSent);


            string strMessage = "";

            if (iRtn.Equals(0))
            {
                if (bSpecialChar == 0)
                {
                    strMessage = String.Format("{0}", strData);
                }
                else
                {
                    strMessage = String.Format("{0}{1}", strData, bSpecialChar);
                }

                    // OK, the SendTo was OK, we are just checking the bytes sent....
                if (iBytesSent == bByteData.Count())
                {
                    Log_General_Message(strPlace, "Data Successfully Sent: " + strMessage);
                }
                else
                {
                    iRtn = 98;
                    Log_Error_Message(strPlace, iRtn, "SendTo()  Byte Sent Mismatch " + strMessage);
                }
            }
            else
            {
                Log_Error_Message(strPlace, iRtn, "SendTo()  Invalid Return From SendTo " + strMessage);
            }

            return iRtn;

        }


        public int Parse_The_Data(string strData)
        {
            string strTemp;
            char strChar;

            int iPos = 0;
            int iIndex = 0;
            int iLength = strData.Length;

            // initialize array
            for (iPos = 0; iPos < g_strDataArray.Length; iPos++)
            {
                g_strDataArray[iPos] = "";
            }

            strTemp = "";
            for (iPos = 0; iPos < iLength; iPos++)
            {
                if (strData[iPos] == ',')
                {
                    g_strDataArray[iIndex] = strTemp;
                    strTemp = "";
                    iIndex++;
                }
                else
                {
                    strChar = strData[iPos];

                    strTemp += strChar;
                }
            }

            if (strTemp.Length > 0)
            {
                g_strDataArray[iIndex] = strTemp;
                iIndex++;
            }

            return iIndex;

        }



        public int Update_EEPROM(string strEEPROM_Update)
        {

            string strPlace = "Update_EEPROM()";

            Int32 iRtn = 0;
            Int32 iBytesSent = 0;

            byte[] bBuffer = Encoding.ASCII.GetBytes("EEPROM_Update");

            iRtn = SendTo(bBuffer, bBuffer.Count(), SocketFlags.None, UDP_IP_Endpoint, ref iBytesSent);

            if (iRtn.Equals(0))
            {
                // OK, the SendTo was OK, we are just checking the bytes sent....
                if (iBytesSent == bBuffer.Count())
                {
                    Log_General_Message(strPlace, "EEPROM Update Successfully Sent! " + strEEPROM_Update);
                }
                else
                {
                    Log_Error_Message(strPlace, 110, "Byte Sent Mismatch from SendTo()");
                }
            }
            else
            {
                Log_Error_Message(strPlace, iRtn, "Invalid Return From SendTo()");
            }



            return 0;
        }



        private void Display_Temperature_Data()
        {

            string strPlace = "Display_Temperature_Data";
            string strDebug = "";

            UInt32 uiIndex = 0;

            string strResult = "";

            Int32 iTempCWhole = 0;
            Int32 iTempCFraction = 0;
            Int32 iSignCFlag = 0;
            Int32 iTempFWhole = 0;
            Int32 iTempFFraction = 0;
            Int32 iSignFFlag = 0;

            double fTemp = 0.0;

            UInt32 uiOffset = 0;

            // ok, now that we have ALL of the data, let's begin displaying it...
            for (uiIndex = 0; uiIndex < 16; uiIndex++)
            {
                Color colorBackGround = Color.FromName("OrangeRed");

                uiOffset = (uiIndex * 7) + 1;

                strResult = g_strDataArray[uiOffset + 0];
                if (strResult.Equals("0"))
                {
                    colorBackGround = Color.FromName("PaleGreen");
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "OK", colorBackGround);
                }
                else if (strResult.Equals("20"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "Chip Rst", colorBackGround);
                }
                else if (strResult.Equals("30"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "Channel-1", colorBackGround);
                }
                else if (strResult.Equals("40"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "ROM Rtrv", colorBackGround);
                }
                else if (strResult.Equals("50"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "No Cfg", colorBackGround);
                }
                else if (strResult.Equals("60"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "Temp Strt", colorBackGround);
                }
                else if (strResult.Equals("85"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "Channel-2", colorBackGround);
                }
                else if (strResult.Equals("90"))
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "Temp Retv", colorBackGround);
                }
                else
                {
                    UDP_FU.Set_Label_Flag_Data(uiIndex, "<" + g_strDataArray[uiOffset + 0] + ">", colorBackGround);
                }

                try
                {
                    iTempCWhole = Convert.ToInt32(g_strDataArray[uiOffset + 1]);
                    iTempCFraction = Convert.ToInt32(g_strDataArray[uiOffset + 2]);
                    iSignCFlag = Convert.ToInt32(g_strDataArray[uiOffset + 3]);

                    iTempFWhole = Convert.ToInt32(g_strDataArray[uiOffset + 4]);
                    iTempFFraction = Convert.ToInt32(g_strDataArray[uiOffset + 5]);
                    iSignFFlag = Convert.ToInt32(g_strDataArray[uiOffset + 6]);
                }
                catch (FormatException eFE)
                {
                    strDebug = String.Format("Index - {0}", uiIndex);
                    Log_General_Message(strPlace, strDebug + "  " + eFE.Message);
                    return;
                }
                catch (OverflowException eOF)
                {
                    strDebug = String.Format("Index - {0}", uiIndex);
                    Log_General_Message(strPlace, strDebug + "  " + eOF.Message);
                    return;
                }

                fTemp = iTempCWhole + (iTempCFraction / 10.0);
                if (iSignCFlag == 1)
                {
                    fTemp *= -1;
                }
                strResult = String.Format("{0:0.0}", fTemp);
                UDP_FU.Set_Label_C_Data(uiIndex, strResult, colorBackGround);



                fTemp = iTempFWhole + (iTempFFraction / 10.0);
                if (iSignFFlag == 1)
                {
                    fTemp *= -1;
                }
                strResult = String.Format("{0:0.0}", fTemp);
                UDP_FU.Set_Label_F_Data(uiIndex, strResult, colorBackGround);

            }

        }

        private void Display_ROM_Codes()
        {

            UInt32 uiFlag = 0;
            UInt32 uiIndex = 0;
            UInt32 uiOffset = 0;

            Color colorBackGround = Color.FromName("OrangeRed");


            // ok, now that we have ALL of the data, let's begin displaying it...
            for (uiIndex = 0; uiIndex < 16; uiIndex++)
            {
                uiOffset = (uiIndex * 2) + 1;

                uiFlag = Convert.ToUInt32(g_strDataArray[uiOffset + 0]);

                if (uiFlag == 0)
                {
                    colorBackGround = Color.FromName("PaleGreen");
                }
                else
                {
                    colorBackGround = Color.FromName("OrangeRed");
                }

                UDP_FU.Set_Label_ROM_Data(uiIndex, g_strDataArray[uiOffset + 1], colorBackGround);
            }

        }


        private bool EndPointsTheSame()
        {
            string strPlace = "EndPointsTheSame";

            string strLocalMachineIP = String.Format("{0}", UDP_IP_Local_Machine);
            string strEndPointMachineIP = String.Format("{0}", UDP_IP_Endpoint);

            bool bCompare;
            try
            {
                bCompare = strEndPointMachineIP.Contains(strLocalMachineIP);
            }
            catch (ArgumentNullException eANE)
            {
                Log_Error_Message(strPlace, 101, eANE.Message);
                return false;
            }

            return bCompare;
        }


        private UInt32 Convert_To_UInt(UInt32 uiIndex)
        {
            string strPlace = "Convert_To_UInt";

            UInt32 uiResult;

            string strTemp = "";

            

            try
            {
                strTemp = g_strDataArray[uiIndex];
                uiResult = UInt32.Parse(strTemp);
            }
            catch (OverflowException eOE)
            {
                string strErrorMsg = String.Format("  Index: {0}", uiIndex);

                uiResult = 0xFFFFFFFF;
                Log_Error_Message(strPlace, 1171, eOE.Message + strErrorMsg);
            }
            catch (FormatException eFE)
            {
                string strErrorMsg = String.Format("  Index: {0}", uiIndex);

                uiResult = 0xFFFFFFFE;
                Log_Error_Message(strPlace, 1172, eFE.Message + strErrorMsg);
            }
            catch (ArgumentNullException eNE)
            {
                string strErrorMsg = String.Format("  Index: {0}", uiIndex);

                uiResult = 0xFFFFFFFE;
                Log_Error_Message(strPlace, 1173, eNE.Message + strErrorMsg);
            }

            return uiResult;

        }

        

        // the magic happens here...
        private void ProcessIncomingMessage(string strData, int iRtnReadBytes)
        {
            string strPlace = "PIM";

            UInt32 uiIndex = 0;
            string strTemp = "";


            // where is the IP coming from?
            bool bCompare = EndPointsTheSame();
            if (bCompare == true)
            {
                Log_General_Message(strPlace, "Received Data From Local Machine IP.  Data Ignored.");
                return;
            }


           

            bReceivedData = true;

            string strDebug = String.Format("PIM - Data ->{0}<-  From: {1}", strData, UDP_IP_Endpoint);
            UDP_Log_Error_Messages.LogIt(strDebug); 


            int iCount = Parse_The_Data(strData);
            if (iCount == 0)
            {
                Log_Error_Message(strPlace, iCount, "Unable To parse The Data!");
                return;
            }

            string strCommandType = g_strDataArray[0];

            if (strCommandType == "SYNCH_2 ")
            {
                // Telemetry sends SYNCH_1, Telemetry replies with SYNCH_2, Telemetry replies with SYNCH_3 and
                // then controllers starts dumping data.  Due to the 'nature of this simple handshake,
                // the session can be stolen by other clients.  This was NOT a design error.

                // The Tiva will delay some operations until it receives SYNCH_3

                Log_General_Message(strPlace, "Received [SYNCH_2 ] From Tiva.  Sending [SYNCH_3 ]");

                bReceivedData_S2 = true;

                g_Received_Sent_S3_No_Data_Received = true;

                int iRtn = Send_Data_To_EndPoint("SYNCH_3 ", 0);
            }
            else if (strCommandType == "COMREPLY")  
            {
                // this message is only for chat window processing
                // we sent a message to the controller via chat, this is the reply.
                // the chat window is pretty basic... it is a Text Box and a List Box...
                // Send a command via the text box, receive output and display in text box.
                g_Received_Sent_S3_No_Data_Received = false;
                for (uiIndex = 1; uiIndex < iCount; uiIndex++)
                {
                    strTemp = g_strDataArray[uiIndex];
                    UDP_FU.Add_Command_ListBox(strTemp);
                }
            }
            else if (strCommandType == "EEPROM  ")
            {
                // these are the EEPROM settings for the controller.  These can be changed on the fly.
                string strIPTemp = String.Format("{0}", UDP_IP_Endpoint);
                UDP_FU.Set_Label_IP_Text(strIPTemp);
                
                g_Received_Sent_S3_No_Data_Received = false;
                for (uiIndex = 1; uiIndex < iCount; uiIndex++)
                {
                    strTemp = g_strDataArray[uiIndex];
                    UDP_FU.Set_Label_EEPROM_Data(uiIndex - 1, strTemp);
                }
            }
            else if (strCommandType == "TEMPS   ")
            {
                // temperatures.
                g_Received_Sent_S3_No_Data_Received = false;
                Display_Temperature_Data();
            }
            else if (strCommandType == "ROMCODES")
            {
                // ROMCODES are part of tempeatures but they are sent infrequently due to their static nature.
                // the hope was to save on bandwidth.
                g_Received_Sent_S3_No_Data_Received = false;
                Display_ROM_Codes();
            }
            else if (strCommandType == "PUMPS   ")
            {
                // 0       ,1,2,3,4,5,6,7,8,9
                // PUMPS   ,1,0,1,0,0,0,0,0,0
                Color colorBackGround = Color.FromName("White");
                UDP_FU.Set_Label_Pump_Data(0, strCommandType, colorBackGround);  // 0



                g_Received_Sent_S3_No_Data_Received = false;
                for (uiIndex = 1; uiIndex < iCount; uiIndex++)
                {
                    colorBackGround = Color.FromName("White");

                    strTemp = g_strDataArray[uiIndex];

                    if (uiIndex == 1)
                    {
                        uint uiTherm = Convert_To_UInt(uiIndex);
                        if (uiTherm == 0)
                        {
                            strTemp = "Off";
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else
                        {
                            strTemp = "On";
                            colorBackGround = Color.FromName("PaleGreen");
                        }
                    }
                    else if ((uiIndex == 2) || (uiIndex == 4) || (uiIndex == 6) || (uiIndex == 8))
                    {
                        uint uiStatus = Convert_To_UInt(uiIndex + 1);

                        uint uiValue = Convert_To_UInt(uiIndex);

                        if ((uiStatus == 0) && (uiValue > 0))
                        {
                            colorBackGround = Color.FromName("PaleGreen");
                        }
                        else if ((uiStatus == 0) && (uiValue == 0))
                        {
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else if (uiStatus == 1)
                        {
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else if (uiStatus == 5)
                        {
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else
                        {
                            colorBackGround = Color.FromName("OrangeRed");
                        }
                    }
                    else if ((uiIndex == 3) || (uiIndex == 5) || (uiIndex == 7) || (uiIndex == 9))
                    {
                        uint uiENUM = Convert_To_UInt(uiIndex);


                        if (uiENUM == 0)
                        {
                            strTemp = "Running";
                            colorBackGround = Color.FromName("PaleGreen");
                        }
                        else if (uiENUM == 1)
                        {
                            strTemp = "Off";
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else if (uiENUM == 5)
                        {
                            strTemp = "Therm. Off";
                            colorBackGround = Color.FromName("Yellow");
                        }
                        else
                        {
                            colorBackGround = Color.FromName("OrangeRed");

                            if (uiENUM == 2) strTemp = "Temp. Err";
                            else if (uiENUM == 3) strTemp = "Too Cold";
                            else if (uiENUM == 4) strTemp = "Too Hot";
                            else if (uiENUM == 6) strTemp = "Failsafe Error";
                            else if (uiENUM == 7) strTemp = "House Temp. Err";
                            else if (uiENUM == 8) strTemp = "House Temp. High";
                            else strTemp = "Unk Error";
                        }

}
                        
                    UDP_FU.Set_Label_Pump_Data(uiIndex, strTemp, colorBackGround);
                }
            }
            else if (strCommandType == "SUPPORT ")
            {
                // speed
                // status
                // failsafe
                // Proximity Right, Proximity Left, Proximity Up, Proximity Down
                // SUPPORT ,0,0,1,0,0,0,0

                Color colorBackGround = Color.FromName("White");

                UDP_FU.Set_Label_Support_Data(0, strCommandType, colorBackGround);  // 0



                /* === */
                colorBackGround = Color.FromName("PaleGreen");
                string strSpeed = g_strDataArray[1];
                UDP_FU.Set_Label_Support_Data(1, strSpeed, colorBackGround);        // 1


                /* === */
                uint uiWindSpeedStatus = Convert_To_UInt(2);
                string strMessage = "OK";
                colorBackGround = Color.FromName("PaleGreen");

                if (uiWindSpeedStatus == 1)
                {
                    strMessage = "Caution";
                    colorBackGround = Color.FromName("Yellow");
                }
                else if (uiWindSpeedStatus == 2)
                {
                    strMessage = "Alert";
                    colorBackGround = Color.FromName("OrangeRed");
                }
                UDP_FU.Set_Label_Support_Data(2, strMessage, colorBackGround);      // 2


                /* === */
                uint uiFailsafe = Convert_To_UInt(3);
                string strFailSafe = "Connected";
                colorBackGround = Color.FromName("PaleGreen");

                if (uiFailsafe == 0)
                {
                    colorBackGround = Color.FromName("OrangeRed");
                    strFailSafe = "Disconnected";
                }
                UDP_FU.Set_Label_Support_Data(3, strFailSafe, colorBackGround);     // 3


                /* === */
                for (uiIndex = 4; uiIndex < 8; uiIndex++)
                {
                    uint uiProximity = Convert_To_UInt(uiIndex);

                    if (uiProximity == 0)
                    {
                        colorBackGround = Color.FromName("PaleGreen");
                        strTemp = "OK";
                    }
                    else
                    {
                        colorBackGround = Color.FromName("Yellow");
                        strTemp = "Limit";
                    }

                    UDP_FU.Set_Label_Support_Data(uiIndex, strTemp, colorBackGround);   // 4, 5, 6, 7
                }

                g_Received_Sent_S3_No_Data_Received = false;
            }
            else if (strCommandType == "DISH    ")
            {
                // FIX THIS!!!!!!!!!!!!!!!!!!
                // 4, 2, 2
                // 0,       1,2,3,4,5,6,7,8,9,10, 11, 12
                // DISH    ,0,0,0,0,0,9,0,0,0, 0,  0,  4

                g_Received_Sent_S3_No_Data_Received = false;
                Color colorBackGround = Color.FromName("White");

                UDP_FU.Set_Label_Movement_Data(0, strCommandType, colorBackGround);

                for (uiIndex = 1; uiIndex < iCount; uiIndex++)
                {
                    colorBackGround = Color.FromName("White");

                    strTemp = g_strDataArray[uiIndex];

                    if ((uiIndex == 6) || (uiIndex == 12))
                    {
                        uint uiMoveFlag = Convert_To_UInt(uiIndex);
                        if (uiMoveFlag <= strMovementAction.Count())
                        {
                            strTemp = strMovementAction[uiMoveFlag];
                        }
                        else
                        {
                            colorBackGround = Color.FromName("OrangeRed");
                            strTemp = "Error: Unknown";
                        }
                    }

                    UDP_FU.Set_Label_Movement_Data(uiIndex, strTemp, colorBackGround);
                }

            }
            else if (strCommandType == "RUNTIME ")
            {
                g_Received_Sent_S3_No_Data_Received = false;
                Color colorBackGround = Color.FromArgb(255, 224, 192);   // not used but can be used later...

                for (uiIndex = 0; uiIndex < iCount; uiIndex++)
                {
                    
                    strTemp = g_strDataArray[uiIndex];

                    if ((uiIndex == 1) || (uiIndex == 8))
                    {
                        UInt32 uiTime = Convert_To_UInt(uiIndex);

                        UInt32 uiDays = 0;
                        if (uiTime >= 86400)
                        {
                            uiDays = uiTime / 86400;  // number of days.
                            uiTime = uiTime % 86400;
                        }


                        strTemp = String.Format("{0}:{1:00}:{2:00}:{3:00}", uiDays, uiTime / 3600, (uiTime / 60) % 60, uiTime % 60);
                    }
                    else if (uiIndex != 0)
                    {
                        UInt32 ui32Value = Convert_To_UInt(uiIndex);

                        strTemp = String.Format("{0:0,0}", ui32Value);
                    }

                    UDP_FU.Set_Label_Runtime_Data(uiIndex, strTemp, colorBackGround);

                }


                UInt32 uiTotalTime = Convert_To_UInt(1);

                UInt32 ui32Value1 = Convert_To_UInt(2);
                float f = (float) ui32Value1 / (float) uiTotalTime;
                strTemp = String.Format("{0:0.00}", f);
                UDP_FU.Set_Label_Runtime_Data(11, strTemp, colorBackGround);

                ui32Value1 = Convert_To_UInt(3);
                f = (float) ui32Value1 / (float) uiTotalTime;
                strTemp = String.Format("{0:0.00}", f);
                UDP_FU.Set_Label_Runtime_Data(12, strTemp, colorBackGround);

            }
            else if (strCommandType == "MESSAGE")
            {
                g_Received_Sent_S3_No_Data_Received = false;
                UDP_FU.Set_Label_Display_Text("MESSAGE->" + g_strDataArray[1]);
            }
            else
            {
                UDP_FU.Set_Label_Display_Text("UNKNOWN->" + g_strDataArray[1]);
            }


            return;
        }


                
        public int ReceiveFrom()
        {
            // this is a BLOCKING read....

            string strPlace = "ReceiveFrom()";

            int iReturnCode = 0;



            //IAsyncResult iAR;

            try
            {
                UDP_Socket_Main.BeginReceiveFrom(UDP_bReceivedData, 0, UDP_bReceivedData.Length, SocketFlags.None, ref UDP_IP_Begin_Endpoint, new AsyncCallback(this.Callback_ReceiveFrom), UDP_Socket_Main);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 90;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
            }
            catch (ArgumentOutOfRangeException eAOoRE)
            {
                iReturnCode = 91;
                Log_Error_Message(strPlace, iReturnCode, eAOoRE.Message);
            }
            catch (SocketException eSE)
            {
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                iReturnCode = 92;
            }
            catch (ObjectDisposedException eODE)
            {
                iReturnCode = 93;
                Log_Error_Message(strPlace, iReturnCode, eODE.Message);
            }
            catch (SecurityException eSecE)
            {
                iReturnCode = 94;
                Log_Error_Message(strPlace, iReturnCode, eSecE.Message);
            }
            catch (Exception ex)
            {
                iReturnCode = 95;
                Log_Error_Message(strPlace, iReturnCode, ex.Message);
            }

            return iReturnCode;
        }
        



        private void Callback_ReceiveFrom(IAsyncResult ar)
        {
            string strPlace = "Callback_ReceiveFrom()";

            int iReturnCode = 0;
            int iRtnReadBytes = 0;

            string strDebug = "";

            try
            {
                iRtnReadBytes = UDP_Socket_Main.EndReceiveFrom(ar, ref UDP_IP_Endpoint);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 100;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
                return;
            }
            catch (ArgumentOutOfRangeException eAOoRe)
            {
                iReturnCode = 101;
                Log_Error_Message(strPlace, iReturnCode, eAOoRe.Message);
                return;
            }
            catch (ArgumentException eAE)
            {
                iReturnCode = 102;
                Log_Error_Message(strPlace, iReturnCode, eAE.Message);
                return;
            }
            catch (SocketException eSE)
            {
                iReturnCode = 103;
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                return;
            }
            catch (ObjectDisposedException eODE)
            {
                iReturnCode = 104;
                Log_Error_Message(strPlace, iReturnCode, eODE.Message);
                return;
            }
            catch (Exception ex)
            {
                iReturnCode = 105;
                Log_Error_Message(strPlace, iReturnCode, ex.Message);
                return;
            }




            if (iRtnReadBytes == 0)
            {
                strDebug = String.Format("Zero Byte Receive From IP: {0} ", UDP_IP_Endpoint);

                Log_General_Message(strPlace, strDebug);

                return;
            }

            string strData = "";

            try
            {
                strData = System.Text.Encoding.ASCII.GetString(UDP_bReceivedData, 0, iRtnReadBytes);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 200;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
                return;
            }
            catch (ArgumentOutOfRangeException eAOoRe)
            {
                iReturnCode = 201;
                Log_Error_Message(strPlace, iReturnCode, eAOoRe.Message);
                return;
            }
            catch (ArgumentException eAE)
            {
                iReturnCode = 202;
                Log_Error_Message(strPlace, iReturnCode, eAE.Message);
                return;
            }
            catch (Exception ex)
            {
                iReturnCode = 203;
                Log_Error_Message(strPlace, iReturnCode, ex.Message);
                return;
            }


            ProcessIncomingMessage(strData, iRtnReadBytes);
                    
            UDP_bReceivedData = new byte[1024];

            // Continue listening for broadcasts

            try
            {
                UDP_Socket_Main.BeginReceiveFrom(UDP_bReceivedData, 0, UDP_bReceivedData.Length, SocketFlags.None, ref UDP_IP_Begin_Endpoint, new AsyncCallback(this.Callback_ReceiveFrom), UDP_Socket_Main);
            }
            catch (ArgumentNullException eANE)
            {
                iReturnCode = 300;
                Log_Error_Message(strPlace, iReturnCode, eANE.Message);
                return;
            }
            catch (ArgumentOutOfRangeException eAOoRe)
            {
                iReturnCode = 301;
                Log_Error_Message(strPlace, iReturnCode, eAOoRe.Message);
                return;
            }
            catch (ArgumentException eAE)
            {
                iReturnCode = 302;
                Log_Error_Message(strPlace, iReturnCode, eAE.Message);
                return;
            }
            catch (SocketException eSE)
            {
                iReturnCode = 303;
                Log_Error_Message(strPlace, eSE.ErrorCode, eSE.Message);
                return;
            }
            catch (ObjectDisposedException eODE)
            {
                iReturnCode = 304;
                Log_Error_Message(strPlace, iReturnCode, eODE.Message);
                return;
            }
            catch (SecurityException eSE)
            {
                iReturnCode = 305;
                Log_Error_Message(strPlace, iReturnCode, eSE.Message);
                return;
            }

            catch (Exception ex)
            {
                iReturnCode = 306;
                Log_Error_Message(strPlace, iReturnCode, ex.Message);
                return;
            }


            return;
        }
    }


}
