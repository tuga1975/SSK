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
            this.Invoke((Action)(() => lblQueueNumber.Text = queueNumber ));
        }
        public void ShowQueueNumber(List<Trinity.BE.QueueNumber> arrayQueue)
        {
            this.Invoke((Action)(() =>
            {
                int maxWidth = this.panelList.Width-10;
                int maxNumberShow = (arrayQueue.Count > 6 ? 6 : arrayQueue.Count) - 1;
                int width = maxWidth / maxNumberShow-10;
                for (int i = 0; i < (arrayQueue.Count > 6 ? 6 : arrayQueue.Count); i++)
                {
                    Trinity.BE.QueueNumber item = arrayQueue[i];
                    if (i == 0)
                    {
                        lblQueueNumber.Text = item.QueueEncoder;
                    }
                    else
                    {
                        Label qs_label = new Label();
                        qs_label.Location = new Point(((i - 1) * width)+(i*10), 10);
                        qs_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        qs_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
                        qs_label.Size = new Size(width, 60);
                        qs_label.Text = item.QueueEncoder;
                        qs_label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        this.panelList.Controls.Add(qs_label);
                    }
                }
            }
            ));
        }
    }
}
