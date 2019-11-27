using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace BatchTickets
{
    public partial class FieldValue : Form
    {
        public FieldValue()
        {
            InitializeComponent();
        }
        #region "Global variables"
        public int i = 0;
        public string v = "";
        public string mask = "##########";
        public bool maskb = false;

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern void SetCueBanner(IntPtr hWnd, int msg, [MarshalAs(UnmanagedType.Bool)] bool wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501;
        private const int CB_SETCUEBANNER = 0x1703;

        private string[] departments = {"Support Center", "Charter Oak State College", "New Haven",
                                       "Springfield College", "University of Hartford", "COSC FinAid",
                                       "Housatonic CC FA", "Middlesex CC FA", "OC Housatonic", "COSC Switchboard",
                                       "CSCU Manufacturing Support Center", "Manufacturing", "Housatonic",
                                       "OC Tunxis", "OC Norwalk", "OC Three Rivers", "OC Naugatuck Valley", "OC Gateway",
                                       "OC North Western", "OC COSC", "OC Manchester", "OC Capital", "OC Quinebaug Valley", "OC Asnuntuck"};
        private string[] types = { "Issue", "Task", "Information Request", "Bug", "Change Request",
                                "SFA Process", "SFA Complex", "SFA Status", "SFA Other Department", "GBTGA",
                                 "OC Housatonic", "OC Three Rivers", "OC Tunxis", "OC Norwalk", "OC Gateway",
                                 "OC Northwestern", "OC COSC", "OC Manchester", "OC Capital", "OC QVCC", "OC Asnuntuck", "OC TRCC PCT"};
        private int[] typesValue = { 1, 2, 4, 3, 12, 10, 14, 13, 16, 15, 39, 42, 43, 44, 45, 47, 40, 49, 50, 46, 52, 53};
        private string[] status = { "New", "SFA Escalation", "Pending Internal Action", "Work in Progress",
                                  "Awaiting Client Reply", "Closed", "Closed as Duplicate", "OC Closed- No Contact",
                                  "OC Closed Spoke to Student", "OC Call Attempt 2", "OC Closed Escalated", "OC New Housatonic", "OC Call Attempt 1",
                                  "OC New TRCC", "'OC New Tunxis", "OC New Norwalk", "OC New Gateway", "OC New Northwestern", "OC New COSC",
                                  "OC New Manchester", "OC New Capital", "OC New QVCC", "OC New Asnuntuck", "OC TRR PCT New"};
        private int[] statusValue = { 4, 11, 7, 9, 5, 6, 12, 51, 52, 53, 54, 55, 56, 72, 67, 87, 77, 92, 59, 97, 105, 82, 111, 128};
        private int[] outboundSubjects = { 301, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 302, 304, 324, 325, 621, 622, 623 };
        private string[] outboundStrings = { "OC- New ", "OC - Already Graduated ", "OC - Career Outcome ", "OC - Chose Another School ", "OC - Cost - Tuition not feasible ", "OC - Degree/Major Not Available ", "OC - Financial Aid Ineligible ", "OC - Military Duty ", "OC - Personal  - Academically ", "OC - Personal - Family ", "OC - Personal - Financial ", "OC - Personal - Health ", "OC - Poor Admission Experience ", "OC - Schedule Options ", "OC - Taking Term/Semester Off ", "OC - Transportation Issues ", "OC - Unprepared ", "OC- Moved to Another State ", "OC - Phone number not in Service ", "OC- Student Would Not Speak to Us ", "OC- 3rd Attempt with no Contact ", "OC- Filing FA Promise Form ", "OC - Intends to pay ", "OC- Spoke to Student " };
        private string[] institutions = { };
        private int[] ownerID = { 0, 91, 162, 6, 113, 26, 42, 139, 97, 136, 131, 65, 70, 27, 75, 123, 15, 79, 96, 94, 98, 95, 122, 99, 1, 124, 4, 82, 86, 125, 119, 80, 3, 128, 129, 84, 126, 148, 64, 116, 114, 5, 115, 66, 121, 83, 145, 92, 161, 90, 89, 135, 9 };
        private string[] ownerNames = { "-- Unassigned -- ", "Abbey Futoma ", "Advanced Manufacturing ", "Anthony DeCusati ", "Bradley Bell ", "Carolyn Caggiano ", "Cathy Bergren ", "Chelsea Armstrong ", "Christian Beauford ", "Cody DallaValle ", "Darryl Bradford ", "Desiree Estela ", "Diane Van Hook ", "George Claffey ", "Greg Goodwin ", "Housatonic CC ", "Instructional Design Team ", "Jacob Buslewicz ", "James Daly ", "James Wilkie ", "Jenna-Noor Khan ", "Joaquin Lindsay ", "John Hayes ", "Jordan Beauford ", "Kayako Administrator ", "Kayako Test ", "Kevin Corcoran ", "Kevin Leigh ", "Kimberly Oravetz ", "Les Cropley ", "Lewis Berry ", "Maryelizabeth Henderson ", "Matt Pickering ", "Mifrah Malik ", "Miranda Velez ", "Mohamed Fahim ", "Monisha Brown ", "MXCC Retention Staff ", "Nafatari Campbell ", "Naomi Velez ", "Oshane Levy ", "Owen Brandt ", "Paige Romei ", "Rebecca Lindsay-DeCusati ", "Samantha Wasilefsky ", "Sammie Meneses ", "Sean Scott ", "Shoon Nakajo-Robben ", "Three Rivers Manufacturing ", "Tommy Daly ", "Treana Bellamy ", "Tyrus Middleton ", "William Burnes" };
        #endregion

        #region "Form events"
        private void FieldValue_Load(object sender, EventArgs e)
        {
            
        }

        private void FieldValue_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            flowLayoutPanel1.Controls.Clear();
            maskb = false;  checkBox1.Visible = false; listBox1.Visible = false; comboBox1.Visible = false; textBox1.Visible = false; comboBox1.Items.Clear(); textBox1.Clear();
            textBox1.Size = new Size(360, 20);
            switch(i)
            {
                case 0: // department
                    comboBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(comboBox1);
                    comboBox1.Items.AddRange(departments);
                    break;
                case 1: // subject
                    textBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(textBox1);
                    SetCueBanner(textBox1.Handle, EM_SETCUEBANNER, false, "Subject of Ticket. Use %0%, %1%, etc to access column information");
                    break;
                case 2: // type
                    comboBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(comboBox1);
                    comboBox1.Items.AddRange(types);
                    break;
                case 3: // status
                    comboBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(comboBox1);
                    comboBox1.Items.AddRange(status);
                    break;
                case 4: // banner id
                    checkBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(checkBox1);
                    flowLayoutPanel1.Controls.Add(textBox1);
                    textBox1.Visible = true; SetCueBanner(textBox1.Handle, EM_SETCUEBANNER, false, "Banner ID. Use %1%, %2% etc to access  column information");
                    break;
                case 5: // phone number
                    flowLayoutPanel1.Controls.Add(textBox1);
                    textBox1.Visible = true; SetCueBanner(textBox1.Handle, EM_SETCUEBANNER, false, "Phone #. Use %1%, %2% etc to access  column information");
                    break;
                case 7: // reply
                    flowLayoutPanel1.Controls.Add(textBox1);
                    textBox1.Multiline = true;
                    textBox1.Size = new Size(320, 60);
                    textBox1.Visible = true; SetCueBanner(textBox1.Handle, EM_SETCUEBANNER, false, "Body of Ticket. Use %0%, %1%, etc to access column information");
                    break;
                case 8: // userid
                    flowLayoutPanel1.Controls.Add(listBox1);
                    listBox1.Visible = true;
                    break;
                case 9: // owner
                    comboBox1.Visible = true;
                    flowLayoutPanel1.Controls.Add(comboBox1);
                    comboBox1.Items.AddRange(ownerNames);
                    break;
                case 11: // outbound
                    flowLayoutPanel1.Controls.Add(comboBox1);
                    comboBox1.Visible = true;
                    comboBox1.Items.AddRange(outboundStrings);
                    break;
                case 12: // oc first last name
                    flowLayoutPanel1.Controls.Add(textBox1);
                    textBox1.Visible = true;
                    SetCueBanner(textBox1.Handle, EM_SETCUEBANNER, false, "First and last name field for outbound calls");
                    break;
                default: this.Close(); break;

            } flowLayoutPanel1.Controls.Add(button1);
        }
        #endregion
        #region "Control events"
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                maskb = true;
                mask = Interaction.InputBox("How should the ID should be masked using only # to represent numbers would should not be masked and lowercase x for numbers that should be masked.", "Mask");
            } Close();   
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            v = textBox1.Text;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.SelectedIndex == 3)
                {
                    v = Interaction.InputBox("Go to Users > Manage Users and click on their Email. The users ID is located at the end of the URL. Please enter it:", "Enter user ID", "0");
                    Close();
                }
            }
            catch (Exception ex) { }
            switch (listBox1.SelectedIndex)
            {
                case 0: v = "5792"; break;
                case 1: v = "17797"; break;
                case 2: v = "39401"; break;
                default: break;
            }
        }
        #endregion

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            switch (i)
            {
                case 0:
                    v = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                    break;
                case 2:
                    v = typesValue[comboBox1.SelectedIndex].ToString();
                    break;
                case 3:
                    v = statusValue[comboBox1.SelectedIndex].ToString();
                    break;
                case 9:
                    v = ownerID[comboBox1.SelectedIndex].ToString();
                    break;
                case 11:
                    v = outboundSubjects[comboBox1.SelectedIndex].ToString();
                    break;
                default: break;
            }
        }

        private void FieldValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString() == Keys.Enter.ToString()) { button1.PerformClick(); }
        }
    }
}
