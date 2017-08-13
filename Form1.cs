/*
[PC-Tool Messdatenlogger]

Copyright (C) [2017]  [Sebastian Bürger]

Dieses Programm ist freie Software. Sie können es unter den Bedingungen der GNU General Public License, wie von der Free Software Foundation veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version.

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License.

Sie sollten ein Exemplar der GNU General Public License zusammen mit diesem Programm erhalten haben. Falls nicht, siehe <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace RS232
{
    public partial class fclsRS232Tester : Form
    {
        string InputData = String.Empty;
        delegate void SetTextCallback(string text);
 
        public fclsRS232Tester()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cmbComSelect.Items.Add(port);
            }
        }

        private void cmbComSelect_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (port.IsOpen) port.Close();
            port.PortName = cmbComSelect.SelectedItem.ToString();
            stsStatus.Text = port.PortName + ": Baudrate: 9600, 8N1";
            try
            {
                port.Open();
            }
            catch
            {
                MessageBox.Show("Seriell Port " + port.PortName + " kann nicht geöffnet werden!", "PC-Tool Messdatenlogger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbComSelect.SelectedText = "";
                stsStatus.Text = "Der Seriell Port kann nicht geöffnet werden!";
            }
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                InputData = port.ReadLine();
                if (InputData != String.Empty)
                {
                    try
                    {
                        string source = InputData;
                        string[] stringSeparators = new string[] { "," };
                        string[] result;

                        result = source.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        SetText(DateTime.Now.ToString("HH:mm:ss -- dd.MM.yy") + "\n" + "Messwert 1: " + result[0] + "\n" + "Messwert 2: " + result[1] + "\n" + "Messwert 3: " + result[2] + "\n" + "-------------------------------" + "\n");
                    }
                    catch(Exception)
                    {
                        SetText("[Fehler!] Messwerte sind ungültig!");
                    }
                }
            }
        }
       
        private void SetText(string text)
        {
            if (this.txtIn.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtIn.Text += text;
                this.txtIn.SelectionStart = txtIn.Text.Length;
                this.txtIn.ScrollToCaret();
            }
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            txtIn.Clear();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = DateTime.Now.ToString("HH:mm:ss -- dd.MM.yy");
        }

        private int conv_Date2Timestam()
        {
            DateTime date1 = new DateTime(1970, 1, 1);
            DateTime date2 = DateTime.Now;
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);
            return (Convert.ToInt32(ts.TotalSeconds));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("setdate.bat", cmbComSelect.Text);
            Thread.Sleep(20000);
            StringBuilder strb_timestamp = new StringBuilder();
            strb_timestamp.Append("T");
            strb_timestamp.Append(Convert.ToString(conv_Date2Timestam()));
            stsStatus.Text = strb_timestamp.ToString();
            try
            {
                port.WriteLine(strb_timestamp.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("Der angegebene Seriell Port von Messdatenlogger ist ungültig", "Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Thread.Sleep(5000);
            Process.Start("flash.bat", cmbComSelect.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("flash.bat", cmbComSelect.Text);
            stsStatus.Text = "Firmware wurde aktuallisiert";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InfoForm f = new InfoForm();
            f.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile1 = new SaveFileDialog();
            saveFile1.DefaultExt = "*.txt";
            saveFile1.Filter = "Textdatei|*.txt";
            try
            {
                if (saveFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                   saveFile1.FileName.Length > 0)
                {
                    txtIn.SaveFile(saveFile1.FileName, RichTextBoxStreamType.PlainText);
                }
            }
            catch(IOException)
            {
                MessageBox.Show("Messwerte können nicht abgespeichert werden!\n Bitte versuchen sie es erneut in einen anderen Ordner oder versuchen es mit einen anderen Dateinamen erneut.", "Fehler!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
