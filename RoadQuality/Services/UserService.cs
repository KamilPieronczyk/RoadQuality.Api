using MongoDB.Driver;
using RoadQuality.Configurations;
using RoadQuality.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoadQuality.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public User Get(string id) =>
            _users.Find<User>(user => user.Id == id).FirstOrDefault();

        public User Create(User user)
        {
            if(CheckIfUserWithEmailExists(user.Email))
            {
                return GetUserWithEmail(user.Email);
            }

            user.IsProfileSet = false;

            _users.InsertOne(user);
            return user;
        }

        public User GetUserWithEmail(string email)
        {
            return _users.Find<User>(user => user.Email == email).FirstOrDefault();
        }

        public bool CheckIfUserWithEmailExists(string email)
        {
            return _users.Find<User>(user => user.Email == email).CountDocuments() > 0;
        }

        public User UpdateUserProfile(User newUser)
        {
            var user = Get(newUser.Id);
            user.Name = newUser.Name;
            user.LastName = newUser.LastName;
            user.PhoneNumber = newUser.PhoneNumber;

            _users.ReplaceOne(u => u.Id == user.Id, user);
            return user;
        }
    }
}
