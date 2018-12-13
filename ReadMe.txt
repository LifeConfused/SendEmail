You can either open the project and tweak/compile it yourself.
Or there are some precompiled dlls in SendEmail\SendEmail\bin\Release
Choose the logging enabled/disabled folder for your preference. If enabled it will create a SendEmail.txt next to where the dll is run from

magic call is "Call UDP '@SendEmail.SendMail' Convention Standard"
First parameter is 'AAA44AAAAAAAA4'
First 13 chars describe the parameters going in. A is for strings 4 is for ints
Last char 4 is to describe the return type which is an int

So arguments in the call should be
'AAA44AAAAAAAA4'
string server
string username
string password
int port
int sslEnabled
string from
string to
string cc
string bcc
string subject
string bodyFileName
string attachments
string imageDirectory
int result

body text has to be saved to file

It will scan the body text for occurences of src"cid:
And using a standard folder location holding all possible images will attach it to the email and display it inline.
For example: <img border="0" src="cid:Logo.jpg" width="105" height="119">
Will be found and it will attach "imageDirectory + Logo.jpg", mark it as inline and mime type jpg, and content id of "Logo.jpg". 
This should make it not appear as an attachment but still appear in the body text.