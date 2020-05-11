using System.IO;
using System.Net;
using System.Threading;
using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace SaveSystem
{
    public class WebSave
    {
        //A file is uploaded in 2048 byte parts
        private const ushort BUFFER_SIZE = 2048;

        private NetworkCredential networkCredential;
        public NetworkCredential NetworkCredential { get { return networkCredential; } }

        private int timeoutMilliseconds = 8000;
        public int TimeoutMilliseconds { get { return timeoutMilliseconds; } }

        public delegate void ProgressAction(float progress);

        private string GetUrl(string pathOnServer)
        {
            return "ftp://" + networkCredential.Domain + "/" + pathOnServer;
        }

        #region CONSTRUCTOR
        public WebSave(NetworkCredential networkCredential)
        {
            this.networkCredential = networkCredential;
        }
        public WebSave(NetworkCredential networkCredential, int timeoutMilliseconds)
        {
            this.networkCredential = networkCredential;
            this.timeoutMilliseconds = timeoutMilliseconds;
        }
        #endregion

        #region NETWORK CONNECTION
        #region HAS CONNECTION
        /// <summary>
        /// Returns true if the device is connected to the server
        /// </summary>
        public bool IsConnected()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + networkCredential.Domain);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.Timeout = timeoutMilliseconds;
                request.GetResponse();
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        #region PING
        /// <summary>
        /// Returns the time in milliseconds it takes the server to respond
        /// </summary>
        public int GetPingMilliseconds()
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + networkCredential.Domain);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.GetResponse();

                stopwatch.Stop();
                return (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception exc)
            {
                Debug.LogError("Error getting ping: Read ERRORS-file.\n" + exc);
                return -1;
            }
        }
        #endregion
        #endregion

        #region GET LAST MODIFIED DATE
        /// <summary>
        /// Gets the time of the last change of a file (normally greenwich time)
        /// </summary>
        public DateTime GetLastModifiedDate(string pathOnServer)
        {
            try
            {
                Uri serverUri = new Uri(GetUrl(pathOnServer));

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return response.LastModified;
            }
            catch (Exception exc)
            {
                Debug.LogError("Error getting last modified date: Read ERRORS-file.\n" + exc);
                return new DateTime();
            }
        }
        #endregion

        #region UPLOAD FILE
        /// <summary>
        /// Uploads a file using FTP.
        /// </summary>
        /// <param name="localFilePath">The local path to the file which will be uploaded.</param>
        /// <param name="pathOnServer">The path on the server to upload the file to.</param>
        /// <param name="progressAction">Is called when the upload progress changes.</param>
        public bool UploadFile(string localFilePath, string pathOnServer, ProgressAction progressAction = null)
        {
            try
            {
                var file = new FileInfo(localFilePath);
                var address = new Uri(GetUrl(pathOnServer));
                var request = WebRequest.Create(address) as FtpWebRequest;

                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                // Set control connection to closed after command execution
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                // Specify data transfer type
                request.UseBinary = true;

                // Notify server about size of uploaded file
                request.ContentLength = file.Length;
                // Set buffer size to 2KB.
                var bufferLength = BUFFER_SIZE;
                var buffer = new byte[bufferLength];
                var contentLength = 0;

                // Open file stream to read file
                var fs = file.OpenRead();

                // Stream to which file to be uploaded is written.
                var stream = request.GetRequestStream();
                // Read from file stream 2KB at a time.
                contentLength = fs.Read(buffer, 0, bufferLength);
                // Loop until stream content ends.
                while (contentLength != 0)
                {
                    if (progressAction != null)
                        progressAction.Invoke(fs.Position / (float)fs.Length);
                    // Write content from file stream to FTP upload stream.
                    stream.Write(buffer, 0, contentLength);
                    contentLength = fs.Read(buffer, 0, bufferLength);
                }

                // Close file and request streams
                stream.Close();
                fs.Close();
            }
            catch (Exception exc)
            {
                Debug.LogError("Error uploading file: Read ERRORS-file.\n" + exc);
                return false;
            }
            return true;
        }
        #endregion

        #region DOWNLOAD FILE
        /// <summary>
        /// Downloads a file using FTP.
        /// </summary>
        /// <param name="localFilePath">The local path to which the file which will be downloaded.</param>
        /// <param name="pathOnServer">The path on the server to download the file from.</param>
        /// <param name="progressAction">Is called when the download progress changes.</param>
        public bool DowloadFile(string localFilePath, string pathOnServer, ProgressAction progressAction = null)
        {
            try
            {
                string url = GetUrl(pathOnServer);
                NetworkCredential credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);

                // Query size of the file to be downloaded
                WebRequest sizeRequest = WebRequest.Create(url);
                sizeRequest.Credentials = credentials;
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                int fileSize = (int)sizeRequest.GetResponse().ContentLength;

                // Download the file
                WebRequest request = WebRequest.Create(url);
                request.Credentials = credentials;
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                byte[] buffer = new byte[BUFFER_SIZE];
                int readLength;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                {
                    using (Stream fileStream = File.Create(localFilePath))
                    {
                        while ((readLength = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, readLength);
                            if (progressAction != null)
                                progressAction.Invoke((int)fileStream.Position / (float)fileSize);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error downloading file: Read ERRORS-file.\n" + exc);
                return false;
            }
            return true;
        }
        #endregion

        #region DELETE FILE
        /// <summary>
        /// Deletes a file on the server.
        /// </summary>
        public bool DeleteFile(string pathOnServer)
        {
            try
            {
                // The address where deleting the file from.
                var address = new Uri(GetUrl(pathOnServer));

                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception exc)
            {
                Debug.LogError("Error deleting file: Read ERRORS-file.\n" + exc);
                return false;
            }
            return true;
        }
        #endregion

        #region FILE EXISTS
        /// <summary>
        /// Does there exist a file at the given path on the server
        /// </summary>
        public bool FileExists(string pathOnServer)
        {
            string url = GetUrl(pathOnServer);

            WebRequest request = WebRequest.Create(url);
            request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            try
            {
                request.GetResponse();
            }
            catch (WebException e)
            {
                FtpWebResponse response = (FtpWebResponse)e.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region CREATE DIRECTORY
        /// <summary>
        /// Creates a directory on the server
        /// </summary>
        /// <param name="directoryName">The name of directory which will be created</param>
        /// <param name="pathOnServer">The path on the server where the directory will be created</param>
        public bool CreateDirectory(string directoryName, string pathOnServer)
        {
            try
            {
                //The address where to create the directory.
                var address = new Uri(GetUrl(pathOnServer) + "/" + directoryName);

                //Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Creating directory process was successful.
                response.Close();
            }
            //The creating-process failed.
            catch (Exception exc)
            {
                Debug.LogError("Error creating directory: Read ERRORS-file.\n" + exc);
                return false;
            }
            return true;
        }
        #endregion

        #region DIRECORY EXISTS
        /// <summary>
        /// Does there exist a directory at the given path on the server
        /// </summary>
        public bool DirectoryExists(string pathOnServer)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetUrl(pathOnServer) + "/");
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (Exception exc)
            {
                return false;
            }
        }
        #endregion

        #region RENAME
        /// <summary>
        /// Renames a file/directory on a server.
        /// </summary>
        /// <param name="pathOnServer">The path on the server where the directory or file is located</param>
        /// <param name="newName">The new name of the directory or file</param>
        public bool Rename(string pathOnServer, string newName)
        {
            try
            {
                //The web address
                var address = new Uri(GetUrl(pathOnServer));

                //Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
                request.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
                request.RenameTo = newName;
                request.Method = WebRequestMethods.Ftp.Rename;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            //The rename process failed.
            catch (Exception exc)
            {
                Debug.LogError("Error renaming file or directory: Read ERRORS-file.\n" + exc);
                return false;
            }
            return true;
        }
        #endregion

        #region LIST ITEMS IN DIRECTORY
        /// <summary>
        /// Lists files and directories in another directory on the server
        /// </summary>
        /// <param name="fileEnding"></param>
        /// <returns>A list of files and directories</returns>
        public List<string> ListDirectory(string pathOnServer, string fileEnding = "")
        {
            var directoryContents = new List<string>(); //Create empty list to fill it later.

            //Create ftpWebRequest object with given options to get the Directory Contents. 
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(GetUrl(pathOnServer) + "/");
            ftpWebRequest.Credentials = new NetworkCredential(networkCredential.UserName, networkCredential.Password);
            ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            try
            {
                using (var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse())
                using (var streamReader = new StreamReader(ftpWebResponse.GetResponseStream())) //Get list of the Directory Contents as Stream.
                {
                    string line = string.Empty; //Initial default value for line.
                    do
                    {
                        line = streamReader.ReadLine(); //Read current line of Stream.
                        if (line != null && line.EndsWith(fileEnding))
                        {
                            directoryContents.Add(line); //Add current line to directory contents
                        }
                    }
                    while (!string.IsNullOrEmpty(line)); //Keep reading while the line has value.
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Error listing files and directories: Read ERRORS-file.\n" + exc);
                return null;
            }

            directoryContents.Remove(".");
            directoryContents.Remove("..");

            return directoryContents; //Return the list of directory contents
        }
        #endregion
    }
}
