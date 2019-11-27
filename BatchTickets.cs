using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Collections;
namespace BatchTickets
{
    public partial class BatchTickets : Form
    {
        public BatchTickets()
        {
            InitializeComponent();
        }
        
        #region "Global Variables"
        private enum State { loggedIn, newTicketWnd, ticketCreation, ticketView, loggedOut, unknown };
        private State status = State.loggedOut;
        //private string webBrowserHtml = "";
        private string ticketUrls = "";
        private int row = -1;
        private int tries = 0;
        private int ticketsCreated = 0;

        private ArrayList settingsToPass;
        private OpenFileDialog ofd;
        private SaveFileDialog sfd;
        private FieldValue fv;
        private const int NATFIELDS = 12; // number of hard coded fields to listView2
        private const string KUSER = "*USER_NAME*";
        //private const string KPASS = KUSER;
        
        private const string KPASS = "*PASSWORD_HERE*"; // work pass
        private const string KURL = "http://esupport.ctdlc.org/staff/"; // work environment
        //private const string KURL = "http://esupport.ctdlc.org/kayakotest/staff/"; // dev environment
        // UI magic
        private delegate void expandFormDelegate(Control c, string s, object o); // for invoking UI from other thread
        #endregion
        #region "UI stuff"
        private static void expandForm(Control c, string s, object o){ // thread safe
            if (c.InvokeRequired)
            {
                c.Invoke(new expandFormDelegate(expandForm), new object[] {c, s, o});
            }
            else
            {
                c.GetType().InvokeMember(s, System.Reflection.BindingFlags.SetProperty, null, c, new object[] { o });
            }
        }

        private void recurseHeight()
        {
            for (int x = 0; x < 300; x++){
                expandForm(this, "Height", this.Height + 1);
                expandForm(this, "Width", this.Width + 3);
            }
        } // end of UI magic
        #endregion
        #region "Buttons"
        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0;
            string line = "";
            if ((ofd = new OpenFileDialog()).ShowDialog() == System.Windows.Forms.DialogResult.OK && File.Exists(ofd.FileName)){
                StreamReader sr;
                try
                {
                    sr = File.OpenText(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); return;
                }
                //Thread t = new Thread(()=> recurseHeight()); // off for debugging
                button1.Visible = false;
                splitContainer1.Visible = true;
                // t.Start();
                while((line=sr.ReadLine())!=null) // parse input csv by row
                {
                    string[] s = line.Split(','); // s is the array of columns
                    string encaps = "";
                    for (int x = 0; x < s.Length; x++)
                    {
                        if (count == 0)
                        {
                            ColumnHeader c = new ColumnHeader();
                            c.Text = x.ToString(); 
                            listView1.Columns.Add(c);
                            listView3.Columns.Add((ColumnHeader)c.Clone());
                            //checkedListBox1.Items.Add(c.Text);
                        }
                        if (x == 0) { listView1.Items.Add(s[0]).Checked = true; } else { listView1.Items[count].SubItems.Add(s[x]); }
                    }
                    count++;
                }
                foreach (ListViewItem lvi in listView1.Items) { if (lvi.Checked) { row = lvi.Index; return; } }
            }
        }

        private ArrayList buildSettings(char delim)
        {
            ArrayList settings = new ArrayList();
            foreach (ListViewItem l in listView2.Items)
            {
                if(!l.Checked){ continue; }
                try
                {
                    settings.Add(l.Tag.ToString() + delim + l.SubItems[1].Text);
                }
                catch (Exception ex) { MessageBox.Show(l.Text); }
            }
            return settings;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            progressBar1.Maximum = listView1.Items.Count;
            listView1.Enabled = false;
            for (int x = 0; x < listView2.Items.Count; x++)
            {
                if (listView2.Items[x].Checked && listView2.Items[x].SubItems.Count < 2) { button1.Enabled = true; listView1.Enabled = true; MessageBox.Show("I think you missed a field!"); return; }
            }
            settingsToPass = buildSettings('|');
            beginNewTicket();
        }
        #endregion
        #region "Methods"
        private void loadCustomFields()
        { // 12 natural fields
            for (int x = NATFIELDS + 1; x < listView2.Items.Count; x++)
            {
                listView2.Items.RemoveAt(x);
            }
            //MessageBox.Show("?");
            foreach (String s in Properties.Settings.Default.customFields)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = s.Split('|')[0]; lvi.Tag = lvi.Text; lvi.SubItems.Add(s.Split('|')[1]);
                    if (!listView2.Items.Contains(lvi)) { listView2.Items.Add(lvi); }
                }
            }
        }

        private void loadValues()
        {
            int c = 0;
            foreach(String s in Properties.Settings.Default.fvs.Split(Environment.NewLine.ToCharArray()))
            {
                try
                {
                    listView2.Items[c].SubItems.Add(s);
                }
                catch (Exception ex) { }
                c++;
            }
        }

        private string insertColumns(string i, int row)
        {
            string newStr = i;
            foreach (ColumnHeader ch in listView1.Columns)
            {
                if (i.Contains("%" + ch.Index + "%"))
                {
                    try
                    {
                        newStr = newStr.Replace("%" + ch.Index + "%", listView1.Items[row].SubItems[ch.Index].Text);
                    }
                    catch (Exception ex) { }
                }
            }
            return newStr;
        }
        private void beginNewTicket()
        {
            if (row < listView1.Items.Count - 1)
            {
                row += 1; try { while (!listView1.Items[row].Checked) { row += 1; } }
                catch (Exception ex) { }
            }
            else { return; }
            listView1.Items[row].Selected = true;
            listView1.Items[row].Checked = false;
            listView1.Items[row].EnsureVisible();
            Browse b = new Browse(listView1.Items[row], settingsToPass);
            b.url = KURL;
            b.FormClosing += new FormClosingEventHandler(b_FormClosing);
            b.Show();
        }

        void b_FormClosing(object sender, FormClosingEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            ticketsCreated+=1;
            progressBar1.Value = ticketsCreated;
            label1.Text = "Tickets created: " + ticketsCreated;
            textBox1.Text += ((Browse)sender).docTitle + Environment.NewLine;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();
            //listView3.Items.Add((ListViewItem)((ListViewItem)sender).Clone());
            beginNewTicket();
        }

        private void initInput()
        {
            fv = new FieldValue();
            fv.FormClosing += new FormClosingEventHandler(fv_FormClosing);
        }
        #endregion
        #region "Form events"
        private void BatchTickets_Load(object sender, EventArgs e)
        {
            loadCustomFields();
            Uri u;
            Uri.TryCreate(KURL, UriKind.RelativeOrAbsolute, out u);
            //webBrowser1.Url = u;
        }

        void fv_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //MessageBox.Show(fv.v);
                if (listView2.Items[fv.i].SubItems.Count < 2)
                {
                    listView2.Items[fv.i].SubItems.Add(fv.v);  
                }
                else
                {
                    listView2.Items[fv.i].SubItems[1].Text = fv.v;
                }
                if (fv.maskb)
                {
                    string s = insertColumns(listView2.Items[fv.i].SubItems[1].Text, row);
                    StringBuilder masked = new StringBuilder();
                    for(int x=0; x<s.Length; x++)
                    {
                        if (Char.Equals(fv.mask[x], '#'))
                        {
                            masked.Append(s[x]);
                        }else{ masked.Append("*"); }
                    }
                    listView2.Items[fv.i].SubItems[1].Text = masked.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Oops!\n" + ex.Message);
            }
        }
        #endregion

        #region "Listview events"
        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                initInput();
                fv.i = listView2.SelectedIndices[0];
                fv.Show();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }
        private void listView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int i = ((ListView)sender).PointToClient(Cursor.Position).X;
            if (i > 22) {
                e.NewValue = e.CurrentValue;
            }
        }
        #endregion
        #region "Toolstrip events"
        private void justOnceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                try
                {
                    foreach (string s in File.ReadAllLines(ofd.FileName))
                    {
                        if (!String.IsNullOrEmpty(s)) { listView2.Items.Add(s.Split('|')[0]).SubItems.Add(s.Split('|')[1]); }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Could not load/parse input file!"); }
            }
        }

        private void foreverToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK && File.Exists(ofd.FileName))
            {
                try
                {
                    foreach (string s in File.ReadAllLines(ofd.FileName))
                    {
                        Properties.Settings.Default.customFields.Add(s);
                        loadCustomFields();
                        Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Could not load/parse input file!"); }
            }
        }

        private void saveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (ListViewItem lvi in listView2.Items)
                {
                    if (lvi.Index > NATFIELDS)
                    {
                        sb.Append(lvi.Text + "|" + lvi.SubItems[1].Text + Environment.NewLine);
                    }
                }
                sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == DialogResult.OK) { File.WriteAllText(sfd.FileName, sb.ToString()); }

            }
            catch (Exception ex) { MessageBox.Show("Error during save attempt, make sure all fields have a value."); }
        }

        private void saveTicketInformationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, ticketUrls);
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            if (ab.ShowDialog() == DialogResult.Retry) { MessageBox.Show("Easter egg");}
        }

        private void addCustomFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fieldName = Interaction.InputBox("Enter the exact HTML ID of the new field: ", "Field name");
            string fieldValue = Interaction.InputBox("Give this field some value, using %0%, %1% etc to represent column information: ", "Value");
            if (!String.IsNullOrEmpty(fieldName) && !String.IsNullOrEmpty(fieldValue))
            {
                Properties.Settings.Default.customFields.Add(fieldName + "|" + fieldValue);
                Properties.Settings.Default.Save();
            }
            loadCustomFields();
        }

        private void deleteCustomFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            try
            {
                foreach (int i in listView2.SelectedIndices)
                {
                    if (i <= NATFIELDS) { errorProvider1.SetError(button4, "Cannot remove selected field, too low"); return; }
                    if (i > listView2.Items.Count + 1) { errorProvider1.SetError(button4, "Cannot remove selected field, too high"); return; }
                    if (MessageBox.Show("Remove " + Properties.Settings.Default.customFields[i - NATFIELDS] + " ?") == DialogResult.OK)
                    {
                        Properties.Settings.Default.customFields.Remove(Properties.Settings.Default.customFields[i - NATFIELDS]);
                        loadCustomFields();
                        Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex) { errorProvider1.SetError(button4, "Oops\n" + ex.Message); }
        }
        #endregion

        private void BatchTickets_FormClosing(object sender, FormClosingEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem lvi in listView2.Items)
            {
                if (lvi.SubItems.Count > 1) { sb.Append(lvi.SubItems[1].Text + Environment.NewLine); }
            }
            Properties.Settings.Default.fvs = sb.ToString();
            Application.Exit();
        }

        private void listView2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.SubItems.Count > 1) { return; } // already set value
            switch (e.Item.Index)
            {
                case 0: //dpt
                    e.Item.SubItems.Add("OC Manchester");
                    break;
                case 1: // subject
                    e.Item.SubItems.Add("Stop Outs");
                    break;
                case 2: // type (45 OC gateway)
                    e.Item.SubItems.Add("49");
                    break;
                case 3: // status (77 OC New gateway)
                    e.Item.SubItems.Add("97");
                    break;
                case 7: // reply (...)
                    var s = @"Good morning/afternoon.	
My name is XXXXX and I’m calling on behalf of Three Rivers Community College.	
Our records indicate that you have been awarded Financial Aid for the Fall 2016 Semester. However, you are NOT REGISTERED as of 8.11.16!	
Please register as soon as possible before classes you need are filled. Also, Do Not Forget that the Registration Deadline is August 28, 2016 (online) and August 26, 2016 (in person)  this year!! 	
Classes start August 29, 2016! Please contact the Financial Aid Office at (860) 215-9040, or financialaidhelp@threerivers.edu, if you have ANY questions regarding your Financial Aid award. 	
Provide Details of conversation:  	
 ";
                    e.Item.SubItems.Add(s);
                    break;
                case 8: // user (17797 sfaanon)
                    e.Item.SubItems.Add("17797");
                    break;
                case 9: // owner (0 unassigned)
                    e.Item.SubItems.Add("0");
                    break;
                case 11: // oc edit subject (301 OC new)
                    e.Item.SubItems.Add("301");
                    break;
                default: break;

            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ArrayList ids = new ArrayList();
            foreach (ListViewItem lvi in listView1.Items)
            {
                ids.Add(lvi.Text);
            }
            Postprocessing p = new Postprocessing();
            p.ids = ids;
            p.searchString = "Applied No Reg";
            p.Show();
        }
    }
}
