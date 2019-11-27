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
    public partial class Postprocessing : Form
    {
        public ArrayList ids;
        private ArrayList idsChecked;
        private int count = 0;
        public string searchString;
        private string searchURL = "http://esupport.ctdlc.org/staff/index.php?/Tickets/Search/Advanced";
        public Postprocessing()
        {
            InitializeComponent();
        }

        private void Postprocessing_Load(object sender, EventArgs e)
        {
            idsChecked = new ArrayList();
            webBrowser1.Navigate(searchURL);
            foreach (String s in ids)
            {
                if (!String.IsNullOrEmpty(s)) { listBox1.Items.Add(s); }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = listBox1.Items.Count - 1;
            newSearch();
        }

        private void newSearch()
        {
            timer1.Stop();
            try
            {
                webBrowser1.Document.GetElementsByTagName("select")[0].SetAttribute("selectedIndex", "86");
                foreach (HtmlElement el in webBrowser1.Document.GetElementsByTagName("input"))
                {
                    if (el.Name == "rulecriteria[1][2]") { el.SetAttribute("value", listBox1.Items[count].ToString()); }
                }
                webBrowser1.Document.GetElementById("View_Searchform_submit_0").InvokeMember("click");
                progressBar1.Value = count;
            }
            catch (Exception ex)
            {
                timer1.Start();
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!idsChecked.Contains(ids[count]) && webBrowser1.Document.Body.InnerHtml.Contains("There is nothing to display here")) // no results, store row
            {
                textBox1.Text += listBox1.Items[count].ToString() + Environment.NewLine;
                idsChecked.Add(ids[count]);
                count++;
                webBrowser1.Navigate(searchURL);
                timer1.Enabled = true; timer1.Start(); return;
            }
            if (!idsChecked.Contains(ids[count]) && webBrowser1.DocumentTitle.StartsWith("[")) // single ticket result
            {
                textBox2.Text += listBox1.Items[count].ToString() + Environment.NewLine;
                idsChecked.Add(ids[count]);
                count++;
                webBrowser1.Navigate(searchURL);
                timer1.Enabled = true; timer1.Start(); return;
            }
            if (!idsChecked.Contains(ids[count]) && webBrowser1.Document.Body.InnerHtml.Contains(searchString))
            {
                textBox3.Text += listBox1.Items[count].ToString() + Environment.NewLine;
                idsChecked.Add(ids[count]);
                count++;
                webBrowser1.Navigate(searchURL);
                timer1.Enabled = true; timer1.Start(); return;
            }
            try //auto login
            {
                webBrowser1.Document.GetElementById("username").SetAttribute("value", "*EMAIL_HERE*");
                webBrowser1.Document.GetElementById("password").SetAttribute("value", "*PASSWORD_HERE*");
                webBrowser1.Document.GetElementsByTagName("input")[2].InvokeMember("click"); // press login
            }
            catch (Exception ex)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            newSearch();
        }
    }
}
