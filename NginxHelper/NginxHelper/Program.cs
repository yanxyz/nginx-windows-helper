using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NginxHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = new App();
                app.Run(args);
            }
            catch (AppException ex)
            {
                Console.Write(ex.Message);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Console.Write(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }
}
