using Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FuelTracker
{
    internal class SQLiteDatabaseInteraction : IDatabaseInteraction
    {
        // ReSharper disable once InconsistentNaming
        private const string _DATABASENAME = "FuelTracker.sqlite";

        private readonly SQLiteConnection _con = new SQLiteConnection { ConnectionString = $"Data Source = {_DATABASENAME};Version=3;PRAGMA foreign_keys = ON" };

        #region Database Interaction

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        public void VerifyDatabaseIntegrity()
        {
            FileInfo dbName = new FileInfo(_DATABASENAME);
            if (!File.Exists(_DATABASENAME) || dbName.Length == 0)
                Functions.ExtractEmbeddedResource("FuelTracker", Directory.GetCurrentDirectory(), "", _DATABASENAME);
        }

        /// <summary>This method fills a DataSet with data from a table.</summary>
        /// <param name="sql">SQL query to be executed</param>
        /// <param name="tableName">In-application table name to be referenced</param>
        /// <returns>Returns DataSet with queried results</returns>
        public async Task<DataSet> FillDataSet(string sql, string tableName)
        {
            DataSet ds = new DataSet();

            await Task.Run(() =>
            {
                try
                {
                    SQLiteDataAdapter da = new SQLiteDataAdapter(sql, _con);
                    da.Fill(ds, tableName);
                }
                catch (Exception ex)
                {
                    AppState.DisplayNotification(ex.Message, "Error Filling DataSet");
                }
                finally { _con.Close(); }
            });
            return ds;
        }

        /// <summary>Executes commands.</summary>
        /// <param name="commands">Commands to be executed</param>
        /// <returns>Returns true if command(s) executed successfully</returns>
        private async Task<bool> ExecuteCommand(params SQLiteCommand[] commands)
        {
            bool success = false;
            await Task.Run(() =>
            {
                try
                {
                    _con.Open();
                    foreach (SQLiteCommand command in commands)
                    {
                        command.Connection = _con;
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    AppState.DisplayNotification(ex.Message, "Error Executing Command");
                }
                finally { _con.Close(); }
            });
            return success;
        }

        #endregion Database Interaction

        #region User Management

        /// <summary>Gets the next UserID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next UserID value</returns>
        public async Task<int> GetNextUserIndex()
        {
            DataSet ds = await FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Users'", "Seq");

            if (ds.Tables[0].Rows.Count > 0)
                return Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1;
            return 1;
        }

        /// <summary>Removes a User and all associated Vehicles and Transactions from the database.</summary>
        /// <param name="deleteUser">User to be deleted</param>
        /// <returns>True if successful</returns>
        public async Task<bool> DeleteUser(User deleteUser)
        {
            string join = string.Join(", ",
                deleteUser.Vehicles.Select(vehicle => vehicle.Nickname) + " " +
                deleteUser.Vehicles.Select(vehicle => vehicle.Model));
            foreach (Vehicle vehicle in deleteUser.Vehicles)
                await DeleteVehicle(vehicle);
            SQLiteCommand cmd = _con.CreateCommand();
            cmd.CommandText = "DELETE FROM Users WHERE [UserID] = @userID";
            cmd.Parameters.AddWithValue("@userID", deleteUser.ID);

            return await ExecuteCommand(cmd);
        }

        /// <summary>Loads a User and all associated Vehicles and their Transactions from the database.</summary>
        /// <param name="username">User to be loaded</param>
        /// <returns>Requested User</returns>
        public async Task<User> LoadUser(string username)
        {
            User loadUser = new User();
            DataSet ds = await FillDataSet($"SELECT * FROM Users WHERE Username='{username}'", "Users");

            if (ds.Tables[0].Rows.Count > 0)
            {
                string.Join(",", ds.Tables[0].Rows.OfType<DataRow>().Select(r => r[0].ToString()));
                DataRow dr = ds.Tables[0].Rows[0];
                int userID = Int32Helper.Parse(dr["UserID"]);
                loadUser = new User(userID, dr["Username"].ToString(), dr["Password"].ToString(), await LoadVehicles(userID));
            }

            return loadUser;
        }

        /// <summary>Logs in a User.</summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plaintext password</param>
        /// <returns>True is successful authentication</returns>
        public async Task<bool> AuthenticateUser(string username, string password)
        {
            bool success = false;
            DataSet ds = await FillDataSet($"SELECT * FROM Users WHERE Username='{username}'", "Users");

            if (ds.Tables[0].Rows.Count > 0)
                if (PBKDF2.ValidatePassword(password, ds.Tables[0].Rows[0]["Password"].ToString()))
                    success = true;

            return success;
        }

        /// <summary>Saves a new user to the database.</summary>
        /// <param name="username">Username for new User</param>
        /// <param name="password">Password for new User</param>
        /// <returns>Returns true if User successfully added to database.</returns>
        public async Task<bool> NewUser(string username, string password)
        {
            DataSet ds = await FillDataSet($"SELECT * FROM Users WHERE [Username]='{username}'", "Users");
            if (ds == null || ds == new DataSet() || ds.Tables[0].Rows.Count == 0)
            {
                SQLiteCommand cmd = _con.CreateCommand();
                cmd.CommandText = $"INSERT INTO Users([Username],[Password])Values('{username}','{PBKDF2.HashPassword(password)}')";

                return await ExecuteCommand(cmd);
            }
            AppState.DisplayNotification("That username has already been taken. Please choose another.", "Fuel Tracker");
            return false;
        }

        #endregion User Management

        #region Transaction Management

        /// <summary>Gets the next TransactionID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next TransactionID value</returns>
        public async Task<int> GetNextTransactionIndex()
        {
            DataSet ds = await FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Transactions'", "Seq");

            if (ds.Tables[0].Rows.Count > 0)
                return Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1;
            return 1;
        }

        /// <summary>Deletes a Transaction from the database.</summary>
        /// <param name="deleteTransaction">Transaction to be deleted</param>
        /// <returns>Returns true if deletion successful</returns>
        public async Task<bool> DeleteTransaction(Transaction deleteTransaction)
        {
            SQLiteCommand cmd = _con.CreateCommand();
            cmd.CommandText = "DELETE FROM Transactions WHERE [TransactionID] = @transactionID";
            cmd.Parameters.AddWithValue("@transactionID", deleteTransaction.VehicleID);

            return await ExecuteCommand(cmd);
        }

        /// <summary>Loads all Transactions associated with a specific Vehicle.</summary>
        /// <param name="vehicleID">Vehicle ID</param>
        /// <returns>Returns all Transactions associated with a specific Vehicle.</returns>
        public async Task<List<Transaction>> LoadTransactions(int vehicleID)
        {
            DataSet ds = await FillDataSet($"SELECT * FROM Transactions WHERE VehicleID='{vehicleID}'", "Transactions");

            List<Transaction> transactions = new List<Transaction>();

            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    Transaction newTransaction = new Transaction(Int32Helper.Parse(dataRow["TransactionID"]),
                        Int32Helper.Parse(dataRow["VehicleID"]), DateTimeHelper.Parse(dataRow["Date"]), dataRow["Store"].ToString(), Int32Helper.Parse(dataRow["Octane"]), DecimalHelper.Parse(dataRow["Distance"]), DecimalHelper.Parse(dataRow["Gallons"]), DecimalHelper.Parse(dataRow["Price"]), DecimalHelper.Parse(dataRow["Odometer"]), Int32Helper.Parse(dataRow["Range"]));

                    transactions.Add(newTransaction);
                }
                transactions = transactions.OrderByDescending(transaction => transaction.Date).ToList();
            }

            return transactions;
        }

        /// <summary>Modifies an existing Transaction.</summary>
        /// <param name="oldTransaction">Existing Transaction</param>
        /// <param name="newTransaction">New Transaction</param>
        /// <returns>Returns true if modification successful</returns>
        public Task<bool> ModifyTransaction(Transaction oldTransaction, Transaction newTransaction)
        {
            throw new NotImplementedException();
        }

        /// <summary>Adds a new Transaction to the database.</summary>
        /// <param name="newTransaction">Transaction to be added</param>
        /// <returns>Returns true if add successful</returns>
        public async Task<bool> NewTransaction(Transaction newTransaction)
        {
            SQLiteCommand cmd = _con.CreateCommand();
            cmd.CommandText = $"INSERT INTO Transactions([VehicleID],[Store],[Date],[Octane],[Distance],[Gallons],[Price],[Odometer],[Range])Values('{newTransaction.VehicleID}','{newTransaction.Store}','{newTransaction.DateToString}','{newTransaction.Octane}','{newTransaction.Distance}','{newTransaction.Gallons}','{newTransaction.Price}','{newTransaction.Odometer}','{newTransaction.Range}')";

            return await ExecuteCommand(cmd);
        }

        #endregion Transaction Management

        #region Vehicle Management

        /// <summary>Gets the next VehicleID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next VehicleID value</returns>
        public async Task<int> GetNextVehicleIndex()
        {
            DataSet ds = await FillDataSet("SELECT * FROM SQLITE_SEQUENCE WHERE name = 'Vehicles'", "Seq");

            if (ds.Tables[0].Rows.Count > 0)
                return Int32Helper.Parse(ds.Tables[0].Rows[0]["seq"]) + 1;
            return 1;
        }

        /// <summary>Deletes a Vehicle and all associated Transactions from the database.</summary>
        /// <param name="deleteVehicle">Vehicle to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        public async Task<bool> DeleteVehicle(Vehicle deleteVehicle)
        {
            SQLiteCommand cmd = _con.CreateCommand();
            cmd.CommandText = "DELETE FROM Vehicles WHERE [VehicleID] = @vehicleID; DELETE FROM Transactions WHERE [VehicleID] = @vehicleID";
            cmd.Parameters.AddWithValue("@vehicleID", deleteVehicle.VehicleID);

            return await ExecuteCommand(cmd);
        }

        /// <summary>Loads all Vehicles associated with a User.</summary>
        /// <param name="userID">User ID</param>
        /// <returns></returns>
        public async Task<List<Vehicle>> LoadVehicles(int userID)
        {
            DataSet ds = await FillDataSet($"SELECT * FROM Vehicles WHERE UserID='{userID}'", "Transactions");
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
            SQLiteCommand cmd = _con.CreateCommand();
            cmd.CommandText = $"INSERT INTO Vehicles([UserID],[Nickname],[Make],[Model],[Year])Values('{newVehicle.UserID}','{newVehicle.Nickname}','{newVehicle.Make}','{newVehicle.Model}',{newVehicle.Year})";

            return await ExecuteCommand(cmd);
        }

        #endregion Vehicle Management
    }
}