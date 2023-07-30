using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnyWebApp.Injections
{
    [ComVisible(true)]
    public class Window
    {
        private readonly Form form;

        public Window(Form form)
        {
            this.form = form;
        }

        public int Width { get => form.Width; set => form.Width = value; }
        public int Height { get => form.Height; set => form.Height = value; }
        public string Title { get => form.Text; set => form.Text = value; }
    }
}
