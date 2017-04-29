using Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FuelTracker
{
    /// <summary>Represents the current state of the application.</summary>
    internal static class AppState
    {
        private static readonly SQLiteDatabaseInteraction DatabaseInteraction = new SQLiteDatabaseInteraction();
        internal static User CurrentUser;

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        internal static void VerifyDatabaseIntegrity()
        {
            DatabaseInteraction.VerifyDatabaseIntegrity();
        }

        #region User Management

        /// <summary>Gets the next UserID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next UserID value</returns>
        public static async Task<int> GetNextUserIndex()
        {
            return await DatabaseInteraction.GetNextUserIndex();
        }

        /// <summary>Removes a User and all associated Vehicles and Transactions from the database.</summary>
        /// <param name="deleteUser">User to be deleted</param>
        /// <returns>True if successful</returns>
        public static async Task<bool> DeleteUser(User deleteUser)
        {
            return await DatabaseInteraction.DeleteUser(deleteUser);
        }

        /// <summary>Loads a User and all associated Vehicles and their Transactions from the database.</summary>
        /// <param name="username">User to be loaded</param>
        /// <returns>Requested User</returns>
        public static async Task<User> LoadUser(string username)
        {
            return await DatabaseInteraction.LoadUser(username);
        }

        /// <summary>Logs in a User.</summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plaintext password</param>
        /// <returns>True is successful authentication</returns>
        public static async Task<bool> Login(string username, string password)
        {
            bool success = false;
            if (await DatabaseInteraction.AuthenticateUser(username, password))
            {
                CurrentUser = await DatabaseInteraction.LoadUser(username);
                success = true;
            }
            else
                DisplayNotification("Unable to validate login.", "Fuel Tracker");

            return success;
        }

        /// <summary>Saves a new user to the database.</summary>
        /// <param name="username">Username for new User</param>
        /// <param name="password">Password for new User</param>
        /// <returns>Returns true if User successfully added to database.</returns>
        public static async Task<bool> NewUser(string username, string password)
        {
            return await DatabaseInteraction.NewUser(username, password);
        }

        #endregion User Management

        #region Transaction Management

        /// <summary>Gets the next TransactionID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next TransactionID value</returns>
        public static async Task<int> GetNextTransactionIndex()
        {
            return await DatabaseInteraction.GetNextTransactionIndex();
        }

        /// <summary>Deletes a Transaction from the database.</summary>
        /// <param name="deleteTransaction">Transaction to be deleted</param>
        /// <returns>Returns true if deletion successful</returns>
        public static async Task<bool> DeleteTransaction(Transaction deleteTransaction)
        {
            return await DatabaseInteraction.DeleteTransaction(deleteTransaction);
        }

        /// <summary>Loads all Transactions associated with a specific Vehicle.</summary>
        /// <param name="vehicleID">Vehicle ID</param>
        /// <returns>Returns all Transactions associated with a specific Vehicle.</returns>
        public static async Task<List<Transaction>> LoadTransactions(int vehicleID)
        {
            return await DatabaseInteraction.LoadTransactions(vehicleID);
        }

        /// <summary>Modifies an existing Transaction.</summary>
        /// <param name="oldTransaction">Existing Transaction</param>
        /// <param name="newTransaction">New Transaction</param>
        /// <returns>Returns true if modification successful</returns>
        public static async Task<bool> ModifyTransaction(Transaction oldTransaction, Transaction newTransaction)
        {
            return await DatabaseInteraction.ModifyTransaction(oldTransaction, newTransaction);
        }

        /// <summary>Adds a new Transaction to the database.</summary>
        /// <param name="newTransaction">Transaction to be added</param>
        /// <returns>Returns true if add successful</returns>
        public static async Task<bool> NewTransaction(Transaction newTransaction)
        {
            return await DatabaseInteraction.NewTransaction(newTransaction);
        }

        #endregion Transaction Management

        #region Vehicle Management

        /// <summary>Gets the next VehicleID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next VehicleID value</returns>
        public static async Task<int> GetNextVehicleIndex()
        {
            return await DatabaseInteraction.GetNextVehicleIndex();
        }

        /// <summary>Deletes a Vehicle and all associated Transactions from the database.</summary>
        /// <param name="deleteVehicle">Vehicle to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        public static async Task<bool> DeleteVehicle(Vehicle deleteVehicle)
        {
            return await DatabaseInteraction.DeleteVehicle(deleteVehicle);
        }

        /// <summary>Loads all Vehicles associated with a User.</summary>
        /// <param name="userID">User ID</param>
        /// <returns></returns>
        public static async Task<List<Vehicle>> LoadVehicles(int userID)
        {
            return await DatabaseInteraction.LoadVehicles(userID);
        }

        /// <summary>Adds a new Vehicle to the database.</summary>
        /// <param name="newVehicle">Vehicle to be added</param>
        /// <returns>Returns whether the Vehicle was successfully added</returns>
        public static async Task<bool> NewVehicle(Vehicle newVehicle)
        {
            return await DatabaseInteraction.NewVehicle(newVehicle);
        }

        #endregion Vehicle Management

        #region Notification Management

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="buttons">Button type to be displayed on the Window</param>
        internal static void DisplayNotification(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK).ShowDialog();
            });
        }

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="buttons">Button type to be displayed on the Window</param>
        /// <param name="window">Window being referenced</param>
        internal static void DisplayNotification(string message, string title, Window window)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK, window).ShowDialog();
            });
        }

        internal static bool YesNoNotification(string message, string title, Window window)
        {
            bool result = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (new Notification(message, title, NotificationButtons.YesNo, window).ShowDialog() == true)
                    result = true;
            });
            return result;
        }

        #endregion Notification Management
    }
}