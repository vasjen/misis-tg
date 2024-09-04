using Microsoft.EntityFrameworkCore;
using misis_tg.Data;
using misis_tg.Models;

namespace misis_tg.Services;

public class SubscriptionService(AppDbContext db)
{
    public async Task SubscribeAsync(long chatId, int registrationNumber)
    {
        if (await db.Subscribers.AnyAsync(s => s.ChatId == chatId))
        {
            throw new Exception("Already subscribed");
        }
        await db.Subscribers.AddAsync(new Subscriber
        {
            ChatId = chatId,
            RegistrationNumber = registrationNumber
        });
        await db.SaveChangesAsync();
    }
    public async Task MakeSubscriberNotifiedAsync(Subscriber subscriber)
    {
        subscriber.IsNotified = true;
        await db.SaveChangesAsync();
    }
    
    public async Task UnsubscribeAsync(long chatId)
    {
        var subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.ChatId == chatId);
        if (subscriber == null)
        {
            throw new Exception("Not subscribed");
        }
        db.Subscribers.Remove(subscriber);
        await db.SaveChangesAsync();
    }
    public async Task<bool> IsSubscribedAsync(long chatId)
        => await db.Subscribers.AnyAsync(s => s.ChatId == chatId);
    
    public async Task<List<Subscriber>> GetAllActiveSubscribers()
        => await  db.Subscribers.Where(p => !p.IsNotified).ToListAsync();
}