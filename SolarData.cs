// Project: Solar System
//
// Rick Faszold
//
// XEn, LLC
//
// August 9th, 2017
//
// This is the start up code for a Telemetry Monitor and logger for a TI controller board.
// This code will 'search' for the controller (or steal a session) and synch up with it using 
// using various synch commands.
//
// Once synch'd, the controller will stream data very quickly to this Telemetry monitor.
// UDP was utilized in order to reduce traffic overhead.  
//
// Within this code is a chat window for the controller.  The operator can send various
// commands to the controller (yes, UDP) and have the controller make on the fly changes to operational
// parameters.  This includes Temperature precesion, server IP's, various movement flags, etc.
//
// The static text boxes for the streamed data were all 'set' to an array of the same type and
// corresponding data set.  The controller does not stream the data out in a specific sequence.
// For instance, two Teperature readings could be sent before an ADC reading and vice versa.
// The arrays correspond to the static fields and essentially mapped to the data.
//
// To Do:
//          1. Display Updates - Working, just need to add stuff..
//          2. Resetting the Logger...  (allow user to select directory)
//          3. Changes to UDP Start Up
//              b. we also want to see if the port changed
//              c. we also want o see if the Socket is null before allocating... see if needs a delete then restart.
//              c(1)...  Now, if we do this, the Tiva NEEDS recognize S1, etc... and reset accordingly...
//          4. Use Current Directory for Logging...
//          5. Display Appropriate Messages to Status Line  (Validate)
//          6. What happens when we loose contact with the Tiva?
//          7. Hour Glass on Connect.
//          8. Seems to be a bug receiving incoming data.  Packects seem to get dropped.
//          9. Seem to be dropping data when data streaming at high volume.  
//              Change code to be more performant
//              Investigate if unnecessary code is coming from Tiva
//          10. Add Copyright notice



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;

using System.Net;
using System.Net.Sockets;

using Namespace_Logger;
using Namespace_UDP_Controls;
using Namespace_Form_Updates;
using Namespace_EEPROM_Update_Form;


/* https://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx  */
/* https://msdn.microsoft.com/en-us/library/5w7b7x5f(v=vs.110).aspx  */
/* https://msdn.microsoft.com/en-us/library/w89fhyex(v=vs.110).aspx  */  // Examples



namespace SolarData
{
    public partial class SolarData : Form
    {

        public UDP_Controls UDP = null;
        public Logger Message_Logger = null;
        public Form_Updates FU = null;
        public EEPROM_Update_Form EEPROM_Command_Update = null;

        private bool m_bInitializeNetworking = false;

        Label[] EEPROM_Labels = new Label[16];

        Label[] Temperature_Labels_F = new Label[16];
        Label[] Temperature_Labels_C = new Label[16];
        Label[] Temperature_Labels_Flag = new Label[16];
        Label[] Temperature_Labels_ROM = new Label[16];

        Label[] Pump_Labels = new Label[10];
        
        Label[] Runtime_Labels = new Label[13];

        Label[] Movement_Labels = new Label[13];

        Label[] Support_Labels = new Label[8];

        public SolarData()
        {
            string strPlace = "SolarData()";

            InitializeComponent();


            Message_Logger = new Logger();
            if (Message_Logger != null)
            {
                Message_Logger.OpenFile("c:\\Logs\\MessageLogs.txt");
            }

            FU = new Form_Updates();
            if (UDP != null)
            {
                FU.Set_Logger(Message_Logger);
            }


            UDP = new UDP_Controls();
            if (UDP != null)
            {
                UDP.Set_Logger(Message_Logger);
                UDP.Set_Form_Updates(FU);
                UDP.Set_EEPROM_Command_Updates(ref EEPROM_Command_Update);
            }


            EEPROM_Command_Update = new EEPROM_Update_Form();
            if (EEPROM_Command_Update != null)
            {
                EEPROM_Command_Update.Set_Logger(Message_Logger);
                EEPROM_Command_Update.Set_UDP_Communications(UDP);
                EEPROM_Command_Update.Set_Form_Updates(FU);
            }


            // Message Display
            FU.Set_Display_Label(ref Label_Messages);
            FU.Set_IP_Label(ref Label_Remote_IP);


            // EEPROM Display
            EEPROM_Labels[0] = Label_Version;
            EEPROM_Labels[1] = Label_Reboot_Count;
            EEPROM_Labels[2] = Label_Processor_Speed;
            EEPROM_Labels[3] = Label_Temperature_Resolution;
            EEPROM_Labels[4] = Label_Proximity_V_Threshold;
            EEPROM_Labels[5] = Label_Failsafe_V_Threshold;
            EEPROM_Labels[6] = Label_Max_Wind_Speed;
            EEPROM_Labels[7] = Label_Max_Wind_Speed_Delay;
            EEPROM_Labels[8] = Label_UART_Telemetry;
            EEPROM_Labels[9] = Label_UDP_Telemetry;
            EEPROM_Labels[10] = Label_Use_DNS;
            EEPROM_Labels[11] = Label_UDP_Port;
            EEPROM_Labels[12] = Label_Server_Name;
            EEPROM_Labels[13] = Label_Dark_Threshold;
            EEPROM_Labels[14] = Label_Delay_Sudden_Moveback;
            EEPROM_Labels[15] = Label_Move_Threshold;

            FU.Set_EEPROM_Labels(ref EEPROM_Labels);
            FU.Set_Display_EEPROM_Button(ref Button_EEPROM_Update);

            FU.FU_strEEPROMUpdateText = Button_EEPROM_Update.Text;

            FU.Set_EEPROM_Editing_State(1);



            Temperature_Labels_F[0] = Label_F_0;
            Temperature_Labels_F[1] = Label_F_1;
            Temperature_Labels_F[2] = Label_F_2;
            Temperature_Labels_F[3] = Label_F_3;
            Temperature_Labels_F[4] = Label_F_4;
            Temperature_Labels_F[5] = Label_F_5;
            Temperature_Labels_F[6] = Label_F_6;
            Temperature_Labels_F[7] = Label_F_7;
            Temperature_Labels_F[8] = Label_F_8;
            Temperature_Labels_F[9] = Label_F_9;
            Temperature_Labels_F[10] = Label_F_10;
            Temperature_Labels_F[11] = Label_F_11;
            Temperature_Labels_F[12] = Label_F_12;
            Temperature_Labels_F[13] = Label_F_13;
            Temperature_Labels_F[14] = Label_F_14;
            Temperature_Labels_F[15] = Label_F_15;
            

            Temperature_Labels_C[0] = Label_C_0;
            Temperature_Labels_C[1] = Label_C_1;
            Temperature_Labels_C[2] = Label_C_2;
            Temperature_Labels_C[3] = Label_C_3;
            Temperature_Labels_C[4] = Label_C_4;
            Temperature_Labels_C[5] = Label_C_5;
            Temperature_Labels_C[6] = Label_C_6;
            Temperature_Labels_C[7] = Label_C_7;
            Temperature_Labels_C[8] = Label_C_8;
            Temperature_Labels_C[9] = Label_C_9;
            Temperature_Labels_C[10] = Label_C_10;
            Temperature_Labels_C[11] = Label_C_11;
            Temperature_Labels_C[12] = Label_C_12;
            Temperature_Labels_C[13] = Label_C_13;
            Temperature_Labels_C[14] = Label_C_14;
            Temperature_Labels_C[15] = Label_C_15;


            Temperature_Labels_Flag[0] = Label_Flag_0;
            Temperature_Labels_Flag[1] = Label_Flag_1;
            Temperature_Labels_Flag[2] = Label_Flag_2;
            Temperature_Labels_Flag[3] = Label_Flag_3;
            Temperature_Labels_Flag[4] = Label_Flag_4;
            Temperature_Labels_Flag[5] = Label_Flag_5;
            Temperature_Labels_Flag[6] = Label_Flag_6;
            Temperature_Labels_Flag[7] = Label_Flag_7;
            Temperature_Labels_Flag[8] = Label_Flag_8;
            Temperature_Labels_Flag[9] = Label_Flag_9;
            Temperature_Labels_Flag[10] = Label_Flag_10;
            Temperature_Labels_Flag[11] = Label_Flag_11;
            Temperature_Labels_Flag[12] = Label_Flag_12;
            Temperature_Labels_Flag[13] = Label_Flag_13;
            Temperature_Labels_Flag[14] = Label_Flag_14;
            Temperature_Labels_Flag[15] = Label_Flag_15;


            Temperature_Labels_ROM[0] = Label_ROM_0;
            Temperature_Labels_ROM[1] = Label_ROM_1;
            Temperature_Labels_ROM[2] = Label_ROM_2;
            Temperature_Labels_ROM[3] = Label_ROM_3;
            Temperature_Labels_ROM[4] = Label_ROM_4;
            Temperature_Labels_ROM[5] = Label_ROM_5;
            Temperature_Labels_ROM[6] = Label_ROM_6;
            Temperature_Labels_ROM[7] = Label_ROM_7;
            Temperature_Labels_ROM[8] = Label_ROM_8;
            Temperature_Labels_ROM[9] = Label_ROM_9;
            Temperature_Labels_ROM[10] = Label_ROM_10;
            Temperature_Labels_ROM[11] = Label_ROM_11;
            Temperature_Labels_ROM[12] = Label_ROM_12;
            Temperature_Labels_ROM[13] = Label_ROM_13;
            Temperature_Labels_ROM[14] = Label_ROM_14;
            Temperature_Labels_ROM[15] = Label_ROM_15;

            FU.Set_Temperature_Labels(ref Temperature_Labels_F, ref Temperature_Labels_C, ref Temperature_Labels_Flag, ref Temperature_Labels_ROM);


            Pump_Labels[0] = Label_Command;
            Pump_Labels[1] = Label_Pump_Thermostat;
            Pump_Labels[2] = Label_Pump_Dish;
            Pump_Labels[3] = Label_Pump_Dish_Status;
            Pump_Labels[4] = Label_Pump_Immd_Res;
            Pump_Labels[5] = Label_Pump_Immd_Res_Status;
            Pump_Labels[6] = Label_Pump_Hold_Res;
            Pump_Labels[7] = Label_Pump_Hold_Res_Status;
            Pump_Labels[8] = Label_Pump_AUX;
            Pump_Labels[9] = Label_Pump_AUX_Status;
            FU.Set_Pump_Labels(ref Pump_Labels);


            Movement_Labels[0] = Label_Command;
            Movement_Labels[1] = Label_H_R_Low;
            Movement_Labels[2] = Label_H_L_Low;
            Movement_Labels[3] = Label_H_R_Med_1;
            Movement_Labels[4] = Label_H_L_Med_1;
            Movement_Labels[5] = Label_H_Total;
            Movement_Labels[6] = Label_H_Status;
            Movement_Labels[7] = Label_V_U_Low;
            Movement_Labels[8] = Label_V_D_Low;
            Movement_Labels[9] = Label_V_U_Med_1;
            Movement_Labels[10] = Label_V_D_Med_1;
            Movement_Labels[11] = Label_V_Total;
            Movement_Labels[12] = Label_V_Status;
            FU.Set_Movement_Labels(ref Movement_Labels);

            
            Support_Labels[0] = Label_Command;
            Support_Labels[1] = Label_Windspeed;
            Support_Labels[2] = Label_Windspeed_Status;
            Support_Labels[3] = Label_FS_Flag;
            Support_Labels[4] = Label_PX_R_Flag;
            Support_Labels[5] = Label_PX_L_Flag;
            Support_Labels[6] = Label_PX_U_Flag;
            Support_Labels[7] = Label_PX_D_Flag;
            FU.Set_Support_Labels(ref Support_Labels);


            Runtime_Labels[0] = Label_Command;
            Runtime_Labels[1] = Label_Runtime_Seconds;
            Runtime_Labels[2] = Label_Temperature_Cycles;
            Runtime_Labels[3] = Label_Dish_Cycles;
            Runtime_Labels[4] = Label_Pump_Cycles;
            Runtime_Labels[5] = Label_ADC_Cycles;
            Runtime_Labels[6] = Label_Listen_Cycles;
            Runtime_Labels[7] = Label_Heat_Requests;
            Runtime_Labels[8] = Label_Heat_Time_Total;
            Runtime_Labels[9] = Label_EEPROM_Writes;
            Runtime_Labels[10] = Label_Data_Output;
            Runtime_Labels[11] = Label_Temp_Pump_Per_Second;
            Runtime_Labels[12] = Label_Photo_Dish_Per_Second;
            FU.Set_Runtime_Labels(ref Runtime_Labels);

            Post_The_Message(strPlace, "Ready!");
        }


        private void Post_The_Message(string strPlace, string strMessage)
        {

            if (Message_Logger != null)
            {
                string strTemp = String.Format("{0} - {1}", strPlace, strMessage);

                Message_Logger.LogIt(strTemp);
            }

            FU.Set_Label_Display_Text(strMessage);
        }


        private Int32 Initialize_Socket(Int32 iPortNumber)
        {
            string strPlace = "Initialize_Socket";

            Int32 iRtn = 0;

            Message_Logger.LogIt(strPlace + " Initializing Socket()");

            iRtn = UDP.Initialize(iPortNumber);

            if (iRtn == 0)
            {
                Message_Logger.LogIt(strPlace + " Socket() Initialized!");
            }
            else
            {
                Message_Logger.LogIt(strPlace + " Socket() Error While Initializing!");
                return 1001;
            }

            Post_The_Message(strPlace, "UDP.ReceiveFrom() Setup");

            iRtn = UDP.ReceiveFrom();
            if (!iRtn.Equals(0))
            {
                Post_The_Message(strPlace, "Error: UDP.ReceiveFrom()");
                return iRtn;
            }

                       

            return 0;
        }


        private Int32 Send_SYNCH_1_To_Subnet(Int32 iPortNumber)
        {

            string strPlace = "Send_SYNCH_1_To_Subnet";

            Post_The_Message(strPlace, "Sending [SYNCH_1 ] to IPV4 Subnet");

            Int32 iRtn;
            int iBytesSent = 0;



            // start sending out SYNCH_1 ...
            String strIPAddress = null;
            IPAddress ipa_RemoteAddress = null;

            // bReceivedData gets triggered when something comes back from the Tiva.
            // if it does, stop what you are doing and go on....
            for (int iIndex = 1; iIndex < 256 && UDP.bReceivedData == false; iIndex++)
            {
                iRtn = 0;

                Byte[] abAddress = UDP.UDP_IP_Local_Machine.GetAddressBytes();

                strIPAddress = String.Format("{0}.{1}.{2}.{3}", abAddress[0], abAddress[1], abAddress[2], iIndex);


                Post_The_Message(strPlace, "Sending [SYNCH_1 ] To: " + strIPAddress);


                // setup remote IP
                iRtn = UDP.Parse_IP(ref ipa_RemoteAddress, strIPAddress);

                if (!iRtn.Equals(0))
                {
                    Post_The_Message(strPlace, "Error: UDP.Parse_IP()");
                    return iRtn;
                }


                // get the local machine IP and the generated IP and compare them...
                // if equal, we do not want to prcess that IP...
                byte[] abRemoteIP = ipa_RemoteAddress.GetAddressBytes();
                byte[] abLocalIP = UDP.UDP_IP_Local_Machine.GetAddressBytes();

                bool bAddressesTheSame = false;


                iRtn = UDP.Compare_IPs(ref bAddressesTheSame, abRemoteIP, abLocalIP);

                if (!iRtn.Equals(0))
                {
                    Post_The_Message(strPlace, "Error: UDP.Compare_IPs()");
                    return iRtn;
                }


                if (bAddressesTheSame == true)
                {
                    Post_The_Message(strPlace, "             Address of Local Machine.  Skip This IP.");
                }
                else
                {

                    IPEndPoint ipEndpoint = null;

                    iRtn = UDP.GetIPEndPoint(ref ipEndpoint, ipa_RemoteAddress, iPortNumber);

                    if (!iRtn.Equals(0))
                    {
                        Post_The_Message(strPlace, "Error: UDP.GetIPEndPoint()");
                        return iRtn;
                    }



                    // encode and send....  then wait....

                    byte[] S1_Buffer = Encoding.ASCII.GetBytes("SYNCH_1 ");

                    iRtn = UDP.SendTo(S1_Buffer, S1_Buffer.Count(), SocketFlags.None, ipEndpoint, ref iBytesSent);

                    if (!iRtn.Equals(0))
                    {
                        Post_The_Message(strPlace, "Error: UDP.SendTo()");
                        return iRtn;
                    }


                    // OK, the SendTo was OK, we just did not send all of the bytes...
                    if (iBytesSent != S1_Buffer.Count())
                    {
                        Post_The_Message(strPlace, "Error: UDP.SendTo()  (Buffer Count)");
                        return iRtn;
                    }
                }
            }

            return 0;

        }



        private Int32 Connect_With_Tiva(Int32 iPortNumber)
        {

            string strPlace = "Connect_With_Tiva()";

            Int32 iRtn = 0;


            // g_Received_Sent_S3_No_Data_Received
            // bReceivedData_S2 = true;

            if (m_bInitializeNetworking == false)
            {
                iRtn = Initialize_Socket(iPortNumber);
                if (!iRtn.Equals(0))
                {
                    return iRtn;
                }
                m_bInitializeNetworking = true;
            }


            // this will loop through the Subnets sending SYNCH_1 causing a Wait for SYNCH_2
            if (UDP.bReceivedData_S2 == false)
            {
                iRtn = Send_SYNCH_1_To_Subnet(iPortNumber);
                if (!iRtn.Equals(0))
                {
                    Post_The_Message(strPlace, "Problem With Sending [SYNCH_1 ] to Subnet.");
                    return iRtn;
                }
                Post_The_Message(strPlace, "Waiting for Tiva Synch [SYNCH_2 ] Data.");

                return 0;
            }


            Post_The_Message(strPlace, "Resnding [SYNCH_3 ]...");
            // this will not happen until we received a SYNCH_2; but, not get any real data.
            // this resends a SYNCH_3 (as does a portion of the UDP code), to try to get data
            if ((UDP.bReceivedData_S2 == true) && (UDP.g_Received_Sent_S3_No_Data_Received = true))
            {
                iRtn = UDP.Send_Data_To_EndPoint("SYNCH_3 ", 0);

                if (!iRtn.Equals(0))
                {
                    Post_The_Message(strPlace, "Error: UDP.SendTo() - SYNCH_3...");
                    return iRtn;
                }


                Post_The_Message(strPlace, "Sent [SYNCH_3 ], Waiting for Tiva To Data.");
            }




            return 0;
        }


        private void Button_Retrieve_Data_Click(object sender, EventArgs e)
        {

            string strPlace = "Button_Retrieve_Data_Click";

            string strPort = Text_Port.Text;

            int iPort = Int32.Parse(strPort);

            //UseWaitCursor = true;
            Int32 iRtn = Connect_With_Tiva(iPort);
            //UseWaitCursor = false;

            if (iRtn == 0)
            {
                Post_The_Message(strPlace, "Connected!");

                FU.Set_EEPROM_Editing_State(2);
            }
            else
            {
                Post_The_Message(strPlace, "Not Connected!");
            }
            

            Message_Logger.FlushBuffer();
        }

        private void Button_Send_EEPROM_Update_Click(object sender, EventArgs e)
        {
            bool bRecreateObject = false;

            try
            {
                EEPROM_Command_Update.Show();
            }
            catch (ObjectDisposedException eODE)
            {
                bRecreateObject = true;
                Message_Logger.LogIt("Button_Send_EEPROM_Update_Click()Recreate Form " + eODE.Message);
            }


            if (bRecreateObject == true)
            {
                EEPROM_Command_Update = new EEPROM_Update_Form();
                if (EEPROM_Command_Update != null)
                {

                    EEPROM_Command_Update.Set_Logger(Message_Logger);
                    EEPROM_Command_Update.Set_UDP_Communications(UDP);
                    EEPROM_Command_Update.Set_Form_Updates(FU);

                    EEPROM_Command_Update.Show();
                }
            }

        }

        private void SolarData_Load(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label52_Click(object sender, EventArgs e)
        {

        }

    }
}
