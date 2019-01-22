using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterLabEvoLogHelper
{
    public partial class Form1 : Form
    {
        public string destinationPath;
        public string preamble;
        public bool abortFunction;
        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.Text = "Files to process: 0";
            abortFunction = false;
            textBox1.ReadOnly = true;
            textBox1.Text = "";
            btnRead.Text = "&Read Logs";
            toolStripStatusLabel1.Text = "None";
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Consolidation process started" + Environment.NewLine; //Status Notification
            abortFunction = false;//Reset Variable

            if (btnRead.Text == "&Read Logs")
            {
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    btnRead.Text = "&Abort";

                    int i = 0;
                    //
                    // The user selected a folder and pressed the OK button.
                    // We print the number of files found.
                    //
                    textBox1.Text = textBox1.Text + "Selected source path:\n " + folderBrowserDialog1.SelectedPath +
                                    Environment.NewLine; //Status Notification

                    string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                    i = files.Length;
                    this.Text = "Files to process: " + i; //Status Notification

                    // When user clicks button, show the dialog.
                    DialogResult resultSaveDialog = saveFileDialog1.ShowDialog();
                    if (resultSaveDialog == DialogResult.OK)
                    {
                        textBox1.Text = textBox1.Text + "Selected destination path:\n " + saveFileDialog1.FileName +
                                        Environment.NewLine;//Status Notification
                        textBox1.Text = textBox1.Text + "Starting consolidation process" + Environment.NewLine;//Status Notification

                        foreach (var file in files)
                        {
                            toolStripStatusLabel1.Text = file;

                            i--;
                            this.Text = "Files to process: " + i;//Status Notification

                            processFiles(file);
                            Application.DoEvents();
                        }
                        
                        if (!abortFunction)
                        {
                            textBox1.Text = textBox1.Text + "Starting sorting process" + Environment.NewLine; //Status Notification
                            toolStripStatusLabel1.Text = "Now sorting";//Status Notification

                            sortFile(destinationPath);

                            textBox1.Text = textBox1.Text + "Consolidation process finished";//Status Notification

                            CreateChunks();

                            this.Text = "Done!";//Status Notification
                            toolStripStatusLabel1.Text = "Finished!";//Status Notification
                            Thread.Sleep(3000);
                            InitializeControls();
                        }
                    }
                    else
                    {
                        btnRead.Text = "&Read Logs";//Reset control
                        textBox1.Text = "";//Reset control

                        toolStripStatusLabel1.Text = "Process aborted!";//Status Notification 
                    }

                }
                else
                {
                    textBox1.Text = "";//Reset control
                    toolStripStatusLabel1.Text = "Process aborted!";//Status Notification
                    abortFunction = true;
                }
                
            }
            else
            {
                DialogResult abortDialogResult = MessageBox.Show("Do you really want to abort the process?",
                    "Information", MessageBoxButtons.YesNo);
                if (abortDialogResult==DialogResult.Yes)
                {
                    abortFunction = true;
                    Application.DoEvents();
                    textBox1.Text = "";//Reset control
                    toolStripStatusLabel1.Text = "Process aborted!";//Status Notification
                    Thread.Sleep(3000);
                    InitializeControls();
                }
            }
        }

        private void processFiles(string file)
        {
            if (!abortFunction)
            {
                string[] origin = file.Split('\\');
                string[] tag = origin[origin.Length - 1].Split('.');

                foreach (string line in File.ReadLines(file))
                {
                    string newLine = line.TrimStart();//remove leading spaces

                    string[] arrStrings = newLine.Split(' ');

                 if (arrStrings.Length >= 4)
                   {
                        if ((arrStrings[3].ToLower() != "info") && (arrStrings[3].ToLower() != "debug"))
                        {
                            //Check arrStrings[0] is Date, if yes, create string arrString[0] ~ [3] plus " - ". If not add string in front
                            DateTime datetime;
                            if (DateTime.TryParse(arrStrings[0], out datetime))
                            {
                                preamble = arrStrings[0] + " " + arrStrings[1] + " " + arrStrings[2] + " " + arrStrings[3] +
                                           " - ";
                                string errorTestErrorString = CreateErrorString(arrStrings);
                                using (StreamWriter writer = new StreamWriter(destinationPath, true))
                                {
                                    writer.WriteLine(preamble + " " + errorTestErrorString + " " + "@@" + tag[0] + "@@");
                                }
                            }
                            else
                            {
                                using (StreamWriter writer = new StreamWriter(destinationPath, true))
                                {
                                    writer.WriteLine(preamble + "\"" + line + "\"" + " " + "@@" + tag[0] + "@@");
                                }

                            }

                        }
                    }
                    Application.DoEvents();
               }
            }
            else
            {
                return;
            }
         
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // Get file name.
            destinationPath = saveFileDialog1.FileName;
            
        }

        private void sortFile(string destinationPath)
        {
            string[] pathin = destinationPath.Split('.');
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

            textBox1.Text = textBox1.Text + "Destination path for sorted file:\n " + pathin[0] + ".txt" +
                            Environment.NewLine;//Status Notification

            File.WriteAllLines(pathin[0]+ ".txt", sorted);
        }

        private string CreateErrorString(string[] lineIn)
        {
            string lineOut = "";

            for (int i = 4; i < lineIn.Length; i++)
            {
                lineOut = lineOut + lineIn[i] + " ";
            }

            lineOut = "\"" + lineOut + "\"";

            return lineOut.TrimEnd();
        }

        private void CreateChunks()
        {
            string compare = "";
            string[] pathin = destinationPath.Split('.');
            string[] pathChunk = destinationPath.Split('\\');
            string outputText = "";
            string outputpath = destinationPath.Replace(pathChunk[pathChunk.Length - 1], "Chunks");
            string outfile= pathChunk[pathChunk.Length-1].Substring(0, pathChunk[pathChunk.Length - 1].Length - 4);

            if (!Directory.Exists(outputpath))
            {
                Directory.CreateDirectory(outputpath);
                pathChunk = destinationPath.Replace(pathChunk[pathChunk.Length - 1], "Chunks").Split('\\');
            }

            var lineCount = File.ReadLines(pathin[0] + ".txt").Count();

            toolStripProgressBar1.Maximum = lineCount;


            foreach (var line in File.ReadLines(pathin[0] + ".txt"))
            {
                string[] toCompare = line.Split(' ');

                if (compare==String.Empty)
                {
                    outputText = outputText + line + Environment.NewLine;
                    compare = toCompare[0];
                }
                else if(compare==toCompare[0])
                {
                    outputText = outputText + line + Environment.NewLine;
                    compare = toCompare[0];
                }
                else
                {   destinationPath = outputpath + "\\" +  compare + outfile + ".txt";

                    // This text is always added, making the file longer over time
                    // if it is not deleted.
                    using (StreamWriter sw = File.AppendText(destinationPath))
                    {
                        sw.Write(outputText);
                        sw.Close();
                        outputText = "";
                        Application.DoEvents();
                    }

                    compare = toCompare[0];
                }

                toolStripProgressBar1.Value = lineCount--;
                toolStripProgressBar1.Text = lineCount.ToString();

                Application.DoEvents();
            }
        }



        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
