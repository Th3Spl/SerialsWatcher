using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialsWatcher
{
    public partial class MainForm : Form
    {
        //Taskbar
        bool mouseDown;
        private Point offset;

        //Program
        bool executed = false;

        public MainForm()
        {
            InitializeComponent();
        }

        //Close
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Minimize
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        //TaskBar
        private void guna2CustomGradientPanel2_MouseDown(object sender, MouseEventArgs e)
        {
            offset.X = e.X;
            offset.Y = e.Y;
            mouseDown = true;
        }

        private void guna2CustomGradientPanel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - offset.X, currentScreenPos.Y - offset.Y);
            }
        }

        private void guna2CustomGradientPanel2_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    
    
        //Cmd Excuter
        private string CmdExceuter(string command)
        {
            string output = "";

            //Building the command executer
            using (Process process = new Process())
            {
                //Setting all the attributes we need to execute the cmd command silently
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {command}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                //Reading the output stream of the command
                using (StreamReader reader = process.StandardOutput)
                {
                    //Saving the output within the output variable
                    output = reader.ReadToEnd();
                }

                //Waiting for the command to finish its execution
                process.WaitForExit();
            }

            //Returning the output
            return output;
        }


        //Assigning the Serials to the textboxs
        private void btnCheckSerials_Click(object sender, EventArgs e)
        {
            executed = true;

            // BIOS Serial Number TextBox
            txbBIOSSerialNumber.Text = CmdExceuter("wmic bios get serialnumber").Substring(17).Trim();

            // BIOS UUID
            txbBIOSUUID.Text = CmdExceuter("wmic csproduct get uuid").Substring(39).Trim();

            // CPU Serial Number
            txbCPUSerialNumber.Text = CmdExceuter("wmic cpu get serialnumber").Substring(17).Trim();

            // Processor Id
            txbProcessorId.Text = CmdExceuter("wmic cpu get processorid").Substring(19).Trim();

            // Basboard Serial Number
            txbBaseboardSerialNumber.Text = CmdExceuter("wmic baseboard get serialnumber").Substring(17).Trim();

            // MAC Address
            txbMACAddress.Text = CmdExceuter("wmic path Win32_NetworkAdapter where \"PNPDeviceID like '%%PCI%%' AND NetConnectionStatus=2 AND AdapterTypeID='0'\" get MacAddress").Substring(20).Trim();


            //We need these variables for the cycles
            int last = 0;
            string RAMs = CmdExceuter("wmic memorychip get serialnumber");
            string DISKs = CmdExceuter("wmic diskdrive get serialnumber");


            lsbDiskDrives.Items.Clear(); //We have to clear the listbox
            for (int i = 0; i < DISKs.Length; i++)
            {
                if (DISKs[i] == '\n')
                {
                    lsbDiskDrives.Items.Add(DISKs.Substring(last, i - last));
                    last = i;
                }
            }


            last = 0;   //We reset the index
            lsbRAMSerials.Items.Clear(); //We have to clear the listbox
            for (int i = 0; i < RAMs.Length; i++)
            {
                if (RAMs[i] == '\n')
                {
                    lsbRAMSerials.Items.Add(RAMs.Substring(last, i - last));
                    last = i;
                }
            }
        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            if (executed == true)
            {
                //Building the final string
                string serials = "Serials:\n" +
                    "\n\nBIOS Serial Number: \n" + txbBIOSSerialNumber.Text +
                    "\n\nBIOS UUID: \n" + txbBIOSUUID.Text +
                    "\n\nCPU Serial Number: \n" + txbCPUSerialNumber.Text +
                    "\n\nProcessor Id: \n" + txbProcessorId.Text +
                    "\n\nBasebaord Serial Number: \n" + txbBaseboardSerialNumber.Text +
                    "\n\nMAC Address: \n" + txbMACAddress.Text;

                //Saving on the file
                Random rand = new Random();
                int random = rand.Next(1001, 9999);
                StreamWriter stream = new StreamWriter(Application.StartupPath + "\\SerialsCheck_" + random + ".txt");
                stream.WriteLine(serials);
                stream.Flush();
                stream.Close();

                //Messagbox
                MessageBox.Show("File Written Correctly! \nPath: " + Application.StartupPath + "\\SerialsCheck_" + random + ".txt" + "\nCopied to clipboard!", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Clipboard.SetText(Application.StartupPath + "\\SerialsCheck_" + random + ".txt");

            }else
            {
                MessageBox.Show("You should fetch the serials before!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
