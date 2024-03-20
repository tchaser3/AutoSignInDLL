/* Title:           Auto Log Off
 * Date:            4-24-16
 * Author:          Terry Holmes
 *
 * Description:     This class will do the auto log off */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DateSearchDLL;
using NewEmployeeDLL;
using NewEventLogDLL;
using VehicleHistoryDLL;
using VehiclesDLL;

namespace AutoSignInDLL
{
    public class AutoSignInClass
    {
        //setting up the classes
        EmployeeClass TheEmployeeClass = new EmployeeClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        VehicleHistoryClass TheVehicleHistorClass = new VehicleHistoryClass();
        VehicleClass TheVehicleClass = new VehicleClass();
        DateSearchClass TheDateSearchClass = new DateSearchClass();

        //setting up the data
        EmployeesDataSet TheEmployeeDataSet;
        VehiclesDataSet TheVehiclesDataSet;

        //setting autosign in date
        AutoSignInDateDataSet aAutoSignInDateDataSet;
        AutoSignInDateDataSetTableAdapters.autosignindateTableAdapter aAutoSignInDateTableAdapter;

        struct EmployeeStructure
        {
            public int mintEmployeeID;
            public string mstrLastName;
            public string mstrFirstName;
            public string mstrHomeOffice;
        }

        //variables for structure
        EmployeeStructure[] TheWarehouseStructure;
        int mintWarehouseCounter;
        int mintWarehouseUpperLimit;
        
        //setting local variables
        bool mblnRunRoutine = true;
        DateTime mdatTodaysDate;
        DateTime mdatTableDate;
       
        public bool AutoSignInProcess()
        {
            bool blnFatalError = false;

            try
            {
                //loading the structure
                SetAutoSignInVariables();
                FillEmployeeStructure();
                blnFatalError = SignInVehicles();
                UpdateAutoSignInDateDB();

                //blnFatalError = mblnRunRoutine;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Auto Sign In Class // Auto Sign In Process " + Ex.Message);

                blnFatalError = true;
            }

            return blnFatalError;
        }
        public bool SignInVehicles()
        {
            //setting local variables
            bool blnFatalError = false;
            int intVehicleCounter;
            int intVehicleNumberOfRecords;
            string strHomeOfficeForSearch;
            int intWarehouseCounter;
            int intWarehouseID = 0;
            string strRemoteVehicle;
            string strVehicleUnderRepair;
            string strActive;
            string strAvailable;
            int intVehicleID;
            int intBJCNumber;
            bool blnIsBoolean;
            DateTime datTransactionDate;
            DateTime datTodaysDate = DateTime.Now;
                                               
            try
            {
                if(mblnRunRoutine == true)
                {
                    //setting up the vehicles data set
                    TheVehiclesDataSet = TheVehicleClass.GetVehiclesInfo();
                    
                    //getting the number of Variables
                    intVehicleNumberOfRecords = TheVehiclesDataSet.vehicles.Rows.Count - 1;
                    datTodaysDate = TheDateSearchClass.RemoveTime(datTodaysDate);
                    
                    //beginning vehicle loop
                    for(intVehicleCounter = 0; intVehicleCounter <= intVehicleNumberOfRecords; intVehicleCounter++)
                    {
                        strActive = TheVehiclesDataSet.vehicles[intVehicleCounter].Active.ToUpper();
                        strAvailable = TheVehiclesDataSet.vehicles[intVehicleCounter].Available.ToUpper();
                        datTransactionDate = TheDateSearchClass.RemoveTime(TheVehiclesDataSet.vehicles[intVehicleCounter].Date);

                        //checking data for null
                        blnIsBoolean = TheVehiclesDataSet.vehicles[intVehicleCounter].IsOutOfTownNull();
                        if(blnIsBoolean == true)
                        {
                            strRemoteVehicle = "NO";
                        }
                        else
                        {
                            strRemoteVehicle = TheVehiclesDataSet.vehicles[intVehicleCounter].OutOfTown.ToUpper();
                        }

                        strVehicleUnderRepair = TheVehiclesDataSet.vehicles[intVehicleCounter].DownForRepairs.ToUpper();
                        strHomeOfficeForSearch = TheVehiclesDataSet.vehicles[intVehicleCounter].HomeOffice.ToUpper();
                        intVehicleID = TheVehiclesDataSet.vehicles[intVehicleCounter].VehicleID;
                        intBJCNumber = TheVehiclesDataSet.vehicles[intVehicleCounter].BJCNumber;
                        
                        //if statements
                        if(strActive == "YES")
                        {
                            if (strAvailable == "NO")
                            {
                                if (strRemoteVehicle == "NO")
                                {
                                    if(datTransactionDate < datTodaysDate)
                                    {
                                        //updating the variables
                                        TheVehiclesDataSet.vehicles[intVehicleCounter].Available = "YES";
                                        TheVehiclesDataSet.vehicles[intVehicleCounter].Date = DateTime.Now;

                                        for (intWarehouseCounter = 0; intWarehouseCounter <= mintWarehouseUpperLimit; intWarehouseCounter++)
                                        {
                                            if (strHomeOfficeForSearch == TheWarehouseStructure[intWarehouseCounter].mstrHomeOffice)
                                            {
                                                intWarehouseID = TheWarehouseStructure[intWarehouseCounter].mintEmployeeID;
                                                TheVehiclesDataSet.vehicles[intVehicleCounter].EmployeeID = intWarehouseID;
                                            }
                                        }

                                        TheVehicleClass.UpdateVehiclesDB(TheVehiclesDataSet);
                                        TheVehicleHistorClass.CreateVehicleHistoryTransaction(intVehicleID, intBJCNumber, intWarehouseID, intWarehouseID, "AUTO LOG IN", "NO");

                                    }
                                }
                            } 
                        }
                    }     
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Auto Sign In Vehicles Class // Sign In Vehicles " + Ex.Message);

                blnFatalError = true;
            }

            return blnFatalError;
        }
        
        private void SetAutoSignInVariables()
        {
            try
            {
                //setting up the data
                aAutoSignInDateDataSet = new AutoSignInDateDataSet();
                aAutoSignInDateTableAdapter = new AutoSignInDateDataSetTableAdapters.autosignindateTableAdapter();
                aAutoSignInDateTableAdapter.Fill(aAutoSignInDateDataSet.autosignindate);

                //getting the table date
                mdatTableDate = aAutoSignInDateDataSet.autosignindate[0].AutoSignInDate;
                mdatTableDate = TheDateSearchClass.RemoveTime(mdatTableDate);
                mdatTodaysDate = DateTime.Now;
                mdatTodaysDate = TheDateSearchClass.RemoveTime(mdatTodaysDate);

                if(mdatTodaysDate > mdatTableDate)
                {
                    mblnRunRoutine = true;
                }
                else
                {
                    mblnRunRoutine = true;
                }

            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Auto Sign In Class // Set Auto Sign In Variables " + Ex.Message);
            }
            
        }
        private void UpdateAutoSignInDateDB()
        {
            try
            {
                aAutoSignInDateDataSet = new AutoSignInDateDataSet();
                aAutoSignInDateTableAdapter = new AutoSignInDateDataSetTableAdapters.autosignindateTableAdapter();
                aAutoSignInDateTableAdapter.Fill(aAutoSignInDateDataSet.autosignindate);

                aAutoSignInDateDataSet.autosignindate[0].AutoSignInDate = mdatTodaysDate;
                aAutoSignInDateTableAdapter.Update(aAutoSignInDateDataSet.autosignindate);
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Auto Sign In Class // Updating Auto Sign In DB " + Ex.Message);
            }
        }

        //Creating method to fill employee structure
        private void FillEmployeeStructure()
        {
            //set local variables
            int intCounter;
            int intNumberOfRecords;
            string strLastNameForSearch;
            string strLastNameFromTable;
            
            try
            {
                //loading the data set
                TheEmployeeDataSet = TheEmployeeClass.GetEmployeesInfo();

                //getting ready for the loop
                intNumberOfRecords = TheEmployeeDataSet.employees.Rows.Count - 1;
                TheWarehouseStructure = new EmployeeStructure[intNumberOfRecords + 1];
                mintWarehouseCounter = 0;
                strLastNameForSearch = "WAREHOUSE";

                //beginning loop
                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    //checking to see if the record is active
                    strLastNameFromTable = TheEmployeeDataSet.employees[intCounter].LastName.ToUpper();

                  
                        if(strLastNameForSearch == strLastNameFromTable)
                        {
                            //loading up the warehouse structure
                            TheWarehouseStructure[mintWarehouseCounter].mintEmployeeID = TheEmployeeDataSet.employees[intCounter].EmployeeID;
                            TheWarehouseStructure[mintWarehouseCounter].mstrFirstName = TheEmployeeDataSet.employees[intCounter].FirstName.ToUpper();
                            TheWarehouseStructure[mintWarehouseCounter].mstrLastName = strLastNameFromTable;
                            TheWarehouseStructure[mintWarehouseCounter].mstrHomeOffice = TheEmployeeDataSet.employees[intCounter].HomeOffice.ToUpper();
                            mintWarehouseUpperLimit = mintWarehouseCounter;
                            mintWarehouseCounter++;
                        }

                    }                

                //setting the counter
                mintWarehouseCounter = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "The Auto Sign In Date " + Ex.Message);
            }
        }
        
    }
}
