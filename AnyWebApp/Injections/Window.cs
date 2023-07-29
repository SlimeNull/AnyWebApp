using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnyWebApp.Injections
{
    public class Window
    {
        private readonly Form form;

        public Window(Form form)
        {
            this.form = form;
        }

        public int width { get => form.Width; set => form.Width = value; }
        public int height { get => form.Height; set => form.Height = value; }
        public string title { get => form.Text; set => form.Text = value; }
    }
}
