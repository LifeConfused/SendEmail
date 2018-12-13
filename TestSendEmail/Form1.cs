using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSendEmail
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //test program references SendEmail in the debug folder so that you can place break points and step into and follow the code within the dll
            textBoxServer.Text = "";
            textBoxUsername.Text = "";
            textBoxPassword.Text = "";
            textBoxPort.Text = "25";
            checkBoxSSL.Checked = false;
            textBoxEmailFrom.Text = "test@from.uk.com";
            textBoxEmailTo.Text = "test@to.uk.com";
            textBoxEmailCC.Text = "";
            textBoxEmailBCC.Text = "";
            textBoxEmailSubject.Text = "C# - test";
            textBoxBodyFileName.Text = "C:\\TEMP\\body.txt";//body text location
            textBoxAttachments.Text = "";
            textBoxImageFolder.Text = "C:\\TEMP\\Images\\";//make sure you put a backslash at the end to denote its a folder
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            int port = 0;
            int.TryParse(textBoxPort.Text, out port);

            /*
                magic call is "Call UDP '@SendEmail.SendMail' Convention Standard"
                First parameter is 'AAA44AAAAAAAA4'
                First 13 chars describe the parameters going in. A is for strings 4 is for ints
                Last char 4 is to describe the return type which is an int
            */
            int result = SendEmail.SendEmail.SendMail(textBoxServer.Text, 
                                                      textBoxUsername.Text, 
                                                      textBoxPassword.Text, 
                                                      port, 
                                                      checkBoxSSL.Checked ? 1 : 0,//don't know how to pass bool from magic so treating as a int
                                                      textBoxEmailFrom.Text, 
                                                      textBoxEmailTo.Text,
                                                      textBoxEmailCC.Text,
                                                      textBoxEmailBCC.Text,
                                                      textBoxEmailSubject.Text,
                                                      textBoxBodyFileName.Text,
                                                      textBoxAttachments.Text,
                                                      textBoxImageFolder.Text);

            textBox1.Text = "Result: " + result;
        }
    }
}
