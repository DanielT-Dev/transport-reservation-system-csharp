namespace MyClientApp.Models;

public class User : IIdentifiable<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";

    public User() { }

    public User(int id, string name, string email, string password)
    {
        Id = id;
        Name = name;
        Email = email;
        Password = password;
    }

    public override string ToString()
    {
        return $"{Id} {Name} {Email} {Password}";
    }
}