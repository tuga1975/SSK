using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSK.Models
{
    class ProfileModel
    {
        public string ParticularsName { get; set; }
        public string NRIC { get; set; }
        public string DOB { get; set; }
        public string Nationlality { get; set; }
        public string MaritalStatus { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondaryContact { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        public AddressModel Address { get; set; }
        public NextOfKinDetailsModel NextOfKinDetails { get; set; }
        public EmployerDetailsModel EmployerDetails { get; set; }
        public ProfileModel()
        {
            ParticularsName = "John Doe";
            NRIC = "S8853329A";
            DOB = "01/01/1988";
            Nationlality = "Singapore";
            MaritalStatus = "Single";
            PrimaryContact = "0123456789";
            SecondaryContact = "";
            PrimaryEmail = "John@doe.com";
            SecondaryEmail = "";
            Address = new AddressModel();
            NextOfKinDetails = new NextOfKinDetailsModel();
            EmployerDetails = new EmployerDetailsModel();
        }

    }

    class AddressModel
    {
        public string AddressHouseNumber { get; set; }
        public string AddressUnitNumber { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressCountry { get; set; }

        public AddressModel()
        {
            AddressHouseNumber = "2017";
            AddressUnitNumber = "12";
            AddressStreetName = "12";
            AddressCountry = "SG";

        }
    }

    class NextOfKinDetailsModel
    {
        public string NextOfKinDetailsName { get; set; }
        public string NextOfKinDetailsContactNumber { get; set; }
        public string NextOfKinDetailsRelationship { get; set; }
        public string NextOfKinDetailsHouseNumber { get; set; }
        public string NextOfKinDetailsUnitNumber { get; set; }
        public string NextOfKinDetailsStreetName { get; set; }
        public string NextOfKinDetailsCountry { get; set; }

        public NextOfKinDetailsModel()
        {
            NextOfKinDetailsName = "John Doe Jr";
            NextOfKinDetailsContactNumber = "0987654321";
            NextOfKinDetailsRelationship = "Son";
            NextOfKinDetailsHouseNumber = "2017";
            NextOfKinDetailsUnitNumber = "12";
            NextOfKinDetailsStreetName = "12";
            NextOfKinDetailsCountry = "SG";
        }
    }

    class EmployerDetailsModel
    {
        public string EmployerName { get; set; }
        public string EmployerContact { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public string StartDate { get; set; }
        public string EndDateAndRemarks { get; set; }

        public EmployerDetailsModel()
        {
            EmployerName = "John Doe";
            EmployerContact = "0123456789";
            CompanyName = "TVO";
            JobTitle = "DEV";
            StartDate = "01/01/2018";
            EndDateAndRemarks = "";
        }
    }
}
