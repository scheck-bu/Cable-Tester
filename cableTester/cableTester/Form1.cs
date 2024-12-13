using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;

namespace cableTester
{
    public struct SWITCHSETTING_
    {
        public int PORT;
        public string PATTERN;
    }

    public partial class Form1 : Form
    {
        // default national instruments path
        string niPath = @"C:\Program Files\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current";
        string configFile = Application.StartupPath + @"\CableTesterConfig.txt";

        string WindowTitle = "Cable Tester:";

        io_frm digitalIOForm = new io_frm();

        Dictionary<string,string> swSettings = new Dictionary<string,string>();

        int equipLoops = 0;
        bool stop = false;
        bool initialized = false;

        const int FROMTP = 0;
        const int TOTP = 1;
        const int MINCOLUMN = 2;
        const int MAXCOLUMN = 3;
        const int MEASCOLUMN = 4;
        const int UNITSCOLUMN = 5;
        const int RANGE = 6;
        const int IO1 = 7;
        const int IO2 = 8;
        const int TESTMETHOD = 9;
        const int PRECISION = 10;
        const int STATUSCOLUMN = 6;
        const int NUMBERICWIDTH = 65;
        const int FROMCOLUMNWIDTH = 120;
        const int TOCOLUMNWIDTH = 120;
        const int OPENLIMIT = 90000000;
        const double METER_ERR = -9E+37;
        const double NUMERIC_ERR = -8E+37;
        const int PANEL1 = 1;
        const int PANEL2 = 2;
        Font printFont;
        bool graphPrint = false;

        string dataDir = @"c:\cableTesterData";
        string dataFileName = "";

        private SWITCHSETTING_ switchSetting;

        CableEquipLibrary.cableEquipLib_c eh;
        KeithleyLibrary.keithley_c keithley;
        string tName = "";
        string mode = "";
        string pn = "";
        string[,] testArr = new string[1, 1];

        System.IO.StreamReader streamToPrint;

        dynamic dioc;

        long errNum;

        private void loadSWFile(string fileName)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
            swSettings.Clear();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                
                string[] elements = line.Split('=');
                if (elements.Length > 1)
                {
                    swSettings.Add(elements[0], elements[1]);
                }
            }

        }

        private void setSw(SWITCHSETTING_ switchSetting)
        {
            string status;

            if (errNum == 0)
            {
                try
                {

                    dioc.outPort(switchSetting.PORT, switchSetting.PATTERN);
                    digitalIOForm.DigitalIOCtrl1.refreshIOGrid();
                }
                catch (Exception ex)
                {
                    System.IO.StreamWriter f = new System.IO.StreamWriter(@"c:\testSequencer\11887_debug.txt");
                    f.WriteLine(ex.Message);
                    f.Close();
                    errNum = -999999999999;
                }
            }
            System.Threading.Thread.Sleep(200);
        }

        private long BinToHex(string bits)
        {
            long lswSetting = 0;
            if (bits != "")
            {
                lswSetting = 2 * BinToHex(bits.Substring(0, bits.Length - 1)) + Convert.ToInt64(bits.Substring(bits.Length - 1, 1));
            }
            return lswSetting;
        }

        private void setSwitches(string setting)
        {
            string[] tempArr;
            int cntr;
            SWITCHSETTING_ s;

            tempArr = setting.Split(',');

            for (cntr = 0; cntr < tempArr.Length; cntr += 2)
            {
                s.PORT = Convert.ToInt32(tempArr[cntr]);
                s.PATTERN = Convert.ToString(tempArr[cntr + 1]);
                setSw(s);
            }
        }

        public Form1()
        {
            InitializeComponent();

            //add column headers to test listing
            listView1.Columns.Add("From", FROMCOLUMNWIDTH, HorizontalAlignment.Left);
            listView1.Columns.Add("To", TOCOLUMNWIDTH, HorizontalAlignment.Left);
            listView1.Columns.Add("Min", NUMBERICWIDTH, HorizontalAlignment.Left);
            listView1.Columns.Add("Max", NUMBERICWIDTH, HorizontalAlignment.Left);
            listView1.Columns.Add("Meas", NUMBERICWIDTH, HorizontalAlignment.Left);
            listView1.Columns.Add("Units", 50, HorizontalAlignment.Right);
            listView1.Columns.Add("Status", listView1.Width - (FROMCOLUMNWIDTH + TOCOLUMNWIDTH), HorizontalAlignment.Left);

            listView1.Items.Clear();
            try
            {
                initSequenceDisplay();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("init sequence: " + ex.Message);
            }

            try
            {
                readConfigFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("read config: " + ex.Message);
            }
            eh = new CableEquipLibrary.cableEquipLib_c();
        }

        private void readConfigFile()
        {
            if (System.IO.File.Exists(configFile))
            {
                StreamReader sr = new StreamReader(configFile);

                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] elements = line.Split('=');
                    if (elements[0] == "dataDir")
                    {
                        dataDir = elements[1];
                    }
                    else if(elements[0] == "meterSN")
                    {
                        meterSN_tb.Text = elements[1];
                    }
                    else if (elements[0] == "NIDriversDir")
                    {
                        niPath = elements[1];
                    }
                }
            }
            else
            {
                throw new Exception("Can not locate configuration file " + configFile);
            }
        }

        private void initSelfTestDisplay()
        {
            ListViewItem lvItem;
            int cntr;
            try
            {
                if (!initialized)
                {
                    progressBar1.Value = 0;
                    listView1.Items.Clear();
                    listView1.Refresh();

                    for (int port = 1; port <= 6; port++)
                    {
                        for (int bit = 1; bit <= 8; bit++)
                        {
                            lvItem = new ListViewItem("p" + port.ToString() + "b" + bit);
                            lvItem.ImageKey = "test 1";
                            lvItem.SubItems.Add("p" + (port + 6).ToString() + "b" + bit);
                            lvItem.SubItems.Add("0");
                            lvItem.SubItems.Add("1");

                            // add a place holder for the measurement
                            lvItem.SubItems.Add("...");

                            lvItem.SubItems.Add("Ohms");

                            // add a place hoder for the status
                            lvItem.SubItems.Add("");


                            listView1.Items.Add(lvItem);
                        }
                    }
                    progressBar1.Maximum = listView1.Items.Count;
                    initialized = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void initSequenceDisplay()
        {
            try
            {
                if (!initialized)
                {
                    this.Text = WindowTitle + " " + tName;
                    progressBar1.Value = 0;
                    statusPanel.BackColor = Color.LightBlue;
                    statusLbl.Text = "STATUS";
                    listView1.BackColor = Color.White;
                    listView1.Items.Clear();
                    ListViewItem lvItem;

                    int arrLen = testArr.GetLength(1);
                    //Load array into listview
                    if (arrLen > 1)
                    {
                        for (int i = 0; i < arrLen; i++)
                        {
                            lvItem = new ListViewItem(testArr[FROMTP, i]);
                            lvItem.ImageKey = "test 1";
                            lvItem.SubItems.Add(testArr[TOTP, i]);
                            lvItem.SubItems.Add(testArr[MINCOLUMN, i]);
                            lvItem.SubItems.Add(testArr[MAXCOLUMN, i]);

                            // add a place holder for the measurement
                            lvItem.SubItems.Add("...");

                            lvItem.SubItems.Add("Ohms");

                            // add a place hoder for the status
                            lvItem.SubItems.Add("");

                            listView1.Items.Add(lvItem);

                        }
                        progressBar1.Maximum = listView1.Items.Count;
                        initialized = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void refreshEquip()
        {
            try
            {
                equipmentRefreshTimer.Enabled = false;
                start_btn.Enabled = false;
                if (equipLoops > 4)
                {
                    toolStripStatusLabel1.Text = "searching for equipment";
                    equipLoops = 0;
                }
                else
                {
                    toolStripStatusLabel1.Text += '.';
                }
                equipLoops++;
                this.Refresh();
 
                eh.refresh(niPath);

                if (eh.equipment != null)
                {
                    bool found = false;
                    for (int equipCntr = 0; equipCntr < eh.equipment.Length; equipCntr++)
                    {
                        dynamic equip = eh.equipment[equipCntr];
                        found = false;
                        foreach (string model in equipment_lb.Items)
                        {
                            if (equip.model == model)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            equipment_lb.Items.Add(equip.model);

                            if (equip.mfg.ToLower().Contains("keithley"))
                            {
                                keithley = new KeithleyLibrary.keithley_c(eh.equipment[equipCntr].session);
                            }
                            else if (equip.mfg.ToLower().Contains("national"))
                            {
                                digitalIOForm.addDIOCtrl(niPath, eh.equipment[equipCntr].device, eh.equipment[equipCntr].model);
                                dioc = digitalIOForm.DigitalIOCtrl1;
                                dioc.init();
                            }
                        }
                    }

                    this.Refresh();
                    Application.DoEvents();
                }

                if (equipment_lb.Items.Count > 1)
                {
                    start_btn.Enabled = true;
                    toolStripStatusLabel1.Text = "Load sequence file and enter test info.";
                    this.Refresh();
                    Application.DoEvents();
                }
                else
                {
                    equipmentRefreshTimer.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("refresh equip: " + ex.Message);
            }
        }


        private void storeResults(string status)
        {

            toolStripStatusLabel1.Text = "Storing data to " + dataDir;
            if (!System.IO.Directory.Exists(dataDir))
            {
                System.IO.Directory.CreateDirectory(dataDir);
            }

            dataFileName = dataDir + @"\" + DateTime.Now.ToString("MMddyyhhmmss") + ".log";

            StreamWriter dataFileSr = new StreamWriter(dataFileName);

            dataFileSr.WriteLine("Sequence: " + tName);
            dataFileSr.WriteLine("Meter SN: " + meterSN_tb.Text);
            dataFileSr.WriteLine("Meter CAL Due Date: " + meterCalDt_tb.Text);
            dataFileSr.WriteLine("Operator: " + operator_tb.Text);
            dataFileSr.WriteLine("DUT SN: " + dutSN_tb.Text);
            dataFileSr.WriteLine("STATUS: " + status);
            dataFileSr.WriteLine("");

            dataFileSr.WriteLine("FROM_TP,TO_TP,MIN,MAX,MEAS,UNITS,STATUS");
            for (int testCntr = 0; testCntr < listView1.Items.Count; testCntr++)
            {
                // write line to file
                dataFileSr.WriteLine(listView1.Items[testCntr].SubItems[FROMTP].Text + ","
                    + listView1.Items[testCntr].SubItems[TOTP].Text + ","
                    + listView1.Items[testCntr].SubItems[MINCOLUMN].Text + ","
                    + listView1.Items[testCntr].SubItems[MAXCOLUMN].Text + ","
                    + listView1.Items[testCntr].SubItems[MEASCOLUMN].Text + ","
                    + listView1.Items[testCntr].SubItems[UNITSCOLUMN].Text + ","
                    + listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text
                    );
            }
            dataFileSr.Close();

        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            string swSetting;
            string status = "PASS";

            initialized = false;
            initSequenceDisplay();

            toolStripStatusLabel1.Text = "testing...";

            for (int testCntr = 0; testCntr < listView1.Items.Count; testCntr++)
            {
                if(stop) break;
                swSetting = swSettings[testArr[FROMTP, testCntr]];
                
                setSwitches(swSetting);
                swSetting = swSettings[testArr[TOTP, testCntr]];

                setSwitches(swSetting);

                System.Threading.Thread.Sleep(1000);

                double minLim = 0;

                double reading = 0.11;

                turnAllOff();
                listView1.Items[testCntr].SubItems[MEASCOLUMN].Text = reading.ToString();

                if (listView1.Items[testCntr].SubItems[MINCOLUMN].Text == "OPEN")
                {
                    minLim = 9.9E+10;
                }
                else
                {
                    if (listView1.Items[testCntr].SubItems[MINCOLUMN].Text != "NA")
                    {
                        minLim = Convert.ToDouble(listView1.Items[testCntr].SubItems[MINCOLUMN].Text);
                    }
                }

                if (listView1.Items[testCntr].SubItems[MINCOLUMN].Text == "NA")
                {
                    if (reading < Convert.ToDouble(listView1.Items[testCntr].SubItems[MAXCOLUMN].Text))
                    {
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "P";
                        listView1.Items[testCntr].BackColor = Color.LimeGreen;
                    }
                    else
                    {
                        status = "FAIL";
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "F";
                        listView1.Items[testCntr].BackColor = Color.Red;
                    }
                }
                else if (listView1.Items[testCntr].SubItems[MAXCOLUMN].Text == "NA")
                {
                    if (reading > minLim)
                    {
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "P";
                        listView1.Items[testCntr].BackColor = Color.LimeGreen;
                    }
                    else
                    {
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "F";
                        listView1.Items[testCntr].BackColor = Color.Red;
                        status = "FAIL";
                    }
                }
                else
                {
                    if ((reading > minLim) && (reading < Convert.ToDouble(listView1.Items[testCntr].SubItems[MAXCOLUMN].Text)))
                    {
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "P";
                        listView1.Items[testCntr].BackColor = Color.LimeGreen;
                    }
                    else
                    {
                        listView1.Items[testCntr].SubItems[STATUSCOLUMN].Text = "F";
                        listView1.Items[testCntr].BackColor = Color.Red;
                        status = "FAIL";
                    }
                }
                string port;
                string[] tempArr;
                swSetting = swSettings[testArr[FROMTP, testCntr]];
                tempArr = swSetting.Split(',');
                port = tempArr[0];

                resetDIOPort(Convert.ToInt32(port));

                swSetting = swSettings[testArr[TOTP, testCntr]];
                tempArr = swSetting.Split(',');
                port = tempArr[0];
                resetDIOPort(Convert.ToInt32(port));
                
                progressBar1.Value = testCntr;
                progressBar1.Refresh();
                dioc.init();
                Application.DoEvents();
                this.Refresh();
            }
            progressBar1.Value = listView1.Items.Count;
            progressBar1.Refresh();
            if (stop) status = "FAIL";
            statusLbl.Text = status;
            if (status == "PASS")
            {
                statusPanel.BackColor = Color.LimeGreen;
            }
            else { statusPanel.BackColor = Color.Red; }
            storeResults(status);
        }

        private void turnAllOff()
        {
            string swArr = swSettings["AllOff"];

            string[] elements = swArr.Split(';');

            for (int elemCntr = 0; elemCntr < elements.Length; elemCntr++)
            {
                setSwitches(elements[elemCntr]);
            }
        }

        private void resetDIOPort(int port)
        {
            char[] bitArr = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
            string pattern;
 
            pattern = new string(bitArr);
            setSwitches(port.ToString() + "," + pattern);
        }

        private void setDIO(int port, int bit)
        {
            char[] bitArr = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
            string pattern;
            int pl = 8 - bit;

            bitArr[pl] = '1';

            pattern = new string(bitArr);
            setSwitches(port.ToString() + "," + pattern);
        }

        private void selfTest()
        {
            string status = "PASS";
            int listItem;
            listItem = 0;
            

            for (int port = 0; port <= 5; port++)
            {
                for (int bit = 1; bit <= 8; bit++)
                {
                    setDIO(port, bit);
                    setDIO(port + 6, bit);

                    System.Threading.Thread.Sleep(1000);

                    double reading = keithley.Ohms("", "");

                    listView1.Items[listItem].SubItems[MEASCOLUMN].Text = reading.ToString();

                    if ((reading > Convert.ToDouble(listView1.Items[listItem].SubItems[MINCOLUMN].Text)) && (reading < Convert.ToDouble(listView1.Items[listItem].SubItems[MAXCOLUMN].Text)))
                    {
                        listView1.Items[listItem].SubItems[STATUSCOLUMN].Text = "P";
                        listView1.Items[listItem].BackColor = Color.LimeGreen;
                    }
                    else
                    {
                        listView1.Items[listItem].SubItems[STATUSCOLUMN].Text = "F";
                        status = "FAIL";
                        listView1.Items[listItem].BackColor = Color.Red;
                    }
                    dioc.init();
                    Application.DoEvents();
                    this.Refresh();
                    resetDIOPort(port);
                    resetDIOPort(port + 6);
                    progressBar1.Value = listItem;
                    progressBar1.Refresh();
                    listItem++;
                }
            }
            progressBar1.Value = listItem;
            progressBar1.Refresh();
            if (stop) status = "FAIL";
            statusLbl.Text = status;
            if (status == "PASS")
            {
                statusPanel.BackColor = Color.LimeGreen;
            }
            else { statusPanel.BackColor = Color.Red; }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog.FileName = @"c:\clt\lim.csv";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName == "")
            {
                //User canceled.
            }
            else
            {
                loadConfigFile(openFileDialog.FileName);
            }

        }

        void ResizeArray<T>(ref T[,] original, int newCoNum, int newRoNum)
        {
            var newArray = new T[newCoNum, newRoNum];
            int columnCount = original.GetLength(1);
            int columnCount2 = newRoNum;
            int columns = original.GetUpperBound(0);
            for (int co = 0; co <= columns; co++)
                Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
            original = newArray;
        }

        private void loadConfigFile(string fName)
        {
            ListViewItem oLI;
            int i;
            System.IO.StreamReader sr;
            string line = "";
            string[] lineElements;
            int lineCntr;
            int rowCntr = 0;
            int elementCntr;
            int portNum;
            testArr = new string[1, 1];
        
            try
            {
                string tempFileName = fName.Replace("lim ", "sw");
                tempFileName = tempFileName.Replace("csv", "ini");

                loadSWFile(tempFileName);
                sr = new System.IO.StreamReader(fName);

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line != "")
                    {
                        if (line.Substring(0, 2) != @"//")
                        {
                            lineElements = line.Split('=');
                            if (lineElements.Length > 1)
                            {
                                if (lineElements[0].ToLower() == "meter sn")
                                {
                                    meterSN_tb.Text = lineElements[1].Trim().Trim(',');
                                }
                                else if (lineElements[0].ToLower() == "daq sn")
                                {
                                    // do nothing for now
                                }
                                else if (lineElements[0].ToLower() == "mode")
                                {
                                    mode = lineElements[1].Trim().Trim(',');
                                }
                                else if (lineElements[0].ToLower() == "name")
                                {
                                    tName = lineElements[1].Trim().Trim(',');
                                }
                                else if (lineElements[0].ToLower() == "pn")
                                {
                                    pn = lineElements[1].Trim().Trim(',');
                                }
                                else if (lineElements[0].ToLower() == "comm")
                                {
                                    portNum = Convert.ToInt32(lineElements[1].Trim());
                                }
                            }
                            else
                            {
                                lineElements = line.Split(',');
                                if (lineElements.Length > 1)
                                {
                                    ResizeArray(ref testArr, 12, rowCntr + 1);

                                    //'Populate array
                                    testArr[FROMTP, rowCntr] = lineElements[0];
                                    testArr[TOTP, rowCntr] = lineElements[1];
                                    testArr[MINCOLUMN, rowCntr] = lineElements[2];
                                    testArr[MAXCOLUMN, rowCntr] = lineElements[3];
                                    testArr[UNITSCOLUMN, rowCntr] = lineElements[4];
                                    testArr[RANGE, rowCntr] = lineElements[5];
                                    testArr[IO1, rowCntr] = lineElements[6];
                                    testArr[IO2, rowCntr] = lineElements[7];
                                    testArr[TESTMETHOD, rowCntr] = lineElements[8];
                                    testArr[PRECISION, rowCntr] = lineElements[9];
                                    rowCntr++;
                                }
                            }
                        }
                        else
                        {
                            // skip comments
                        }

                    }

                }

                initialized = false;
                //Load array into listview
                initSequenceDisplay();
            }
            catch (Exception ex)
            {
            }
        }

        private void relayTest_btn_Click(object sender, EventArgs e)
        {
            initSelfTestDisplay();
            initialized = false;
            selfTest();
        }

        private void clearDisplay()
        {
            listView1.Items.Clear();
            initSequenceDisplay();
        }

        private void exit_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void equipment_lb_DoubleClick(object sender, EventArgs e)
        {
            if (equipment_lb.SelectedItem.ToString() == "USB-6509")
            {
                digitalIOForm.Show();
            }
        }

        public void print_(string curData)
        {
            try
            {
                streamToPrint = new StreamReader
                (dataFileName);
                try
                {
                    printFont = new Font("Lucida Console", 10);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler
                    (this.printDocument1_PrintPage);
                    pd.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            if (graphPrint)
            {//cwd
                printDocument1_PrintPage(sender, ev);//cwd
            }//cwd
            else //cwd
            {
                float linesPerPage = 0;
                float yPos = 0;
                int count = 0;
                float leftMargin = ev.MarginBounds.Left;
                float topMargin = ev.MarginBounds.Top;
                string line = null;
 
                // Calculate the number of lines per page.
                linesPerPage = ev.MarginBounds.Height /
                printFont.GetHeight(ev.Graphics);

                // Print each line of the file.
                while (count < linesPerPage &&
                 ((line = streamToPrint.ReadLine()) != null))
                {
                    yPos = topMargin + (count *
                    printFont.GetHeight(ev.Graphics));

                    string[] elements = line.Split(',');

                    string formattedLine = "";
                    if (elements.Length > 1)
                    {
                        string statusStr = elements[STATUSCOLUMN];
                        if(statusStr == "F")
                        {
                            statusStr = "* " + statusStr;
                        }

                        formattedLine = elements[FROMTP].PadRight(15)
                        + elements[TOTP].PadRight(15)
                        + elements[MINCOLUMN].PadRight(10)
                        + elements[MAXCOLUMN].PadRight(10)
                        + elements[MEASCOLUMN].PadRight(15)
                        + elements[UNITSCOLUMN].PadLeft(7)
                        + statusStr.PadLeft(8);
                    }
                    else
                    {
                        formattedLine = line;
                    }
                    ev.Graphics.DrawString(formattedLine, printFont, Brushes.Black,
                    leftMargin, yPos, new StringFormat());
                    count++;
                }

                // If more lines exist, print another page.
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
        }

        private void print_btn_Click(object sender, EventArgs e)
        {
            print_(dataFileName);
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void equipmentRefreshTimer_Tick(object sender, EventArgs e)
        {
            refreshEquip();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dioc.init();
        }
    }




}
