using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public partial class NovelUserControl : UserControl
    {
        public NovelUserControl()
        {
            InitializeComponent();
        }

        private string Description
        {
            set
            {
                toolTip1.SetToolTip(panel1, value);
            }
        }
    }
}
