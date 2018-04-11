using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Identity;

namespace ARK
{
    public partial class FormTextToSpeech : Form
    {
        public FormTextToSpeech()
        {
            InitializeComponent();
        }

        private void btnSpeak_Click(object sender, EventArgs e)
        {
            APIUtils.TextToSpeech.Speak(txtTextToSpeech.Text);
            //SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            //synthesizer.Volume = 100;  // 0...100
            //synthesizer.Rate = -2;     // -10...10

            //// Synchronous
            ////synthesizer.Speak(txtTextToSpeech.Text);

            //// Asynchronous
            //synthesizer.SpeakAsync(txtTextToSpeech.Text);
        }

        private async void CreateUserAsync()
        {
            //RoleManager<IdentityRole> roleManager1 = ApplicationIdentityManager.GetRoleManager();
            //IdentityRole role1 = new IdentityRole("SuperAdmin");
            //IdentityResult result2 = await roleManager1.CreateAsync(role1);

            //role1 = new IdentityRole("CaseOfficer");
            //result2 = await roleManager1.CreateAsync(role1);

            //role1 = new IdentityRole("EnrolmentOfficer");
            //result2 = await roleManager1.CreateAsync(role1);

            //role1 = new IdentityRole("Supervisee");
            //result2 = await roleManager1.CreateAsync(role1);
            //MessageBox.Show("Ngon");
            //return;
            UserManager<ApplicationUser> userManager = ApplicationIdentityManager.GetUserManager();
            ApplicationUser user = new ApplicationUser()
            {
                UserName = "minhdq1982@gmail.com"
            };

            IdentityResult result = await userManager.CreateAsync(user, "123456");
            if (result.Succeeded)
            {
                RoleManager<IdentityRole> roleManager = ApplicationIdentityManager.GetRoleManager();
                userManager.AddToRole(user.Id, "Supervisee");
            }
            else
            {
                MessageBox.Show("Hỏng hết cơm cháo");
            }
        }
    }
}
