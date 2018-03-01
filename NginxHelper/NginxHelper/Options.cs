using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NginxHelper
{
    class Options
    {
        public List<string> Args;
        public bool Help { get; }
        public bool Wait { get; }

        public Options(string[] args)
        {
            Args = new List<string>();

            int i = 0;
            foreach (var item in args)
            {
                ++i;
                if (item[0] == '-')
                {
                    if (item == "-h" || item == "--help")
                        Help = true;
                    else if (item == "-w" || item == "--wait")
                        Wait = true;
                }
                else
                {
                    Args.Add(item);
                }
            }

            if (Args.Count == 0)
            {
                Help = true;
            }
        }
    }
}
