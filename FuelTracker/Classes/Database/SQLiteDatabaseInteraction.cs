using Extensions;
using Extensions.DatabaseHelp;
using Extensions.DataTypeHelpers;
using Extensions.Encryption;
using FuelTracker.Classes.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FuelTracker.Classes.Database
{
    internal class SQLiteDatabaseInteraction : IDatabaseInteraction
    {
        // ReSharper disable once InconsistentNaming
        private const string _DATABASENAME = "FuelTracker.sqlite";

        private readonly string _con = $"Data Source = {_DATABASENAME};Version=3;PRAGMA foreign_keys = ON";

        #region Database Interaction

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        public void VerifyDatabaseIntegrity()
        {
            Functions.VerifyFileIntegrity(Assembly.GetExecutingAssembly().GetManifestResourceStream($"FuelTracker.{_DATABASENAME}"), _DATABASENAME);
        }

        #endregion Database Interaction

        #region User Management

        /// <summary>Logs in a User.</summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plaintext password</param>
        /// <returns>True is successful authentication</returns>
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "SELECT[Password] FROM Users WHERE[Username] = @name" };
            cmd.Parameters.AddWithValue("@name", username);
            DataSet ds = await SQLite.FillDataSet(cmd, _con);

            return ds.Tables[0].Rows.Count > 0 &&
                   Argon2.ValidatePassword(ds.Tables[0].Rows[0]["Password"].ToString(), password);
        }

        /// <summary>Changes details in the database about a User.</summary>
        /// <param name="oldUser">Old User details</param>
        /// <param name="newUser">New User details</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ChangeUserDetails(User oldUser, User newUser)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "UPDATE Users SET [Username] = @newName, [Password] = @password WHERE [Username] = @oldName" };
            cmd.Parameters.AddWithValue("@newName", newUser.Username);
            cmd.Parameters.AddWithValue("@password", newUser.Password);
            cmd.Parameters.AddWithValue("@oldName", oldUser.Username);
            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Removes a User and all associated Vehicles and Transactions from the database.</summary>
        /// <param name="deleteUser">User to be deleted</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteUser(User deleteUser)
        {
            foreach (Vehicle vehicle in deleteUser.Vehicles)
                await DeleteVehicle(vehicle);
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "DELETE FROM Users WHERE [UserID] = @userID" };
            cmd.Parameters.AddWithValue("@userID", deleteUser.ID);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Gets the next UserID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next UserID value</returns>
        public async Task<int> GetNextUserIndex()
        {
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE [name] = 'Users'", _con);

            return ds.Tables[0].Rows.Count > 0 ? Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1 : 1;
        }

        /// <summary>Loads a User and all associated Vehicles and their Transactions from the database.</summary>
        /// <param name="username">User to be loaded</param>
        /// <returns>Requested User</returns>
        public async Task<User> LoadUser(string username)
        {
            User loadUser = new User();
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "SELECT * FROM Users WHERE [Username] = @name" };
            cmd.Parameters.AddWithValue("@name", username);
            DataSet ds = await SQLite.FillDataSet(cmd, _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                int userID = Int32Helper.Parse(dr["UserID"]);
                loadUser = new User(userID, dr["Username"].ToString(), dr["Password"].ToString(), await LoadVehicles(userID));
            }

            return loadUser;
        }

        /// <summary>Saves a new user to the database.</summary>
        /// <param name="username">Username for new User</param>
        /// <param name="password">Password for new User</param>
        /// <returns>Returns true if User successfully added to database.</returns>
        public async Task<bool> NewUser(string username, string password)
        {
            User newUser = await LoadUser(username);
            if (newUser == null || newUser == new User())
            {
                SQLiteCommand cmd = new SQLiteCommand
                {
                    CommandText =
                        "INSERT INTO Users([Username], [Password])VALUES(@name, @password)"
                };
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@password", password);
                return await SQLite.ExecuteCommand(_con, cmd);
            }
            AppState.DisplayNotification("That username has already been taken. Please choose another.", "Fuel Tracker");
            return false;
        }

        #endregion User Management

        #region Transaction Management

        /// <summary>Deletes a Transaction from the database.</summary>
        /// <param name="deleteTransaction">Transaction to be deleted</param>
        /// <returns>Returns true if deletion successful</returns>
        public async Task<bool> DeleteTransaction(Transaction deleteTransaction)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "DELETE FROM Transactions WHERE [TransactionID] = @transactionID" };
            cmd.Parameters.AddWithValue("@transactionID", deleteTransaction.VehicleID);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Gets the next TransactionID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next TransactionID value</returns>
        public async Task<int> GetNextTransactionIndex()
        {
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Transactions'", _con);

            return ds.Tables[0].Rows.Count > 0 ? Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1 : 1;
        }

        /// <summary>Loads all Transactions associated with a specific Vehicle.</summary>
        /// <param name="vehicleID">Vehicle ID</param>
        /// <returns>Returns all Transactions associated with a specific Vehicle.</returns>
        public async Task<List<Transaction>> LoadTransactions(int vehicleID)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "SELECT * FROM Transactions WHERE VehicleID = @id" };
            cmd.Parameters.AddWithValue("@id", vehicleID);

            DataSet ds = await SQLite.FillDataSet(cmd, _con);

            List<Transaction> transactions = new List<Transaction>();

            if (ds.Tables[0].Rows.Count > 0)
            {
                transactions.AddRange(from DataRow dr in ds.Tables[0].Rows select new Transaction(Int32Helper.Parse(dr["TransactionID"]), Int32Helper.Parse(dr["VehicleID"]), DateTimeHelper.Parse(dr["Date"]), dr["Store"].ToString(), Int32Helper.Parse(dr["Octane"]), DecimalHelper.Parse(dr["Distance"]), DecimalHelper.Parse(dr["Gallons"]), DecimalHelper.Parse(dr["Price"]), DecimalHelper.Parse(dr["Odometer"]), Int32Helper.Parse(dr["Range"])));
                transactions = transactions.OrderByDescending(transaction => transaction.Date).ToList();
            }

            return transactions;
        }

        /// <summary>Modifies an existing Transaction.</summary>
        /// <param name="oldTransaction">Existing Transaction</param>
        /// <param name="newTransaction">New Transaction</param>
        /// <returns>Returns true if modification successful</returns>
        public async Task<bool> ModifyTransaction(Transaction oldTransaction, Transaction newTransaction)
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
            cmd.Parameters.AddWithValue("@transactionID", newTransaction.TranscationID);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Adds a new Transaction to the database.</summary>
        /// <param name="newTransaction">Transaction to be added</param>
        /// <returns>Returns true if add successful</returns>
        public async Task<bool> NewTransaction(Transaction newTransaction)
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
            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Transaction Management

        #region Vehicle Management

        /// <summary>Changes details in the database regarding a Vehicle.</summary>
        /// <param name="oldVehicle">Old Vehicle details</param>
        /// <param name="newVehicle">New Vehicle details</param>
        /// <returns>Returns true if modification successful</returns>
        public async Task<bool> ModifyVehicle(Vehicle oldVehicle, Vehicle newVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "UPDATE Vehicles SET [UserID] = @id, [Nickname] = @name, [Make] = @make, [Model] = @model, [Year] = @year WHERE [VehicleID] = @vehicleID" };
            cmd.Parameters.AddWithValue("@id", newVehicle.UserID);
            cmd.Parameters.AddWithValue("@name", newVehicle.Nickname);
            cmd.Parameters.AddWithValue("@make", newVehicle.Make);
            cmd.Parameters.AddWithValue("@model", newVehicle.Model);
            cmd.Parameters.AddWithValue("@year", newVehicle.Year);
            cmd.Parameters.AddWithValue("@vehicleID", oldVehicle.VehicleID);
            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Deletes a Vehicle and all associated Transactions from the database.</summary>
        /// <param name="deleteVehicle">Vehicle to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        public async Task<bool> DeleteVehicle(Vehicle deleteVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "DELETE FROM Vehicles WHERE [VehicleID] = @vehicleID; DELETE FROM Transactions WHERE [VehicleID] = @vehicleID" };
            cmd.Parameters.AddWithValue("@vehicleID", deleteVehicle.VehicleID);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Gets the next VehicleID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next VehicleID value</returns>
        public async Task<int> GetNextVehicleIndex()
        {
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Vehicles'", _con);

            if (ds.Tables[0].Rows.Count > 0)
                return Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1;
            return 1;
        }

        /// <summary>Loads all Vehicles associated with a User.</summary>
        /// <param name="userID">User ID</param>
        /// <returns></returns>
        public async Task<List<Vehicle>> LoadVehicles(int userID)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "SELECT * FROM Vehicles WHERE UserID = @id" };
            cmd.Parameters.AddWithValue("@id", userID);
            DataSet ds = await SQLite.FillDataSet(cmd, _con);
            List<Vehicle> vehicles = new List<Vehicle>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    int vehicleID = Int32Helper.Parse(dataRow["VehicleID"]);
                    Vehicle newVehicle = new Vehicle(vehicleID,
                        Int32Helper.Parse(dataRow["UserID"]),
                        dataRow["Nickname"].ToString(), dataRow["Make"].ToString(), dataRow["Model"].ToString(),
                        Int32Helper.Parse(dataRow["Year"]), await LoadTransactions(vehicleID));
                    vehicles.Add(newVehicle);
                }
                vehicles = vehicles.OrderBy(vehicle => vehicle.Nickname).ToList();
            }
            return vehicles;
        }

        /// <summary>Adds a new Vehicle to the database.</summary>
        /// <param name="newVehicle">Vehicle to be added</param>
        /// <returns>Returns whether the Vehicle was successfully added</returns>
        public async Task<bool> NewVehicle(Vehicle newVehicle)
        {
            SQLiteCommand cmd = new SQLiteCommand { CommandText = "INSERT INTO Vehicles([UserID], [Nickname], [Make], [Model], [Year])VALUES(@id, @name, @make, @model, @year)" };
            cmd.Parameters.AddWithValue("@id", newVehicle.UserID);
            cmd.Parameters.AddWithValue("@name", newVehicle.Nickname);
            cmd.Parameters.AddWithValue("@make", newVehicle.Make);
            cmd.Parameters.AddWithValue("@model", newVehicle.Model);
            cmd.Parameters.AddWithValue("@year", newVehicle.Year);
            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Vehicle Management
    }
}