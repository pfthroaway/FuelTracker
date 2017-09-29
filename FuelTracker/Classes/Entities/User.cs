using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FuelTracker.Classes.Entities
{
    /// <summary>Represents a User who owns Vehicle(s).</summary>
    public class User : INotifyPropertyChanged, IEnumerable<Vehicle>, IEquatable<User>
    {
        private int _id;
        private string _username, _password;
        private List<Vehicle> _vehicles = new List<Vehicle>();

        #region Modifying Properties

        /// <summary>The User's ID.</summary>
        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>The User's login name.</summary>
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        /// <summary>The User's hashed password.</summary>
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        /// <summary>List of all Vehicles the User owns.</summary>
        internal ReadOnlyCollection<Vehicle> Vehicles => new ReadOnlyCollection<Vehicle>(_vehicles);

        #endregion Modifying Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Vehicle Management

        /// <summary>Adds a Vehicle to the list of vehicles.</summary>
        /// <param name="newVehicle">Vehicle to be removed</param>
        internal void AddVehicle(Vehicle newVehicle)
        {
            _vehicles.Add(newVehicle);
            _vehicles = _vehicles.OrderBy(vehicle => vehicle.Nickname).ToList();
            OnPropertyChanged("Vehicles");
        }

        /// <summary>Modifies a Vehicle in the list of Vehicles.</summary>
        /// <param name="oldVehicle">Original Vehicle to be replaced</param>
        /// <param name="newVehicle">New Vehicle to replace old</param>
        internal void ModifyVehicle(Vehicle oldVehicle, Vehicle newVehicle)
        {
            _vehicles.Replace(oldVehicle, newVehicle);
            _vehicles = _vehicles.OrderBy(trans => trans.Nickname).ToList();
            OnPropertyChanged("Vehicles");
        }

        /// <summary>Removes a Vehicle from the list of vehicles.</summary>
        /// <param name="vehicle">Vehicle to be removed</param>
        internal void RemoveVehicle(Vehicle vehicle)
        {
            _vehicles.Remove(vehicle);
            OnPropertyChanged("Vehicles");
        }

        #endregion Vehicle Management

        #region Enumerators

        public IEnumerator<Vehicle> GetEnumerator() => Vehicles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion Enumerators

        #region Override Operators

        private static bool Equals(User left, User right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.ID == right.ID && string.Equals(left.Username, right.Username, StringComparison.OrdinalIgnoreCase) && string.Equals(left.Password, right.Password, StringComparison.Ordinal) && left.Vehicles.Count == right.Vehicles.Count && !left.Vehicles.Except(right.Vehicles).Any();
        }

        public override bool Equals(object obj) => Equals(this, obj as User);

        public bool Equals(User otherUser) => Equals(this, otherUser);

        public static bool operator ==(User left, User right) => Equals(left, right);

        public static bool operator !=(User left, User right) => !Equals(left, right);

        public override int GetHashCode() => base.GetHashCode() ^ 17;

        public override string ToString() => $"{ID} - {Username}";

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of User.</summary>
        public User()
        {
        }

        /// <summary>Initializes an instance of User by assigning Property values.</summary>
        /// <param name="id">User's ID</param>
        /// <param name="username">User's login name</param>
        /// <param name="password">User's hashed password</param>
        /// <param name="vehicles">User's list of owned Vehicles</param>
        public User(int id, string username, string password, IEnumerable<Vehicle> vehicles)
        {
            ID = id;
            Username = username;
            Password = password;
            List<Vehicle> newVehicles = new List<Vehicle>();
            newVehicles.AddRange(vehicles);
            _vehicles = newVehicles;
        }

        /// <summary>Replaces this instance of User with another instance.</summary>
        /// <param name="other">Instance of User to replace this instance</param>
        public User(User other) : this(other.ID, other.Username, other.Password, other.Vehicles)
        {
        }

        #endregion Constructors
    }
}