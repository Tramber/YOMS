using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Oms.Server.host
{
    public class Bootstapper
    {
        private static ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Main()
        {
            try
            {
                var hostedGuest = new HostedGuest();
                hostedGuest.Start();
                Console.ReadKey();
                hostedGuest.Stop();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
