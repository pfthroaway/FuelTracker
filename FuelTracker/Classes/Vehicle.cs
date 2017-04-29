using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FuelTracker
{
    /// <summary>Represents a Vehicle owned by a User.</summary>
    public class Vehicle : IEnumerable<Transaction>, IEquatable<Vehicle>, INotifyPropertyChanged
    {
        private string _nickname, _make, _model;
        private int _vehicleID, _userID, _year;
        private List<Transaction> _transactions;

        #region Modifying Properties

        /// <summary>Vehicle ID</summary>
        public int VehicleID
        {
            get => _vehicleID;
            set { _vehicleID = value; OnPropertyChanged("VehicleID"); }
        }

        /// <summary>User ID</summary>
        public int UserID
        {
            get => _userID;
            set { _userID = value; OnPropertyChanged("UserID"); }
        }

        /// <summary>Vehicle nickname</summary>
        public string Nickname
        {
            get => _nickname;
            set { _nickname = value; OnPropertyChanged("Nickname"); }
        }

        /// <summary>Brand of car</summary>
        public string Make
        {
            get => _make;
            set { _make = value; OnPropertyChanged("Make"); }
        }

        /// <summary>Model of car</summary>
        public string Model
        {
            get => _model;
            set { _model = value; OnPropertyChanged("Model"); }
        }

        /// <summary>Model year of car</summary>
        public int Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged("Year"); }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>List of Transactions associated with the current Vehicle.</summary>
        public ReadOnlyCollection<Transaction> Transactions => new ReadOnlyCollection<Transaction>(_transactions);

        public decimal MPG
        {
            get
            {
                decimal i = 0;
                foreach (Transaction trans in Transactions)
                    i += trans.MPG;
                return i;
            }
        }

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Transaction Management

        /// <summary>Adds a Transaction to the list of Transactions.</summary>
        /// <param name="transaction">Transaction to be removed</param>
        internal void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
            _transactions = _transactions.OrderBy(trans => trans.Date).ToList();
        }

        /// <summary>Removes a Transaction from the list of Transactions.</summary>
        /// <param name="transaction">Transaction to be removed</param>
        internal void RemoveTransaction(Transaction transaction)
        {
            _transactions.Remove(transaction);
        }

        #endregion Transaction Management

        #region Enumerators

        public IEnumerator<Transaction> GetEnumerator()
        {
            return Transactions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Enumerators

        #region Override Operators

        private static bool Equals(Vehicle left, Vehicle right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.VehicleID == right.VehicleID && left.UserID == right.UserID && string.Equals(left.Nickname, right.Nickname, StringComparison.OrdinalIgnoreCase) && string.Equals(left.Make, right.Make, StringComparison.OrdinalIgnoreCase) && string.Equals(left.Model, right.Model, StringComparison.OrdinalIgnoreCase) && left.Year == right.Year && left.Transactions == right.Transactions;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj as Vehicle);
        }

        public bool Equals(Vehicle otherVehicle)
        {
            return Equals(this, otherVehicle);
        }

        public static bool operator ==(Vehicle left, Vehicle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Vehicle left, Vehicle right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ 17;
        }

        public override string ToString()
        {
            return $"{UserID} - {Nickname}";
        }

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Vehicle.</summary>
        public Vehicle()
        {
        }

        /// <summary>Initializes an instance of Vehicle by assigning Property values.</summary>
        /// <param name="vehicleID"></param>
        /// <param name="userID"></param>
        /// <param name="nickname"></param>
        /// <param name="make"></param>
        /// <param name="model"></param>
        /// <param name="year"></param>
        /// <param name="transactions"></param>
        public Vehicle(int vehicleID, int userID, string nickname, string make, string model, int year, IEnumerable<Transaction> transactions)
        {
            VehicleID = vehicleID;
            UserID = userID;
            Nickname = nickname;
            Make = make;
            Model = model;
            Year = year;

            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _transactions = newTransactions;
        }

        /// <summary>Replaces this instance of Vehicle with another instance.</summary>
        /// <param name="otherVehicle">Instance of Vehicle to replace this instance</param>
        public Vehicle(Vehicle otherVehicle)
        {
            VehicleID = otherVehicle.VehicleID;
            UserID = otherVehicle.UserID;
            Nickname = otherVehicle.Nickname;
            Make = otherVehicle.Make;
            Model = otherVehicle.Model;
            Year = otherVehicle.Year;
            _transactions = new List<Transaction>(otherVehicle.Transactions);
        }

        #endregion Constructors
    }
}