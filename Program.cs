using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Client_SCM_2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SCMClient client = new SCMClient();
            client.SCMTimer.Elapsed += client.checkConnected;
            client.SCMTimer.Interval = 60000;
            client.SCMTimer.Enabled = true;


            Thread thread = new Thread(client.connect);
            thread.IsBackground = true;
            thread.Start();




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            client.setform(form);
            Application.Run(form);


        }
    }
}
