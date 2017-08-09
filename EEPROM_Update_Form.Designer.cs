namespace Namespace_EEPROM_Update_Form
{
    partial class EEPROM_Update_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Listbox_Command_Dialog = new System.Windows.Forms.ListBox();
            this.SL_Command = new System.Windows.Forms.Label();
            this.TextBox_Command = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Listbox_Command_Dialog
            // 
            this.Listbox_Command_Dialog.FormattingEnabled = true;
            this.Listbox_Command_Dialog.ItemHeight = 15;
            this.Listbox_Command_Dialog.Location = new System.Drawing.Point(14, 31);
            this.Listbox_Command_Dialog.Name = "Listbox_Command_Dialog";
            this.Listbox_Command_Dialog.Size = new System.Drawing.Size(486, 364);
            this.Listbox_Command_Dialog.TabIndex = 0;
            // 
            // SL_Command
            // 
            this.SL_Command.AutoSize = true;
            this.SL_Command.Location = new System.Drawing.Point(10, 429);
            this.SL_Command.Name = "SL_Command";
            this.SL_Command.Size = new System.Drawing.Size(63, 15);
            this.SL_Command.TabIndex = 2;
            this.SL_Command.Text = "Command:";
            // 
            // TextBox_Command
            // 
            this.TextBox_Command.Location = new System.Drawing.Point(84, 426);
            this.TextBox_Command.Name = "TextBox_Command";
            this.TextBox_Command.Size = new System.Drawing.Size(416, 23);
            this.TextBox_Command.TabIndex = 3;
            this.TextBox_Command.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_Command_KeyDown);
            // 
            // EEPROM_Update_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 482);
            this.Controls.Add(this.TextBox_Command);
            this.Controls.Add(this.SL_Command);
            this.Controls.Add(this.Listbox_Command_Dialog);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "EEPROM_Update_Form";
            this.Text = "EEPROM_Update_Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EEPROM_Update_Form_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox Listbox_Command_Dialog;
        private System.Windows.Forms.Label SL_Command;
        private System.Windows.Forms.TextBox TextBox_Command;
    }
}