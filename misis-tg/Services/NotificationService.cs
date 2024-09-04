using Microsoft.EntityFrameworkCore;
using misis_tg.Data;
using misis_tg.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace misis_tg.Services;

public class NotificationService(AppDbContext db, ILogger<NotificationService> logger, ITelegramBotClient botClient)
{
    public async Task NotifyAsync(long chatId, string message, ReplyKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }
        catch (ApiRequestException e)
        {
            if (e.ErrorCode == 403)
            {
                Subscriber? subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.ChatId == chatId, cancellationToken: cancellationToken);
                if (subscriber is not null)
                {
                    logger.LogWarning($"Chat with id {chatId} blocked the bot and will be unsubscribed forcefully");
                    db.Subscribers.Remove(subscriber);
                    await db.SaveChangesAsync(cancellationToken);
                }
                logger.LogError(e, $"Error while sending message to chat {chatId}.\n Details: {e.Message}");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error while sending message to chat {chatId}.\n Details: {e.Message}");
        }
    }
    
    
}