using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SeleniumPeer.MainApplication
{
    internal class Start
    {
        private static PageObjectCreatorGui _mainGui;
        private static Receiver _mainReceiver;

        /// <summary>
        ///     This is where the SeleniumPeer application begins. Generally, it creates an
        ///     instantiaion of the primary GUI and an instantiation of the extension receiver. Then,
        ///     it sets up threading for the two by creating a background worker for the receiver
        ///     and running the GUI thread through the Application class.
        /// </summary>
        /// <param name="args">
        ///     The "args" parameter is not used.
        /// </param>
        [STAThread]
        private static void Main(string[] args)
        {
            _mainGui = new PageObjectCreatorGui();
            //Note: The port number 8055 is meaningless. It simply must be the same as the port
            //number used in content.js within the chrome extension.
            _mainReceiver = new Receiver(_mainGui, 8055);

            var worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = false;
            worker.WorkerReportsProgress = false;
            //The background worker's thread begin's in the receiver's StartRecording method.
            worker.DoWork += _mainReceiver.StartReceiving;
            //This starts the worker.
            worker.RunWorkerAsync();

            //This starts and controls the GUI's thread. Part of and required for the C# Form Paradigm.
            Application.Run(_mainGui);
        }
    }
}