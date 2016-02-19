using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This class is, essentially, the backend of the chrome extension. It operates by constructing
    ///     a raw TcpListener and Socket, and then by waiting for the AJAX calls from content.js in the
    ///     extension. Primarily, it processes the HTTP request, parses JSON, and sends a UserAction class
    ///     to an existing SeleniumPeerReceiverForm.
    /// </summary>
    public class Receiver
    {
        //This variable can change with SetReceivingForm, but should never be null!
        private static SeleniumPeerReceiverForm _receivingForm;

        //Simple localhost server address to listen on. Safe, unless the port is opened?
        private static readonly IPAddress IpAddress = Dns.GetHostEntry("localhost").AddressList[0];
        private readonly byte[] _bytes = new byte[65535];
        private readonly TcpListener _listener;
        private int _numBytes;
        private Socket _socket;

        /// <summary>
        ///     Instantiates the receiving form and the the TCP listener, but lets the BackgroundWorker
        ///     in Start.cs to thread the socket's actions.
        /// </summary>
        /// <param name="gui">
        ///     Not used.
        /// </param>
        /// <param name="port">
        ///     Not used.
        /// </param>
        public Receiver(SeleniumPeerReceiverForm gui, int port)
        {
            _receivingForm = gui;

            _listener = new TcpListener(IpAddress, port);
        }

        /// <summary>
        ///     This is the method used by the BackgroundWorker's thread, and does all the TCP accepting
        ///     and handling. When JSON (from a click) or a character (plain text from a key press) is
        ///     found, the data is passed off to methods to affect the receiving form.
        /// </summary>
        /// <param name="sender">
        ///     Not used.
        /// </param>
        /// <param name="e">
        ///     Not sued.
        /// </param>
        public void StartReceiving(object sender, DoWorkEventArgs e)
        {
            string read;

            //Engages the listener.
            _listener.Start();

            Debug.WriteLine("Receiving actions.");

            //Once SeleniumPeer starts, it should always be able to accpept and properly process a
            //request from content.js.
            while (true)
            {
                Thread.Sleep(100);

                //Accepts a request.
                _socket = _listener.AcceptSocket();

                Debug.WriteLine("Got socket connection!");

                //Reads all the data from the request and fills both _numBytes and _bytes.
                _numBytes = _socket.Receive(_bytes);

                read = "";

                for (int i = 0; i < _numBytes; i++)
                {
                    char chr = Convert.ToChar(_bytes[i]);
                    read += chr;
                    //Newline (\n) is 10. Every newline is caught and checked.
                    if (_bytes[i] == 10)
                    {
                        //If the line is the Accept part of the HTTP header.
                        if (read.StartsWith("Accept:"))
                        {
                            //If this request is JSON, then find the raw JSON data and process it.
                            if (read.Contains("application/json"))
                            {
                                FindJson(i);
                            }
                                //Or, if the request is plain text then there should be a single character
                                //to read at the end of the request.
                            else if (read.Contains("text/plain"))
                            {
                                //Invoke is required to avoid synchronization issues betweeen the
                                //BackgroundWorker's thread and the ReceivingForm's thread.
                                _receivingForm.Invoke(
                                    new MethodInvoker(
                                        //The last byte from the request is send to the AddCharacter
                                        //method of the ReceivingForm.
                                        () => _receivingForm.AddCharacter(Convert.ToChar(_bytes[_numBytes - 1]))));
                            }
                            //Done with this request
                            break;
                        }
                        read = "";
                    }
                }

                //Kill the socket and wait for another connection request!
                _socket.Close();
            }
        }

        /// <summary>
        ///     Simply sets the receiving form. Used when someone accesses the BlockCreator.
        /// </summary>
        /// <param name="form">
        ///     Either a valid instance of BlockCreatorGui or PageObjectCreatorGui.
        /// </param>
        public static void SetReceivingForm(SeleniumPeerReceiverForm form)
        {
            _receivingForm = form;
        }

        /// <summary>
        ///     This method starts at a position in the bytes of the request and finds where
        ///     valid JSON begins and ends. The JSON is parsed, and the parsed object is then
        ///     sent to the ReceivingForm.
        /// </summary>
        /// <param name="startFrom">
        ///     The byte index where the Accept header field was found to be JSON.
        /// </param>
        private void FindJson(int startFrom)
        {
            //The number of open brackets found that need to be closed. When this is 0, JSON has been found.
            int open = 0;
            //Whether or not an opening bracket has been found (indicating the start of JSON text).
            bool json = false;
            //The valid JSON string.
            string jstring = "";

            //For each byte in the request starting from the Accept header.
            for (int i = startFrom; i < _numBytes; i++)
            {
                char chr = Convert.ToChar(_bytes[i]);
                if (chr == '{')
                {
                    if (json)
                    {
                        open++;
                    }
                    json = true;
                }
                //If we have found JSON, then append the current character to a valid JSON string.
                if (json)
                {
                    jstring += chr;
                }
                if (chr == '}')
                {
                    if (open == 0)
                    {
                        Debug.WriteLine(jstring);
                        json = false;
                        var jsonSerializer = new DataContractJsonSerializer(typeof (UserAction));
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jstring));

                        //Tries to serialize the JSON into a UserAction instance.
                        try
                        {
                            var act = jsonSerializer.ReadObject(stream) as UserAction;
                            stream.Close();

                            //Uses Invoke to asynchronously and safely add the new UserAction to the ReceivingForm. 
                            _receivingForm.Invoke(new MethodInvoker(() => _receivingForm.AddAction(act)));
                        }
                            //If there was a serialization error, then it is likely that chrome has not been updated
                            //with the newest content.js version, or the page simply must be refreshed
                        catch (SerializationException e)
                        {
                            MessageBox.Show(
                                "You need to reload your SeleniumPeer Extension AND/OR refresh the current page in Chrome in order to record elements!",
                                "Serialization Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            Debug.WriteLine(e.Message);
                        }
                        break;
                    }
                    //If not enough closing brackets have been found for a valid JSON string, keep reading.
                    open--;
                }
            }
        }
    }
}