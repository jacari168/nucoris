In nucoris we have 4 types of data:

* Patient Data
	- stored under partitionKey = "P_" + patientId
	- only queried by docType or id

* Materialized Views
	- stored under partitionKey = "V_" + query-specific descriptor. E.g. "V_ActiveOrdersQuery"
	- queried by id or any of the fields under the main piece of data such as "PatientQuery" or "OrderQuery"

* User Data
	- store under partitionKey = "U_" + userId
	- only queried by id

* Reference Data
	- stored under partitionKey = "R_" + data-specific descriptor. E.g. "R_Medication"
	- only queried by id

So, our indexing policy excludes everything except
1) The 3 metadata fields: partitionKey, docType, docSubType
2) All under docContents/PatientQuery, docContents/OrderQuery

This may save around 30% RU in write operations of patient data.