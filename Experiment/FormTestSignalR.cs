using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.SignalR;

namespace Experiment
{
    public partial class FormTestSignalR : Form
    {
        public FormTestSignalR()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Client signalrClient = Client.Instance;
            signalrClient.SendToSupervisee(txtFromUser.Text, txtToUser.Text, txtSubject.Text, txtContent.Text, EnumNotificationTypes.Notification);
        }
    }
}
