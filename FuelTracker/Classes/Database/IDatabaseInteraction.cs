using FuelTracker.Classes.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelTracker.Classes.Database
{
    public interface IDatabaseInteraction
    {
        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        void VerifyDatabaseIntegrity();

        #region User Management

        /// <summary>Logs in a User.</summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plaintext password</param>
        /// <returns>True is successful authentication</returns>
        Task<bool> AuthenticateUser(string username, string password);

        /// <summary>Changes details in the database about a User.</summary>
        /// <param name="oldUser">Old User details</param>
        /// <param name="newUser">New User details</param>
        /// <returns>True if successful</returns>
        Task<bool> ChangeUserDetails(User oldUser, User newUser);

        /// <summary>Removes a User and all associated Vehicles and Transactions from the database.</summary>
        /// <param name="deleteUser">User to be deleted</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteUser(User deleteUser);

        /// <summary>Gets the next UserID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next UserID value</returns>
        Task<int> GetNextUserIndex();

        /// <summary>Loads a User and all associated Vehicles and their Transactions from the database.</summary>
        /// <param name="username">User to be loaded</param>
        /// <returns>Requested User</returns>
        Task<User> LoadUser(string username);

        /// <summary>Saves a new user to the database.</summary>
        /// <param name="username">Username for new User</param>
        /// <param name="password">Password for new User</param>
        /// <returns>Returns true if User successfully added to database.</returns>
        Task<bool> NewUser(string username, string password);

        #endregion User Management

        #region Transaction Management

        /// <summary>Deletes a Transaction from the database.</summary>
        /// <param name="deleteTransaction">Transaction to be deleted</param>
        /// <returns>Returns true if deletion successful</returns>
        Task<bool> DeleteTransaction(Transaction deleteTransaction);

        /// <summary>Gets the next TransactionID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next TransactionID value</returns>
        Task<int> GetNextTransactionIndex();

        /// <summary>Loads all Transactions associated with a specific Vehicle.</summary>
        /// <param name="vehicleID">Vehicle ID</param>
        /// <returns>Returns all Transactions associated with a specific Vehicle.</returns>
        Task<List<Transaction>> LoadTransactions(int vehicleID);

        /// <summary>Modifies an existing Transaction.</summary>
        /// <param name="oldTransaction">Existing Transaction</param>
        /// <param name="newTransaction">New Transaction</param>
        /// <returns>Returns true if modification successful</returns>
        Task<bool> ModifyTransaction(Transaction oldTransaction, Transaction newTransaction);

        /// <summary>Adds a new Transaction to the database.</summary>
        /// <param name="newTransaction">Transaction to be added</param>
        /// <returns>Returns true if add successful</returns>
        Task<bool> NewTransaction(Transaction newTransaction);

        #endregion Transaction Management

        #region Vehicle Management

        /// <summary>Deletes a Vehicle and all associated Transactions from the database.</summary>
        /// <param name="deleteVehicle">Vehicle to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        Task<bool> DeleteVehicle(Vehicle deleteVehicle);

        /// <summary>Gets the next VehicleID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next VehicleID value</returns>
        Task<int> GetNextVehicleIndex();

        /// <summary>Loads all Vehicles associated with a User.</summary>
        /// <param name="userID">User ID</param>
        /// <returns>User's Vehicles</returns>
        Task<List<Vehicle>> LoadVehicles(int userID);

        /// <summary>Changes details in the database regarding a Vehicle.</summary>
        /// <param name="oldVehicle">Old Vehicle details</param>
        /// <param name="newVehicle">New Vehicle details</param>
        /// <returns>Returns true if modification successful</returns>
        Task<bool> ModifyVehicle(Vehicle oldVehicle, Vehicle newVehicle);

        /// <summary>Adds a new Vehicle to the database.</summary>
        /// <param name="newVehicle">Vehicle to be added</param>
        /// <returns>Returns whether the Vehicle was successfully added</returns>
        Task<bool> NewVehicle(Vehicle newVehicle);

        #endregion Vehicle Management
    }
}