using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterLabEvoLogHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                int i = 0;
                //
                // The user selected a folder and pressed the OK button.
                // We print the number of files found.
                //
                string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                //MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                i=files.Length;
                this.Text = "Files to process: " + i;
                foreach (var file in files)
                {
                    foreach (string line in File.ReadLines(file))
                    {
                        string[] arrStrings = line.Split(' ');

                        if (arrStrings.Length >= 4)
                        {
                            if ((arrStrings[3].ToLower() != "info"))
                            {
                                using (StreamWriter writer = new StreamWriter("d:\\temp\\important.txt",true))
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        } 
                        Application.DoEvents();
                    }
                    i--;

                }
            }
        }
    }
}
