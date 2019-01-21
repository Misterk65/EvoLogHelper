using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterLabEvoLogHelper
{
    public partial class Form1 : Form
    {
        public string destinationPath;
        public string preamble;
        public Form1()
        {
            InitializeComponent();
            this.Text = "Files to process: 0";
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
                i=files.Length;
                this.Text = "Files to process: " + i;

                // When user clicks button, show the dialog.
                saveFileDialog1.ShowDialog();

                foreach (var file in files)
                {
                    i--;
                    this.Text = "Files to process: " + i;
                    processFiles(file);
                }
                sortFile(destinationPath);
                this.Text = "Done!";
            }
        }

        private void processFiles(string file)
        {
            string[] origin = file.Split('\\');

            foreach (string line in File.ReadLines(file))
            {
                string[] arrStrings = line.Split(' ');

                if (arrStrings.Length >= 4)
                {
                    if ((arrStrings[3].ToLower() != "info"))
                    {
                        //Check arrStrings[0] is Date, if yes, create string arrString[0] ~ [3] plus " - ". If not add string in front
                        DateTime datetime;
                        if (DateTime.TryParse(arrStrings[0], out datetime))
                        {
                            preamble = arrStrings[0] + " " + arrStrings[1] + " " + arrStrings[2] + " " + arrStrings[3] +
                                       " - ";
                            using (StreamWriter writer = new StreamWriter(destinationPath, true))
                            {
                                writer.WriteLine(line + " " + origin[origin.Length - 1].Substring(0, origin[origin.Length - 1].Length - 4));
                            }
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(destinationPath, true))
                            {
                                writer.WriteLine(preamble + line + " " + origin[origin.Length - 1].Substring(0,origin[origin.Length - 1].Length-4));
                            }
                           
                        }

                       
                    }
                }
                Application.DoEvents();
            }
         
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // Get file name.
            destinationPath = saveFileDialog1.FileName;
            
        }

        private void sortFile(string destinationPath)
        {
            DateTime dateTime;
            string[] lines = File.ReadAllLines(destinationPath);
            var data = lines;
            var sorted = data.Select(line => new
                {
                    SortKey = DateTime.ParseExact(line.Split(' ')[0],"yyyy-MM-dd",null ),
                    SortKeyThenBy = DateTime.TryParse(line.Split(' ')[1], out dateTime),
                Line = line
                })
                .OrderByDescending(x => x.SortKey).ThenByDescending(x => x.SortKeyThenBy)
                .Select(x => x.Line);
            File.WriteAllLines(@"E:\temp\sorteddata.csv", sorted);
        }
    }
}
