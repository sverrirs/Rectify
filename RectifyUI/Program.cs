using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RectifyUI
{
    static class Program
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config.xml"));
            log4net.Config.XmlConfigurator.Configure(GetLog4NewEmbeddedConfigFile("log4net.config.xml"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static Stream GetLog4NewEmbeddedConfigFile(string filename )
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            foreach (string resName in assembly.GetManifestResourceNames())
            {
                if (resName.EndsWith(filename))
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(assembly.GetManifestResourceStream(resName),
                        System.Text.Encoding.UTF8);
                    return reader.BaseStream;
                }
            }
            return null;
        }
    }
}
