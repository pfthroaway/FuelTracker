using Extensions;
using Extensions.DatabaseHelp;
using Extensions.DataTypeHelpers;
using FuelTracker.Models.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FuelTracker.Models.Database
{
    /// <summary>Represents database interaction covered by SQLite.</summary>
    internal class SQLiteDatabaseInteraction : IDatabaseInteraction
    {
        private const string _DATABASENAME = "FuelTracker.sqlite";
        private readonly string _con = $"Data Source = {Path.Combine(AppData.Location, _DATABASENAME)}; foreign keys = TRUE; Version = 3;";

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        public void VerifyDatabaseIntegrity() => Functions.VerifyFileIntegrity(
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"FuelTracker.{_DATABASENAME}"), _DATABASENAME, AppData.Location);

        #region Fuel

        #region Fuel Transaction Management

        /// <summary>Deletes a <see cref="FuelTransaction"/> from the database.</summary>
        /// <param name="deleteTransaction"><see cref="FuelTransaction"/> to be deleted</param>
        /// <returns>Returns true if deletion is successful</returns>
        public Task<bool> DeleteTransaction(FuelTransaction deleteTransaction)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "DELETE FROM Transactions WHERE [TransactionID] = @transactionID" };
            cmd.Parameters.AddWithValue("@transactionID", deleteTransaction.TranscationID);

            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        /// <summary>Gets the next <see cref="FuelTransaction"/> ID autoincrement value in the database for the <see cref="Vehicle"/ table.</summary>
        /// <returns>Next <see cref="FuelTransaction"/> ID value</returns>
        public async Task<int> GetNextTransactionIndex()
        {
            DataSet ds = await SQLiteHelper.FillDataSet(_con, "SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Transactions'");

            return ds.Tables[0].Rows.Count > 0 ? Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1 : 1;
        }

        /// <summary>Loads all <see cref="FuelTransaction"/>s associated with a specific <see cref="Vehicle"/.</summary>
        /// <param name="vehicleID"><see cref="Vehicle"/> ID</param>
        /// <returns>Returns all <see cref="FuelTransaction"/>s associated with a specific <see cref="Vehicle"/.</returns>
        public async Task<List<FuelTransaction>> LoadTransactions(int vehicleID)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "SELECT * FROM Transactions WHERE VehicleID = @id" };
            cmd.Parameters.AddWithValue("@id", vehicleID);

            DataSet ds = await SQLiteHelper.FillDataSet(_con, cmd);

            List<FuelTransaction> transactions = new List<FuelTransaction>();

            if (ds.Tables[0].Rows.Count > 0)
            {
                transactions.AddRange(from DataRow dr in ds.Tables[0].Rows select new FuelTransaction(Int32Helper.Parse(dr["TransactionID"]), Int32Helper.Parse(dr["VehicleID"]), DateTimeHelper.Parse(dr["Date"]), dr["Store"].ToString(), Int32Helper.Parse(dr["Octane"]), DecimalHelper.Parse(dr["Distance"]), DecimalHelper.Parse(dr["Gallons"]), DecimalHelper.Parse(dr["Price"]), DecimalHelper.Parse(dr["Odometer"]), Int32Helper.Parse(dr["Range"])));
                transactions = transactions.OrderByDescending(transaction => transaction.Date).ToList();
            }

            return transactions;
        }

        /// <summary>Modifies an existing <see cref="FuelTransaction"/>.</summary>
        /// <param name="oldTransaction">Existing <see cref="FuelTransaction"/></param>
        /// <param name="newTransaction">New <see cref="FuelTransaction"/></param>
        /// <returns>Returns true if modification is successful</returns>
        public Task<bool> ModifyTransaction(FuelTransaction oldTransaction, FuelTransaction newTransaction)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Transactions SET [VehicleID] = @vehicleID, [Store] = @store, [Date] = @date, [Octane] = @octane, [Distance] = @distance, [Gallons] = @gallons, [Price] = @price, [Odometer] = @odometer, [Range] = @range WHERE [TransactionID] = @transactionID"
            };
            cmd.Parameters.AddWithValue("@vehicleID", newTransaction.VehicleID);
            cmd.Parameters.AddWithValue("@store", newTransaction.Store.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@date", newTransaction.DateToString);
            cmd.Parameters.AddWithValue("@octane", newTransaction.Octane);
            cmd.Parameters.AddWithValue("@distance", newTransaction.Distance);
            cmd.Parameters.AddWithValue("@gallons", newTransaction.Gallons);
            cmd.Parameters.AddWithValue("@price", newTransaction.Price);
            cmd.Parameters.AddWithValue("@odometer", newTransaction.Odometer);
            cmd.Parameters.AddWithValue("@range", newTransaction.Range);
            cmd.Parameters.AddWithValue("@transactionID", oldTransaction.TranscationID);

            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        /// <summary>Adds a new <see cref="FuelTransaction"/> to the database.</summary>
        /// <param name="newTransaction"><see cref="FuelTransaction"/> to be added</param>
        /// <returns>Returns true if add is successful</returns>
        public Task<bool> NewTransaction(FuelTransaction newTransaction)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "INSERT INTO Transactions([VehicleID], [Store], [Date], [Octane], [Distance], [Gallons], [Price], [Odometer], [Range])VALUES(@id, @store, @date, @octane, @distance, @gallons, @price, @odometer, @range)" };
            cmd.Parameters.AddWithValue("@id", newTransaction.VehicleID);
            cmd.Parameters.AddWithValue("@store", newTransaction.Store);
            cmd.Parameters.AddWithValue("@date", newTransaction.DateToString);
            cmd.Parameters.AddWithValue("@octane", newTransaction.Octane);
            cmd.Parameters.AddWithValue("@distance", newTransaction.Distance);
            cmd.Parameters.AddWithValue("@gallons", newTransaction.Gallons);
            cmd.Parameters.AddWithValue("@price", newTransaction.Price);
            cmd.Parameters.AddWithValue("@odometer", newTransaction.Odometer);
            cmd.Parameters.AddWithValue("@range", newTransaction.Range);
            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        #endregion Fuel Transaction Management

        #region Vehicle Management

        /// <summary>Changes details in the database regarding a <see cref="Vehicle"/>.</summary>
        /// <param name="oldVehicle">Old <see cref="Vehicle"/> details</param>
        /// <param name="newVehicle">New <see cref="Vehicle"/> details</param>
        /// <returns>Returns true if modification is successful</returns>
        public Task<bool> ModifyVehicle(Vehicle oldVehicle, Vehicle newVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "UPDATE Vehicles SET [Nickname] = @name, [Make] = @make, [Model] = @model, [Year] = @year WHERE [VehicleID] = @vehicleID" };
            cmd.Parameters.AddWithValue("@name", newVehicle.Nickname);
            cmd.Parameters.AddWithValue("@make", newVehicle.Make);
            cmd.Parameters.AddWithValue("@model", newVehicle.Model);
            cmd.Parameters.AddWithValue("@year", newVehicle.Year);
            cmd.Parameters.AddWithValue("@vehicleID", oldVehicle.VehicleID);
            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        /// <summary>Deletes a <see cref="Vehicle"/> and all associated <see cref="FuelTransaction"/>s from the database.</summary>
        /// <param name="deleteVehicle"><see cref="Vehicle"/> to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        public Task<bool> DeleteVehicle(Vehicle deleteVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "DELETE FROM Transactions WHERE [VehicleID] = @vehicleID, DELETE FROM Vehicles WHERE [VehicleID] = @vehicleID" };
            cmd.Parameters.AddWithValue("@vehicleID", deleteVehicle.VehicleID);

            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        /// <summary>Gets the next <see cref="Vehicle"/> ID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next <see cref="Vehicle"/> ID value</returns>
        public async Task<int> GetNextVehicleIndex()
        {
            DataSet ds = await SQLiteHelper.FillDataSet(_con, "SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Vehicles'");

            if (ds.Tables[0].Rows.Count > 0)
                return Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1;
            return 1;
        }

        /// <summary>Loads all <see cref="Vehicle"/>.</summary>
        /// <returns>All <see cref="Vehicle"/></returns>
        public async Task<List<Vehicle>> LoadVehicles()
        {
            DataSet ds = await SQLiteHelper.FillDataSet(_con, "SELECT * FROM Vehicles");
            List<Vehicle> vehicles = new List<Vehicle>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    int vehicleID = Int32Helper.Parse(dataRow["VehicleID"]);
                    Vehicle newVehicle = new Vehicle(vehicleID,
                        dataRow["Nickname"].ToString(), dataRow["Make"].ToString(), dataRow["Model"].ToString(),
                        Int32Helper.Parse(dataRow["Year"]), await LoadTransactions(vehicleID));
                    vehicles.Add(newVehicle);
                }
                vehicles = vehicles.OrderBy(vehicle => vehicle.Nickname).ToList();
            }
            return vehicles;
        }

        /// <summary>Adds a new <see cref="Vehicle"/> to the database.</summary>
        /// <param name="newVehicle"><see cref="Vehicle"/> to be added</param>
        /// <returns>Returns true if the <see cref="Vehicle"/> was successfully added</returns>
        public Task<bool> NewVehicle(Vehicle newVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "INSERT INTO Vehicles([Nickname], [Make], [Model], [Year])VALUES(@name, @make, @model, @year)" };
            cmd.Parameters.AddWithValue("@name", newVehicle.Nickname);
            cmd.Parameters.AddWithValue("@make", newVehicle.Make);
            cmd.Parameters.AddWithValue("@model", newVehicle.Model);
            cmd.Parameters.AddWithValue("@year", newVehicle.Year);
            return SQLiteHelper.ExecuteCommand(_con, cmd);
        }

        #endregion Vehicle Management

        #endregion Fuel
    }
}