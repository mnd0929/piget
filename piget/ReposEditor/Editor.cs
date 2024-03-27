using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piget.ReposEditor
{
    public class Editor
    {
        public void Initialize()
        {
            string c = new AuthPage().GetCredentials();
        }
    }
}
