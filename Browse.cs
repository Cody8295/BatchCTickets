using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace BatchTickets
{
    public partial class Browse : Form
    {
        private ListViewItem lvi;
        private ArrayList settings;
        public char settingDelim = '|';
        public WebBrowser getBrowser() { return webBrowser1; }
        public string url = "";
        public void done() { Close(); }
        private int tries = 0;
        public string docTitle;
        public bool created = false;
        private bool loginAttempt = false;

        public Browse(ListViewItem l, ArrayList map)
        {
            settings = new ArrayList();
            settings.AddRange(map.ToArray());
            lvi = l;
            InitializeComponent();
        }
        
        private void Browse_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true; timer1.Start();
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(url);
            newTicket.Enabled = true; newTicket.Start();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
        }

        private void newTicket_Tick(object sender, EventArgs e)
        {
            try //auto login
            {
                webBrowser1.Document.GetElementById("username").SetAttribute("value", "*EMAIL_HERE*");
                webBrowser1.Document.GetElementById("password").SetAttribute("value", "*PASSWORD_HERE*");
                webBrowser1.Document.GetElementsByTagName("input")[2].InvokeMember("click"); // press login
            }
            catch (Exception ex)
            {

            }
            try // TICKET > NEW TICKET
            {
                webBrowser1.Document.GetElementById("tb_menusection2").InvokeMember("click");
                //MessageBox.Show("Preaa");
                webBrowser1.Document.GetElementById("linkmenu2_2").InvokeMember("click");
                //MessageBox.Show("prea");
                tries = 0;
                newTicket.Stop();
                dprtmntSel.Enabled = true; dprtmntSel.Start();
            }
            catch (Exception ex)
            {
                tries++;
                if (tries > 10) { tries = 0; webBrowser1.Refresh(); }
                return;
            }
        }

        private string insertColumns(string i)
        {
            string newStr = i;
            for (int x = 0; x < lvi.SubItems.Count; x++)
            {
                if (i.Contains("%" + x.ToString() + "%"))
                {
                    newStr = newStr.Replace("%" + x.ToString() + "%", lvi.SubItems[x].Text);
                }
            }
            return newStr;
        }
        private string htmlID(string settingLine)
        {
            return settingLine.Split(settingDelim)[0];
        }
        private string htmlValue(string settingLine)
        {
            return insertColumns(settingLine.Split(settingDelim)[1]);
        }

        private string getDepartment()
        {
            foreach (string s in settings)
            {
                if (htmlID(s) == "selectdepartmentid") { return htmlValue(s); }
            }
            return null;
        }

        private bool autoresponderOff()
        {
            foreach (string s in settings)
            {
                if (htmlID(s) == "autoresponder") { return (htmlValue(s)=="1"?true:false); }
            }
            return true; // off by default
        }

        private void dprtmntSel_Tick(object sender, EventArgs e)
        {
            newTicket.Stop();
            //MessageBox.Show("A");
            try
            {   // why not use element id to set value?
                //MessageBox.Show(getDepartment());
                webBrowser1.Document.GetElementById("selectdepartmentid").SetAttribute("value", "45");
                foreach (HtmlElement h in webBrowser1.Document.GetElementById("selectdepartmentid").Children)
                {
                    if (h.InnerHtml.Replace("&nbsp;", " ").Trim() == getDepartment())
                    {
                        h.SetAttribute("selected", "selected");
                        webBrowser1.Document.GetElementById("selectdepartmentid").Focus();
                        webBrowser1.Document.GetElementById("selectdepartmentid").SetAttribute("value", h.GetAttribute("value"));
                        webBrowser1.Document.GetElementById("selectdepartmentid").InvokeMember("onkeyup");
                        webBrowser1.Document.GetElementById("selectdepartmentid").InvokeMember("onchange");
                        break;
                    }
                }
                //MessageBox.Show("D");
                webBrowser1.Document.GetElementById("newticketdialogform_submit_0").InvokeMember("click");
                //MessageBox.Show("E");
                tries = 0;
                ticketCreation.Enabled = true; ticketCreation.Start();
            }
            catch (Exception ex) { tries++; if (tries > 6) { dprtmntSel.Stop(); webBrowser1.Refresh(); newTicket.Start(); } }
        }

        private void ticketCreation_Tick(object sender, EventArgs e)
        {
            tries = 0;
            dprtmntSel.Stop();
            try
            {
                if (autoresponderOff()) { webBrowser1.Document.GetElementById("newticketsendar").InvokeMember("click"); }
                foreach (string s in settings)
                {
                    try { webBrowser1.Document.GetElementById(htmlID(s)).SetAttribute("value", htmlValue(s)); } catch (Exception ex) { }
                }
                webBrowser1.Document.GetElementById("newticketform_submitform_1").InvokeMember("click");
                
                ticketCreation.Stop();
                ticketView.Enabled = true; ticketView.Start();
            }
            catch (Exception ex) { tries++; if (tries > 17) { ticketCreation.Stop(); webBrowser1.Refresh(); newTicket.Start(); } return; }
            
        }

        private void ticketView_Tick(object sender, EventArgs e)
        {
            ticketCreation.Stop();
            docTitle = webBrowser1.DocumentTitle;
            if(docTitle.StartsWith("[")){
                created = true;
                ticketView.Stop();
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        { // 25 seconds after opening the form, detect if the ticket was created
            if (!created && Application.OpenForms.Count <= 3)
            {
                ticketView.Stop(); ticketCreation.Stop(); dprtmntSel.Stop(); newTicket.Stop();
                Browse b = new Browse(lvi, settings);
                b.url = url;
                b.Show();
                Close();
            }
        }
    }
}
