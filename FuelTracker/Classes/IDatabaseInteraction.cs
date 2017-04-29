using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FuelTracker
{
    public interface IDatabaseInteraction
    {
        void VerifyDatabaseIntegrity();

        Task<DataSet> FillDataSet(string sql, string tableName);

        #region User Management

        Task<int> GetNextUserIndex();

        Task<bool> AuthenticateUser(string username, string password);

        Task<bool> DeleteUser(User deleteUser);

        Task<User> LoadUser(string username);

        Task<bool> NewUser(string username, string password);

        #endregion User Management

        #region Transaction Management

        Task<int> GetNextTransactionIndex();

        Task<bool> DeleteTransaction(Transaction deleteTransaction);

        Task<List<Transaction>> LoadTransactions(int vehicleID);

        Task<bool> ModifyTransaction(Transaction oldTransaction, Transaction newTransaction);

        Task<bool> NewTransaction(Transaction newTransaction);

        #endregion Transaction Management

        #region Vehicle Management

        Task<int> GetNextVehicleIndex();

        Task<bool> DeleteVehicle(Vehicle deleteVehicle);

        Task<List<Vehicle>> LoadVehicles(int userID);

        Task<bool> NewVehicle(Vehicle newVehicle);

        #endregion Vehicle Management
    }
}