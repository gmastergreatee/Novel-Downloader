﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            set => toolTip1.SetToolTip(panel1, value);
        }
    }
}
