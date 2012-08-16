using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KeypicLib;

namespace KeypicExample
{
    public partial class _Default : System.Web.UI.Page
    {
        protected IKeypicClient keypicClient = null;
        string clientEmail = "";
        string clientUsername = "";
        string clientMessage = "";
        string clientFingerPrint = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            
            keypicClient = KeypicClient.GetInstance(); //get instance of Keypic Client
            keypicClient.FormID = "8ee81a54e8df53eadae33ac1cf115c8a"; //set form id

            if (!IsPostBack) {
                hdnToken.Value = keypicClient.GetToken(clientEmail, clientUsername, clientMessage, clientFingerPrint, 1);  //get new token
            }

            if (keypicClient.IsErrorExist()) { // check for error
                ltlErrorMessage.Text = keypicClient.GetLastError();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try {
                clientEmail = txtEmail.Text;
                clientUsername = txtUsername.Text;
                clientMessage = txtMessage.Text;

                keypicClient.Token = hdnToken.Value;
                int result = keypicClient.IsSpam(clientEmail, clientUsername, clientMessage, clientFingerPrint); //check for spam

                if (result>=0) {
                    ltlErrorMessage.Text = string.Format("This message has {0}% of spam probability.", result);
                }else {
                    throw new Exception(keypicClient.GetLastError());
                }

            } catch (Exception ex) {
                ltlErrorMessage.Text= ex.Message;
            }

            hdnToken.Value = keypicClient.GetToken(clientEmail, clientUsername, clientMessage, clientFingerPrint, 1);
        }
    }
}