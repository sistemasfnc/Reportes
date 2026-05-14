using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Security
    {
        public int idprofile { get; set; }

        public int idaccess { get; set; }

        public string profilename { get; set; }

        public string accessname { get; set; }
    }

    [Serializable]
    public class Profile
    {
        public int idprofile { get; set; }

        public string name { get; set; }
    }

    [Serializable]
    public class Access
    {
        public int idaccess { get; set; }

        public string name { get; set; }
    }

    public enum ProfileEnum
    {
        cashier = 1,
        invoicingaux = 2,
        director = 3,
        administrator = 4,
        healthcarecoordinator = 5,
        rostercoordinator = 6,
        educationcoordinator = 7,
        investigationcoordiator = 8,
        rhbcashier = 10,
        admincoordinator = 11,
        adminaux = 12,
        pharmacycoordinator = 13,
    }
}
