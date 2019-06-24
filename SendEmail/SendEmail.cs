using RGiesecke.DllExport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SendEmail
{
    public class SendEmail
    {
        public static bool EnableLogging = false;//global logging flag
        public SendEmail()
        {
            // 
            // TODO: Add constructor logic here 
            // 
        }

        [DllExport("SendMail", CallingConvention = CallingConvention.StdCall)]
        public static int SendMail(string server, string username, string password, int port, int sslEnabled, string from, string to, string cc, string bcc, string subject, string bodyFileName, string attachments, string imageDirectory)
        {
            //log inbound parameters
            EnableLogging = true;//change this to false and rebuild dll if you want to disable logging. This could be added as a parameter
            WriteLog("-----");
            WriteLog("Server: " + server);
            WriteLog("Username: " + username);
            WriteLog("Password: " + (String.IsNullOrEmpty(password) ? "" : "*****"));//only log presence of password
            WriteLog("Port: " + port);
            WriteLog("SSL Enabled: " + (sslEnabled == 1 ? "true" : "false"));
            WriteLog("From: " + from);
            WriteLog("To: " + to);
            WriteLog("CC: " + cc);
            WriteLog("BCC: " + bcc);
            WriteLog("Subject: " + subject);
            WriteLog("Body File Name: " + bodyFileName);
            WriteLog("Attachments: " + attachments);
            WriteLog("Image Directory: " + imageDirectory);

            try
            {
                int status = 1;//1 means it didnt get as far as even attempting to send

                //Check for multiple email to's and create string list
                List<string> EmailTo = new List<string>();
                string EmailToAddress = to;
                if (EmailToAddress != null)
                {
                    string[] separators = { ",", ";", " " };
                    string[] emails = EmailToAddress.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string email in emails)
                        EmailTo.Add(email);
                }

                //Check for multiple email cc's and create string list
                List<string> EmailCC = new List<string>();
                string EmailCCAddress = cc;
                if (EmailCCAddress != null)
                {
                    string[] separators = { ",", ";", " " };
                    string[] emails = EmailCCAddress.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string email in emails)
                        EmailCC.Add(email);
                }

                //Check for multiple email bcc's and create string list
                List<string> EmailBCC = new List<string>();
                string EmailBCCAddress = bcc;
                if (EmailBCCAddress != null)
                {
                    string[] separators = { ",", ";", " " };
                    string[] emails = EmailBCCAddress.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string email in emails)
                        EmailBCC.Add(email);
                }

                //Check for multiple email attachment's and create string list (full filepath and name)
                List<string> EmailAttachments = new List<string>();
                string FileAttachments = attachments;
                if (FileAttachments != null)
                {
                    string[] separators = { ",", ";" };
                    string[] files = FileAttachments.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string file in files)
                        EmailAttachments.Add(file);
                }

                string EmailFromAddress = from;
                string EmailServer = server;
                string EmailUsername = username;
                string EmailPassword = password;

                //Init Message object and smtp client
                MailMessage objeto_mail = new MailMessage();
                SmtpClient client = new SmtpClient();

                bool sslMode = sslEnabled == 1;//dont know code type for magic to call with boolean
                port = port != 0 ? port : sslMode ? 465 : 25;
                WriteLog("Initialise SMTP Client: Port " + port);
                client.Port = port;
                WriteLog("Initialise SMTP Client: Enable SSL " + (sslMode ? "true" : "false"));
                client.EnableSsl = sslMode;

                client.Host = EmailServer != null ? EmailServer : "test";
                client.Timeout = 10000;//in milliseconds so currently 10 seconds
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;//we use custom credentials but just flip this to true if necessary
                client.Credentials = new System.Net.NetworkCredential(EmailUsername != null ? EmailUsername : "", EmailPassword != null ? EmailPassword : "");

                objeto_mail.From = new MailAddress(EmailFromAddress != null ? EmailFromAddress : "test@test.uk.com");//fallback email address from

                //Add all email to's to mail object
                foreach (string email in EmailTo)
                {
                    objeto_mail.To.Add(new MailAddress(email));
                }
                //Add all email cc's to mail object
                foreach (string email in EmailCC)
                {
                    objeto_mail.CC.Add(new MailAddress(email));
                }
                //Add all email bcc's to mail object
                foreach (string email in EmailBCC)
                {
                    objeto_mail.Bcc.Add(new MailAddress(email));
                }
                //Add subject to mail object
                objeto_mail.Subject = subject != null ? subject : "";

                //flag as html
                objeto_mail.IsBodyHtml = true;

                string body = "";
                if (File.Exists(bodyFileName))//if html body filename exists then process it
                {
                    body = File.ReadAllText(bodyFileName, Encoding.Default);//read file contents into string, encoding default will hopefully catch special characters

                    // Add the alternate body to the message.
                    AlternateView alternate = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                    //find and attach images
                    int start = body.IndexOf("src=\"cid:");
                    int end = 0;
                    if (start != 0)
                        WriteLog("Images detected so attach them as resources");
                    while (start > 0)
                    {
                        end = body.IndexOf("\"", start + 5);//getquote mark after cid:
                        string filename = body.Substring(start + 9, end - (start + 9));
                        filename = filename.Replace("&amp;", "&");//handle ckeditor messing with image filenames when editing templates. we could maybe do urldecode here instead if more symbols become an issue

                        if (File.Exists(imageDirectory + filename))//only process image if it exists, prevents error 6
                        {
                            //must define mime type or windows mail shows all images as attachments as well as inline
                            //we pass in a image directory as all our email images are in one place, you could put the full filepath in the src tag and replace it with a guid or something if you wanted to
                            Attachment att = new Attachment(imageDirectory + filename, "image/jpg");//jpg might be enough to enter image mode, might need to be more specific with mime type
                            att.ContentDisposition.Inline = true;//flag it as inline so it doesnt appear as attachment
                            att.ContentId = filename;//this must match the content of the source tag without "cid:" so if "cid:filename.jpg" then this needs to be "filename.jpg"
                            objeto_mail.Attachments.Add(att);//add attachment to mail object
                        }
                        else
                        {
                            WriteLog("Image does not exist so leave image blank: " + imageDirectory + filename);
                        }

                        start = body.IndexOf("src=\"cid:", end);//look for next cid:
                    }

                    objeto_mail.AlternateViews.Add(alternate);//add view to mail object. You need to add another if you want a plain text version fallback as well
                }

                objeto_mail.Body = body != null ? body : "";//maybe not necessary as we are using alternate views

                //Add any normal attachements to the mail object
                foreach (string file in EmailAttachments)
                {
                    WriteLog("Attach file " + file);
                    Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);

                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(file);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

                    // Add the file attachment to this e-mail message.
                    objeto_mail.Attachments.Add(data);
                }


                //dont send if required fields are missing
                if (!String.IsNullOrEmpty(EmailServer) && !String.IsNullOrEmpty(EmailToAddress) && !String.IsNullOrEmpty(EmailFromAddress))
                {
                    try
                    {
                        WriteLog("Sending Email");
                        client.Send(objeto_mail);//if this fails it will go through the below catch
                        status = 0;
                        WriteLog("Email sent");
                    }
                    catch (Exception e)
                    {
                        status = 2;//failed to send
                        WriteLog("Error sending email(2): " + (e != null ? e.Message : ""));
                    }
                }
                else
                {
                    string error = "Could not send email notification because:";
                    if (String.IsNullOrEmpty(EmailServer))
                    {
                        status = 3;
                        error += " No Email Server Name Defined.";
                    }
                    if (String.IsNullOrEmpty(EmailToAddress))
                    {
                        status = 4;
                        error += " No Email To Address Defined.";
                    }
                    if (String.IsNullOrEmpty(EmailFromAddress))
                    {
                        status = 5;
                        error += " No Email From Address Defined.";
                    }
                    WriteLog(error);
                }

                objeto_mail.Dispose();

                return status;
            }
            catch (Exception e)
            {
                WriteLog("Error sending email(6): " + (e != null ? e.Message : ""));
                return 6;
            }
        }

        private static void WriteLog(string entry)
        {
            if (EnableLogging)
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\SendEmail.log"))//if log file doesnt exist then create it
                {
                    using (StreamWriter sw = File.CreateText(Directory.GetCurrentDirectory() + "\\SendEmail.log"))
                    {
                        //if "-----" then just print that as its a line seperator, else prepend with date/time
                        sw.WriteLine((entry != "-----" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " : "") + entry);
                    }
                }
                else//log file exists so append to it
                {
                    using (StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + "\\SendEmail.log"))
                    {
                        sw.WriteLine((entry != "-----" ? DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " : "") + entry);
                    }
                }
            }
        }
    }
}
