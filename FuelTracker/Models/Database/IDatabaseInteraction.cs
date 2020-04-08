using FuelTracker.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelTracker.Models.Database
{
    /// <summary>Represents required interactions for implementations of databases.</summary>
    public interface IDatabaseInteraction
    {
        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        void VerifyDatabaseIntegrity();

        #region Transaction Management

        /// <summary>Deletes a Transaction from the database.</summary>
        /// <param name="deleteTransaction">Transaction to be deleted</param>
        /// <returns>Returns true if deletion successful</returns>
        Task<bool> DeleteTransaction(FuelTransaction deleteTransaction);

        /// <summary>Gets the next TransactionID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next TransactionID value</returns>
        Task<int> GetNextTransactionIndex();

        /// <summary>Loads all Transactions associated with a specific Vehicle.</summary>
        /// <param name="vehicleID">Vehicle ID</param>
        /// <returns>Returns all Transactions associated with a specific Vehicle.</returns>
        Task<List<FuelTransaction>> LoadTransactions(int vehicleID);

        /// <summary>Modifies an existing Transaction.</summary>
        /// <param name="oldTransaction">Existing Transaction</param>
        /// <param name="newTransaction">New Transaction</param>
        /// <returns>Returns true if modification successful</returns>
        Task<bool> ModifyTransaction(FuelTransaction oldTransaction, FuelTransaction newTransaction);

        /// <summary>Adds a new Transaction to the database.</summary>
        /// <param name="newTransaction">Transaction to be added</param>
        /// <returns>Returns true if add successful</returns>
        Task<bool> NewTransaction(FuelTransaction newTransaction);

        #endregion Transaction Management

        #region Vehicle Management

        /// <summary>Deletes a Vehicle and all associated Transactions from the database.</summary>
        /// <param name="deleteVehicle">Vehicle to be deleted</param>
        /// <returns>Returns true if deletion is successful.</returns>
        Task<bool> DeleteVehicle(Vehicle deleteVehicle);

        /// <summary>Gets the next VehicleID autoincrement value in the database for the Vehicle table.</summary>
        /// <returns>Next VehicleID value</returns>
        Task<int> GetNextVehicleIndex();

        /// <summary>Loads all Vehicles.</summary>
        /// <returns>All Vehicles</returns>
        Task<List<Vehicle>> LoadVehicles();

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