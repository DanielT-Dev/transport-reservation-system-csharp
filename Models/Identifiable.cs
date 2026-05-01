namespace MyClientApp.Models;

public interface IIdentifiable<TId>
{
    TId Id { get; set; }
}