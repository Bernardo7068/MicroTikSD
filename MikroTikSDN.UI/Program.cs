using System;
using System.Windows.Forms;

namespace MikroTikSDN.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // É AQUI A MAGIA: Dizemos ao programa para arrancar com o LoginForm!
            Application.Run(new LoginForm());
        }
    }
}