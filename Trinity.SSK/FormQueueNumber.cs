using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK
{
    public partial class FormQueueNumber : Form
    {
        private static FormQueueNumber _instance = null;
        public static FormQueueNumber GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FormQueueNumber();
            }
            return _instance;
        }
        public FormQueueNumber()
        {
            InitializeComponent();
        }

        public void ShowOnSecondaryScreen()
        {
            if (Screen.AllScreens.Count() > 1)
            {
                if (Screen.AllScreens[0].Primary)
                {
                    this.Location = Screen.AllScreens[1].WorkingArea.Location;
                }
                else
                {
                    this.Location = Screen.AllScreens[0].WorkingArea.Location;
                }
            }
        }
        public void ShowQueueNumber(string queueNumber)
        {
            this.Invoke((Action)(() => lblQueueNumber.Text = queueNumber));       
        }
    }
}
