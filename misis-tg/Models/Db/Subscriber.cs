namespace misis_tg.Models;

public class Subscriber
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? FirstName { get; set; }
    public int RegistrationNumber { get; set; }
    public bool IsNotified { get; set; }
}