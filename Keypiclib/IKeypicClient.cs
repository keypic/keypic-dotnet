using System;

namespace KeypicLib
{
    public enum ResponseType
    {
        Default = 0,
        CSV = 1,
        JSON = 2,
        PHPSerial = 3,
        XML = 4
    }

    /// <summary>
    /// Interface to create client for Keypic services
    /// </summary>
    public interface IKeypicClient
    {

        #region Properties
        /// <summary>
        /// Version of the Keypic service.
        /// </summary>
        String Version { get; }

        /// <summary>
        /// User Agent's description.
        /// </summary>
        String UserAgent { get; }

        /// <summary>
        /// Execute the request with debug mode or not.
        /// </summary>
        Boolean Debug { get; set; }

        /// <summary>
        /// Form ID you generated with the web-site of Keypic.
        /// </summary>
        String FormID { get; set; }

        /// <summary>
        /// Token you got from server, using GetTokenMethod.
        /// </summary>
        String Token { get; set; }

        /// <summary>
        /// Url of Keypic server.
        /// </summary>
        String Host { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Check with server that form id is valid or not.
        /// </summary>
        /// <returns>It will return true, if form ID is valid. Otherwise return false.</returns>
        Boolean CheckFormID();

        /// <summary>
        /// Get new token from server.
        /// </summary>
        /// <returns>Return Token.</returns>
        String GetToken(string clientEmailAddress, string clientUsername, string clientMessage, string clientFingerprint, int quantity);

        /// <summary>
        /// Get HTML code of image box to place the Keypic icon on the form.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>HTML code of image box</returns>
        String GetImage(int width, int height);

        /// <summary>
        /// Get HTML code of image box to place the Keypic icon on the form within an iFrame.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>HTML code of image box within iFrame</returns>
        String GetFrame(int width, int height);

        /// <summary>
        /// Check
        /// </summary>
        /// <returns></returns>
        int IsSpam(string clientEmailAddress, string clientUsername, string clientMessage, string clientFingerprint); 

        /// <summary>
        /// Report
        /// </summary>
        /// <returns></returns>
        Boolean ReportSpam();

        string GetLastDtaRecived();

        string GetLastError();

        bool IsErrorExist();
        #endregion

    }
}
