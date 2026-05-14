using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    [Serializable]
    public class Generic
    {
        public int id { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public string table { get; set; }

        public DateTime date { get; set; }
    }

    [Serializable]
    public class ACL
    {
        public int idprofile { get; set; }

        public int idmodule { get; set; }
    }

    public enum ChargeStatus
    {
        incomplete = 1,
        saved = 2,
        dispatched = 3,
        recieved = 4,
        readytoinvoice = 5,
        returned = 6,
        recievedreturned = 7,
        invoicedpending = 8,
        readytoinvoicepending = 9,
        intreatment = 10,
        intreatmentnoncentral = 11,
        intreatmentreturned = 12,
    }

    public enum Permissions
    {
        entrylist = 1,
        sendcentral = 2,
        returnreception = 3,
        returnresponse = 4,
        entryreception = 5,
        entryreturn = 6,
        sendreturn = 7,
        supportpendingreport = 8,
        pendingsendreport = 9,
        returnreport = 10,
        supportsave = 11,
        listuser = 12,
        createuser = 13,
        edituser = 14,
        entryreport = 15,
        chargesnotinvoicedreport = 16,
        surplusnotinvoicedreport = 17, 
        chargesnottransactedreport = 18,
        chargestatusreport = 19,
        invoicereport = 20,   
        logreport = 21,
        invoicelist = 22,
        chargescanceledreport = 23,
        chargesfci = 24,
        readytoinvoice = 25,
        ripsreport = 26,
        recievecomment = 27,
        responsecomment = 28,
        createadmission = 29,
        listemployee = 30,
        filteremployee = 31,
        assigncost = 32,
        savecost = 33,
        costreport = 34,
        generatecost = 35,
        createhospadmission = 36,
        listcostmaster = 37,
        editcost = 38,
        viewspecialcost = 39,
        addspecialcost = 40,
        rhbcharges = 41,
        relationshipgeneration = 42,
        relationshiplist = 43,
        relationshipvalidation = 44,
        relationshipreport = 45,
        specialcostassign = 46,
        programsinvoices = 47,
        compensardematerialize = 48,
        passwordtrunk = 49,
        pharmacylist = 50,
        pharmacyedit = 51,
    }

    public enum relationshipstatus
    {
        none = 0,
        sent = 1,
        pending = 2,
        completed = 3,
    }

}
