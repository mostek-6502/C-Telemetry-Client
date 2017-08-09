// Project: Solar System
//
// Rick Faszold
//
// XEn, LLC
//
// August 9th, 2017
//
// Provides a Thread Safe environment for screen updates.
// 
//


using System;
using System.Collections.Generic;

using System.Drawing;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using Namespace_Logger;


namespace Namespace_Form_Updates
{
    public class Form_Updates
    {
        Logger FU_Message_Logger = null;

        int iMaxEEPROMLabels = 0;
        private Label[] FU_EEPROM_Labels = null;
        private Label FU_Message_Label = null;
        private Label FU_IP_Label = null;

        private Button FU_Button_EEPROM_Update = null;
        private ListBox FU_Command_ListBox = null;

        public string FU_strEEPROMUpdateText = "";


        delegate void SetTextCallback_IP(string strIP);
        delegate void SetTextCallback_EEPROM(UInt32 uiIndex, string text);
        delegate void SetTextCallback_Message(string text);
        delegate void SetStateCallback_EEPROM_Button_Enabled_State(bool bEnabled, string strText);

        delegate void SetTextCallback_Command_ListBox(string strText);


        // temperatures....
        int iMaxTemperatureLabels = 0;
        

        delegate void SetTextCallback_Temperature_F(UInt32 uiIndex, string strF, Color colorBackGround);
        delegate void SetTextCallback_Temperature_C(UInt32 uiIndex, string strC, Color colorBackGround);
        delegate void SetTextCallback_Temperature_Flag(UInt32 uiIndex, string strFlag, Color colorBackGround);
        delegate void SetTextCallback_Temperature_ROM(UInt32 uiIndex, string strROM, Color colorBackGround);


        private Label[] FU_Temperature_Labels_F = null;
        private Label[] FU_Temperature_Labels_C = null;
        private Label[] FU_Temperature_Labels_Flag = null;
        private Label[] FU_Temperature_Labels_ROM = null;



        int iMaxPumpLabels = 0;
        delegate void SetTextCallback_Pump(UInt32 uiIndex, string strF, Color colorBackGround);
        private Label[] FU_Pump_Labels = null;


        int iMaxMovementLabels = 0;
        delegate void SetTextCallback_Movement(UInt32 uiIndex, string strF, Color colorBackGround);
        private Label[] FU_Movement_Labels = null;


        int iMaxSupportLabels = 0;
        delegate void SetTextCallback_Support(UInt32 uiIndex, string strF, Color colorBackGround);
        private Label[] FU_Support_Labels = null;


        int iMaxRuntimeLabels = 0;
        delegate void SetTextCallback_Runtime(UInt32 uiIndex, string strF, Color colorBackGround);
        private Label[] FU_Runtime_Labels = null;



        public void Set_Logger(Logger Message_Logger)
        {
            FU_Message_Logger = Message_Logger;
        }


        private void Log_Message(string strMessage)
        {
            if (FU_Message_Logger != null)
            {
                FU_Message_Logger.LogIt(strMessage);
            }
        }


        private void Log_General_Message(string strPlace, string strMessage)
        {
            string s1 = strMessage.Replace('\n', ' ');
            string s2 = s1.Replace('\r', ' ');
            string s3 = s2.Trim();


            if (FU_Message_Logger != null)
            {
                FU_Message_Logger.LogIt(strPlace + " " + s3);
            }

        }


        // Message Label Section
        public void Set_Display_Label(ref Label Form_Message_Label)
        {
            FU_Message_Label = Form_Message_Label;
        }


        public void Set_Label_Display_Text(string strText)
        {
            if (FU_Message_Label.InvokeRequired)
            {
                SetTextCallback_Message d = new SetTextCallback_Message(Set_Label_Display_Text);
                FU_Message_Label.Invoke(d, new object[] { strText });
            }
            else
            {
                FU_Message_Label.Text = strText;
            }
        }


        public void Set_IP_Label(ref Label Form_IP_Label)
        {
            FU_IP_Label = Form_IP_Label;
        }


        public void Set_Label_IP_Text(string strIP)
        {
            if (FU_IP_Label.InvokeRequired)
            {
                SetTextCallback_IP d = new SetTextCallback_IP(Set_Label_IP_Text);
                FU_IP_Label.Invoke(d, new object[] { strIP });
            }
            else
            {
                FU_IP_Label.Text = strIP;
            }
        }



        // ======================================================================================================================= //
        // Update EEPROM Button Section - This just activates the EEPROM button.
        public void Set_Display_EEPROM_Button(ref Button Button_EEPROM_Update)
        {
            FU_Button_EEPROM_Update = Button_EEPROM_Update;
        }

        public void Set_Display_EEPROM_Button_Enabled_State(bool bState, string strText)
        {
            if (FU_Button_EEPROM_Update.InvokeRequired)
            {
                SetStateCallback_EEPROM_Button_Enabled_State d = new SetStateCallback_EEPROM_Button_Enabled_State(Set_Display_EEPROM_Button_Enabled_State);
                FU_Button_EEPROM_Update.Invoke(d, new object[] { bState, strText });
            }
            else
            {
                FU_Button_EEPROM_Update.Enabled = bState;
                FU_Button_EEPROM_Update.Text = strText;
            }
        }


        public void Set_EEPROM_Editing_State(int iState)
        {

            if ((iState < 0) || (iState > 4))
            {
                string strMessage = String.Format("Invalid EEPROM State Requested: {0}  Valid States 1-4", iState);
                Set_Label_Display_Text(strMessage);
                Log_Message(strMessage);
            }

            if (iState == 1)  // State 1 (Initialize)
            {
                Set_Display_EEPROM_Button_Enabled_State(false, FU_strEEPROMUpdateText);
            }
            else if (iState == 2)  // State 2 (Connected)
            {
                Set_Display_EEPROM_Button_Enabled_State(true, FU_strEEPROMUpdateText);
            }
            else if (iState == 3)  // State 3 (Connected & Request To Update)
            {
                Set_Display_EEPROM_Button_Enabled_State(true, "Close EEPROM Update");
            }
            else if (iState == 4)  // State 4 (Connected & Request To Update)
            {
                Set_Display_EEPROM_Button_Enabled_State(true, FU_strEEPROMUpdateText);
            }
        }


        // EEPROM Data / Display Section
        public void Set_EEPROM_Labels(ref Label[] Form_EEPROM_Labels)
        {
            iMaxEEPROMLabels = Form_EEPROM_Labels.Count();

            FU_EEPROM_Labels = new Label[iMaxEEPROMLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxEEPROMLabels; iIndex++)
            {
                FU_EEPROM_Labels[iIndex] = Form_EEPROM_Labels[iIndex];
            }
        }



        public void Set_Label_EEPROM_Data(UInt32 uiIndex, string strText)
        {
            string strPlace = "Set_Label_EEPROM_Data";

            string strTemp = "";

            if (uiIndex > iMaxEEPROMLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxEEPROMLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_EEPROM_Labels[uiIndex].InvokeRequired)
            {
                SetTextCallback_EEPROM d = new SetTextCallback_EEPROM(Set_Label_EEPROM_Data);
                FU_EEPROM_Labels[uiIndex].Invoke(d, new object[] { uiIndex, strText });
            }
            else
            {
                FU_EEPROM_Labels[uiIndex].Text = strText;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }


        // ======================================================================================================================= //
        public void Set_Pump_Labels(ref Label[] Form_Pump_Labels)
        {
            iMaxPumpLabels = Form_Pump_Labels.Count();

            FU_Pump_Labels = new Label[iMaxPumpLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxPumpLabels; iIndex++)
            {
                FU_Pump_Labels[iIndex] = Form_Pump_Labels[iIndex];
            }
        }


        public void Set_Label_Pump_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_Pump_Data";

            string strTemp = "";

            if (uiIndex > iMaxPumpLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxPumpLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

 
            if (FU_Pump_Labels[uiIndex].InvokeRequired)
            {
                SetTextCallback_Pump d = new SetTextCallback_Pump(Set_Label_Pump_Data);
                FU_Pump_Labels[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Pump_Labels[uiIndex].Text = strText;
                FU_Pump_Labels[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }



        // ======================================================================================================================= //
        public void Set_Movement_Labels(ref Label[] Form_Movement_Labels)
        {
            iMaxMovementLabels = Form_Movement_Labels.Count();

            FU_Movement_Labels = new Label[iMaxMovementLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxMovementLabels; iIndex++)
            {
                FU_Movement_Labels[iIndex] = Form_Movement_Labels[iIndex];
            }
        }


        public void Set_Label_Movement_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_Movement_Data";

            string strTemp = "";

            if (uiIndex > iMaxMovementLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxMovementLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Movement_Labels[uiIndex].InvokeRequired)
            {
                SetTextCallback_Movement d = new SetTextCallback_Movement(Set_Label_Movement_Data);
                FU_Movement_Labels[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround } );
            }
            else
            {
                FU_Movement_Labels[uiIndex].Text = strText;
                FU_Movement_Labels[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);
        }



        // ======================================================================================================================= //
        public void Set_Support_Labels(ref Label[] Support_Labels)
        {
            iMaxSupportLabels = Support_Labels.Count();

            FU_Support_Labels = new Label[iMaxSupportLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxSupportLabels; iIndex++)
            {
                FU_Support_Labels[iIndex] = Support_Labels[iIndex];
            }
        }



        public void Set_Label_Support_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_Support_Data";

            string strTemp = "";

            if (uiIndex > iMaxSupportLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxMovementLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Support_Labels[uiIndex].InvokeRequired)
            {
                SetTextCallback_Support d = new SetTextCallback_Support(Set_Label_Support_Data);
                FU_Support_Labels[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Support_Labels[uiIndex].Text = strText;
                FU_Support_Labels[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);
        }








        // ======================================================================================================================= //
        public void Set_Runtime_Labels(ref Label[] Form_Runtime_Labels)
        {
            iMaxRuntimeLabels = Form_Runtime_Labels.Count();

            FU_Runtime_Labels = new Label[iMaxRuntimeLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxRuntimeLabels; iIndex++)
            {
                FU_Runtime_Labels[iIndex] = Form_Runtime_Labels[iIndex];
            }
        }
        


        public void Set_Label_Runtime_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_Runtime_Data";

            string strTemp = "";

            if (uiIndex > iMaxMovementLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxRuntimeLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }



            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Runtime_Labels[uiIndex].InvokeRequired)
            {
                SetTextCallback_Runtime d = new SetTextCallback_Runtime(Set_Label_Runtime_Data);
                FU_Runtime_Labels[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Runtime_Labels[uiIndex].Text = strText;
                FU_Runtime_Labels[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);
        }




        public void Set_Command_ListBox(ref ListBox Command_ListBox)
        {
            FU_Command_ListBox = Command_ListBox;
        }



        public void Add_Command_ListBox(string strText)
        {
            if (FU_Command_ListBox.InvokeRequired)
            {
                SetTextCallback_Command_ListBox d = new SetTextCallback_Command_ListBox(Add_Command_ListBox); 
                FU_Message_Label.Invoke(d, new object[] { strText });
            }
            else
            {
                try
                {
                    FU_Command_ListBox.Items.Add(strText);
                }
                catch (SystemException eSE)
                {
                    Log_Message("Add_Command_ListBox() Data: " + strText + " System Exception: " + eSE.Message);
                    return;
                }
            }
        }




        public void Set_Temperature_Labels(ref Label[] Temp_Labels_F, ref Label[] Temp_Labels_C, ref Label[] Temp_Labels_Flag, ref Label[] Temp_Labels_ROM)
        {
            iMaxTemperatureLabels = Temp_Labels_F.Count();

            FU_Temperature_Labels_F = new Label[iMaxTemperatureLabels];
            FU_Temperature_Labels_C = new Label[iMaxTemperatureLabels];
            FU_Temperature_Labels_Flag = new Label[iMaxTemperatureLabels];
            FU_Temperature_Labels_ROM = new Label[iMaxTemperatureLabels];

            int iIndex = 0;
            for (iIndex = 0; iIndex < iMaxTemperatureLabels; iIndex++)
            {
                FU_Temperature_Labels_F[iIndex] = Temp_Labels_F[iIndex];
                FU_Temperature_Labels_C[iIndex] = Temp_Labels_C[iIndex];
                FU_Temperature_Labels_Flag[iIndex] = Temp_Labels_Flag[iIndex];
                FU_Temperature_Labels_ROM[iIndex] = Temp_Labels_ROM[iIndex];
            }
        }



        public void Set_Label_F_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_F_Data";

            string strTemp = "";

            if (uiIndex > iMaxTemperatureLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxTemperatureLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Temperature_Labels_F[uiIndex].InvokeRequired)
            {
                SetTextCallback_Temperature_F d = new SetTextCallback_Temperature_F(Set_Label_F_Data);
                FU_Temperature_Labels_F[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Temperature_Labels_F[uiIndex].Text = strText;
                FU_Temperature_Labels_F[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }



        public void Set_Label_C_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_C_Data";

            string strTemp = "";

            if (uiIndex > iMaxTemperatureLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxTemperatureLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Temperature_Labels_C[uiIndex].InvokeRequired)
            {
                SetTextCallback_Temperature_C d = new SetTextCallback_Temperature_C(Set_Label_C_Data);
                FU_Temperature_Labels_C[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Temperature_Labels_C[uiIndex].Text = strText;
                FU_Temperature_Labels_C[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }



        public void Set_Label_Flag_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_Flag_Data";

            string strTemp = "";

            if (uiIndex > iMaxTemperatureLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxTemperatureLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Temperature_Labels_Flag[uiIndex].InvokeRequired)
            {
                SetTextCallback_Temperature_Flag d = new SetTextCallback_Temperature_Flag(Set_Label_Flag_Data);
                FU_Temperature_Labels_Flag[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround} );
            }
            else
            {
                FU_Temperature_Labels_Flag[uiIndex].Text = strText;
                FU_Temperature_Labels_Flag[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }


        public void Set_Label_ROM_Data(UInt32 uiIndex, string strText, Color colorBackGround)
        {
            string strPlace = "Set_Label_ROM_Data";

            string strTemp = "";

            if (uiIndex > iMaxTemperatureLabels - 1)
            {
                strTemp = String.Format("Data ->{0}<-  Index: {1}  Greater than Max Index: {2}", strText, uiIndex, iMaxTemperatureLabels);
                Log_General_Message(strPlace, strTemp);
                return;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (Begin)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);


            if (FU_Temperature_Labels_ROM[uiIndex].InvokeRequired)
            {
                SetTextCallback_Temperature_ROM d = new SetTextCallback_Temperature_ROM(Set_Label_ROM_Data);
                FU_Temperature_Labels_ROM[uiIndex].Invoke(d, new object[] { uiIndex, strText, colorBackGround });
            }
            else
            {
                FU_Temperature_Labels_ROM[uiIndex].Text = strText;
                FU_Temperature_Labels_ROM[uiIndex].BackColor = colorBackGround;
            }


            strTemp = String.Format("Data ->{0}<-  Index: {1}  (End)", strText, uiIndex);
            Log_General_Message(strPlace, strTemp);

        }





    }
}
