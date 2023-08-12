using WebServer;

namespace Main
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public ItemCount[] Inventory { get; set; }

        /// <summary>
        /// When provided with only a username, attempts to load the user from its file.
        /// </summary>
        /// <param name="username"></param>
        public static User? MakeUserFromUserName(string username)
        {
            return Helpers.ReadJSONFileToObject<User>($"Users/{username}.json");
        }

        public static User MakeNewUser(string username, string password)
        {
            User user = new() { Username = username, Password = password, Inventory = InventoryHelpers.DefaultInventory };
            
            // Write user to file.
            WriteToFile(user);

            return user;
        }

        public bool WriteToFile() => WriteToFile(this);

        public static bool WriteToFile(User user) {
            Console.WriteLine($"User: {user.Username}, {user.Password}");
            return Helpers.WriteObjecToJSON($"Users/{user.Username}.json", user);
        }
    }
}