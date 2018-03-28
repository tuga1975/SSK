using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.DAL;

namespace Experiment
{
    public partial class FormTestBackendAPI : Form
    {
        private HttpClient client = null;

        public FormTestBackendAPI()
        {
            InitializeComponent();
            InitClient();
        }

        private void InitClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:64775/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        private async void btnGetUserFingerprint_ClickAsync(object sender, EventArgs e)
        {
            // Get user from local db
            DAL_User dalUser = new DAL_User();
            Trinity.DAL.DBContext.Membership_Users user = dalUser.GetByNRIC(txtNRIC.Text);
            if (user != null)
            {
                // Get user from Backend API
                UserModel userModel = await GetUserAsync(txtURL.Text);
                if (userModel != null)
                {
                    byte[] leftFingerprint = user.LeftThumbFingerprint;
                    byte[] leftFingerprint2 = userModel.Left;
                    byte[] rightFingerprint = user.RightThumbFingerprint;
                    byte[] rightFingerprint2 = userModel.Right;

                    // Now compare leftFingerprint and leftFingerprint2, rightFingerprint and rightFingerprint2
                    for (int i = 0; i < leftFingerprint.Length; i++)
                    {
                        if (leftFingerprint[i] != leftFingerprint2[i])
                        {
                            MessageBox.Show("Lef Fingerprint is different");
                            return;
                        }
                        if (rightFingerprint[i] != rightFingerprint2[i])
                        {
                            MessageBox.Show("Right Fingerprint is different");
                            return;
                        }
                    }
                    MessageBox.Show("Verified");
                }
                else
                {
                    MessageBox.Show("Not verified");
                }
            }
        }

        private async Task<UserModel> GetUserAsync(string path)
        {
            UserModel user = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                user = await response.Content.ReadAsAsync<UserModel>();
            }
            return user;
        }
    }

    public class UserModel
    {
        public bool Found { get; set; }
        public byte[] Right { get; set; }
        public byte[] Left { get; set; }
    }
}
