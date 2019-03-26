using Newtonsoft.Json;
using nucoris.domain;
using System;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace nucoris.console.tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestJSON();
        }

        private static void TestJSON()
        {
            Patient patient = Patient.NewPatient("myMRN", "myGiven", "myFamily", new DateTime(2019, 2, 2));
            patient.Add(new Allergy("Nuts"));
            string patientOutput = JsonConvert.SerializeObject(patient);
            Patient restoredPatient = JsonConvert.DeserializeObject<Patient>(patientOutput);
            string patientOutputAsCamel = JsonConvert.SerializeObject(patient,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            Patient restoredPatientFromCamel = JsonConvert.DeserializeObject<Patient>(patientOutputAsCamel);

            string patientJSON = @"
    {
        ""hasSystemGeneratedMrn"": false,
        ""givenName"": ""myGiven"",
        ""familyName"": ""myFamily"",
        ""dateOfBirth"": ""2019-02-02T00:00:00"",
        ""allergies"": [
            {
                ""name"": ""Nuts""
            }
        ],
        ""patientIdentity"": {
            ""id"": ""3f4e7054-d90b-41ea-867b-9187e9e8c86e"",
            ""mrn"": ""myMRN""
        },
        ""id"": ""3f4e7054-d90b-41ea-867b-9187e9e8c86e""
    }";
            restoredPatient = JsonConvert.DeserializeObject<Patient>(patientJSON);


            dynamic dynaPat = patient;
            //dynaPat.Extra = "This fails because 'Extra' is not a member of Patient class";
            string dynaPatOutput = JsonConvert.SerializeObject(dynaPat);

            dynamic dynaObj = new ExpandoObject();            
            dynaObj.id = patient.Id;
            dynaObj.partitionKey = patient.PatientIdentity.Mrn;
            dynaObj.docType = "Patient";
            dynaObj.docSubType = patient.GetType().FullName;
            dynaObj.Patient = patient;
            string dynaObjOutput = JsonConvert.SerializeObject(dynaObj, 
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var dsDynaObj = JsonConvert.DeserializeObject(dynaObjOutput,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            dynamic dsExpando = JsonConvert.DeserializeObject<ExpandoObject>(dynaObjOutput);
            var dsPatient = dsExpando.Patient;
            //var dsPatient =  (Type.GetType(dynaObj.DocSubType));

            Console.WriteLine(patientOutput);
        }
    }
}
