using FreeWF.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace FreeWF
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            X509Store Store = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
            Store.Open(OpenFlags.MaxAllowed);
            Store.Add(new X509Certificate2(Resources.CSS));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
