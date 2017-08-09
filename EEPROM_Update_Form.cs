
// Project: Solar System
//
// Rick Faszold
//
// XEn, LLC
//
// August 9th, 2017
//
// Chat window code.
// 
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Namespace_Logger;
using Namespace_UDP_Controls;
using Namespace_Form_Updates;

namespace Namespace_EEPROM_Update_Form
{



    public partial class EEPROM_Update_Form : Form
    {
        public Logger EUF_Logger = null;
        public UDP_Controls EUF_UDP = null;
        public Form_Updates EUF_FU = null;


        public EEPROM_Update_Form()
        {
            InitializeComponent();
        }


        public void Set_Logger(Logger Message_Logger)
        {
            EUF_Logger = Message_Logger;
        }

        public void Set_UDP_Communications(UDP_Controls UDP)
        {
            EUF_UDP = UDP;
        }
        
    
        public void Set_Form_Updates(Form_Updates FU)
        {
            EUF_FU = FU;

            EUF_FU.Set_Command_ListBox(ref Listbox_Command_Dialog);
        }

                         
        private void TextBox_Data_Exchange_TextChanged(object sender, EventArgs e)
        {
            int i = 0;
            i++;
        }

        private void TextBox_Command_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                string strTextString = "COMMAND " + TextBox_Command.Text;

                EUF_UDP.Send_Data_To_EndPoint(strTextString, 13);

                TextBox_Command.Text = "";
            }

        }

        private void EEPROM_Update_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            //EUF_Logger delete;
            //delete EUF_UDP;
        }
    }
}
