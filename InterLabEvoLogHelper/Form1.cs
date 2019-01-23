using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace InterLabEvoLogHelper
{
    public partial class Form1 : Form
    {
        public string destinationPath;
        public string preamble;
        public bool abortFunction;
        public int fileCntProgress = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            toolStripProgressBar1.Visible = false;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            try
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

                            toolStripProgressBar1.Visible = true;

                            foreach (var file in files)
                            {
                                toolStripStatusLabel1.Text = file;

                                i--;
                                this.Text = "Files to process: " + i;//Status Notification

                                

                                processFiles(file);

                                Application.DoEvents();
                            }

                            toolStripProgressBar1.Visible = false;

                            if (!abortFunction)
                            {
                                textBox1.Text = textBox1.Text + "Starting sorting process" + Environment.NewLine; //Status Notification
                                toolStripStatusLabel1.Text = "Now sorting";//Status Notification

                                sortFile(destinationPath);
                                textBox1.Text = textBox1.Text + "Sorting process finished" + Environment.NewLine;

                                textBox1.Text = textBox1.Text + "Consolidation process finished" + Environment.NewLine; ;//Status Notification

                                toolStripStatusLabel1.Text = "Process running, please wait!";

                                textBox1.Text = textBox1.Text + "Creation of sorted and consolidated log started" + Environment.NewLine; ;//Status Notification

                                CreateChunks();

                                textBox1.Text = textBox1.Text + "Creation of sorted and consolidated log finished" + Environment.NewLine; ;//Status Notification

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
                    if (abortDialogResult == DialogResult.Yes)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + "\nBtn Read File");
            }
        }

        private void processFiles(string file)
        {
            try
            {
                if (!abortFunction)
                {
                    fileCntProgress = File.ReadLines(file).Count();//determine number of lines
                    toolStripProgressBar1.Maximum = fileCntProgress;

                    string[] origin = file.Split('\\');
                    string[] tag = origin[origin.Length - 1].Split('.');

                    foreach (string line in File.ReadLines(file))
                    {
                        toolStripProgressBar1.Value = fileCntProgress--;

                        string newLine = line.TrimStart();//remove leading spaces
                        newLine = newLine.TrimEnd();
                        newLine=newLine.Replace("\"", "\'");

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
                    textBox1.Text = textBox1.Text + "Processing Files aborted" + Environment.NewLine;
                    return;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString() + "\nProcess File");
            }
         
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            // Get file name.
            destinationPath = saveFileDialog1.FileName;
            
        }

        private void sortFile(string destinationPath)
        {
            try
            {
                string[] pathin = destinationPath.Split('.');
                DateTime dateTime;
                string[] lines = File.ReadAllLines(destinationPath);
                var data = lines;
                var sorted = data.Select(line => new
                {
                    SortKey = DateTime.ParseExact(line.Split(' ')[0], "yyyy-MM-dd", null),
                    SortKeyThenBy = DateTime.TryParse(line.Split(' ')[1], out dateTime),
                    Line = line
                })
                    .OrderByDescending(x => x.SortKey).ThenByDescending(x => x.SortKeyThenBy)
                    .Select(x => x.Line);

                textBox1.Text = textBox1.Text + "Destination path for sorted file:\n " + pathin[0] + "_sorted.txt" +
                                Environment.NewLine;//Status Notification

                File.WriteAllLines(pathin[0] + ".txt", sorted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + "\nSortFile");
            }
        }

        private string CreateErrorString(string[] lineIn)
        {
            try
            {
                string lineOut = "";

                for (int i = 4; i < lineIn.Length; i++)
                {
                    lineOut = lineOut + lineIn[i] + " ";
                    
                }
                
                lineOut = "\"" + lineOut + "\"";

                return lineOut.TrimEnd();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString() + "\nCreate Error String");
                return String.Empty;
            }
        }

        private void CreateChunks()
        {
            try
            {
                string compare = "";
                string comparePath="";
                string[] pathin = destinationPath.Split('.');
                string[] pathChunk = destinationPath.Split('\\');
                string outputText = "";
                string outputpath = destinationPath.Replace(pathChunk[pathChunk.Length - 1], "Chunks");
                string outfile = pathChunk[pathChunk.Length - 1].Substring(0, pathChunk[pathChunk.Length - 1].Length - 4);

                if (!Directory.Exists(outputpath))
                {
                    Directory.CreateDirectory(outputpath);
                    pathChunk = destinationPath.Replace(pathChunk[pathChunk.Length - 1], "Chunks").Split('\\');
                }

                var lineCount = File.ReadLines(pathin[0] + ".txt").Count();

                toolStripProgressBar1.Maximum = lineCount;
                toolStripProgressBar1.Visible = true;
                toolStripStatusLabel1.Text = "Lines to process: " + lineCount;
                
                //using (StreamWriter sw = File.AppendText(destinationPath))

                    foreach (var line in File.ReadLines(pathin[0] + ".txt"))
                    {
                        string[] toCompare = line.Split(' ');

                        compare = toCompare[0];

                        // This text is always added, making the file longer over time
                        //if it is not deleted.

                        destinationPath = outputpath + "\\" + compare + "_" + outfile + ".txt";
                        File.AppendAllText(destinationPath, line + Environment.NewLine);

                        if (comparePath==String.Empty || comparePath != destinationPath)
                        {
                            comparePath = destinationPath;
                            textBox1.Text = textBox1.Text + "File created: " + destinationPath + Environment.NewLine;
                            textBox1.Select(textBox1.Text.Length, 0);
                            Application.DoEvents();
                        }
                        
                    toolStripProgressBar1.Value = lineCount--;
                    toolStripStatusLabel1.Text = "Lines to process: " + lineCount;

                    Application.DoEvents();
                    }
                toolStripProgressBar1.Visible = false;
            }
            catch (Exception ex)
            {

               MessageBox.Show(ex.ToString() + "\nCreate Chunks");
            }
        }



        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        
    }
}
