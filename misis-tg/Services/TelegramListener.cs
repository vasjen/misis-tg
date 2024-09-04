using misis_tg.Extensions;
using misis_tg.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace misis_tg.Services;

public class TelegramListener(
    ITelegramBotClient client,
    ILogger<TelegramListener> logger,
    IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private readonly EnrolledService enrolledService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<EnrolledService>();
    private readonly NotificationService notificationService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<NotificationService>();
    private readonly SubscriptionService subscriptionService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<SubscriptionService>();
    private string? _currentContext = null;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

       client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );

       return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Telegram service stopped");
        return Task.CompletedTask;
    }
     public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            await HandleMessageAsync(botClient, update.Message, cancellationToken);
        }
    }


    public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.From != null)
        {
            logger.LogInformation($"Received message: {message.Text} from user: {message.From.Id} : {message.From.Username}");
        }

        switch (message.Text)
        {
            case "/start":
                ReplyKeyboardMarkup keyboard = MakeKeyboard([["Проверить статус"], ["Подписаться", "Отписаться"]]);
                await notificationService.NotifyAsync(message.Chat.Id, "Привет! Чем могу помочь?", replyMarkup: keyboard, cancellationToken: cancellationToken);
                _currentContext = null;
                break;
            case "Отписаться":
                if (!await subscriptionService.IsSubscribedAsync(message.Chat.Id))
                {
                    await notificationService.NotifyAsync(message.Chat.Id, "Вы не подписаны на уведомления!", cancellationToken: cancellationToken);
                }
                else
                {
                    await subscriptionService.UnsubscribeAsync(message.Chat.Id);
                    await notificationService.NotifyAsync(message.Chat.Id, "Вы успешно отписаны!", cancellationToken: cancellationToken);
                }
                break;
            case "Подписаться":
                await notificationService.NotifyAsync(message.Chat.Id,
                    $"Введите регистрационный номер для отслеживания! Он состоит из 7 цифр.", cancellationToken: cancellationToken);
                _currentContext = "subscribe";
                break;
            case "Проверить статус":
                await notificationService.NotifyAsync(message.Chat.Id,
                    $"Введите регистрационный номер для отслеживания! Он состоит из 7 цифр.", cancellationToken: cancellationToken);
                _currentContext = "check";
                break;
            default:
                if (_currentContext == "subscribe")
                {
                    if (int.TryParse(message.Text, out int regNumber))
                    {
                        if (await subscriptionService.IsSubscribedAsync(message.Chat.Id))
                        {
                            await notificationService.NotifyAsync(message.Chat.Id, "Вы уже подписаны на уведомления", cancellationToken: cancellationToken);
                            return;
                        }
                        await subscriptionService.SubscribeAsync(message.Chat.Id, regNumber);
                        await notificationService.NotifyAsync(message.Chat.Id, "Вы подписаны на уведомления", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await notificationService.NotifyAsync(message.Chat.Id, "Неверный формат регистрационного номера", cancellationToken: cancellationToken);
                    }
                }
                else if (_currentContext == "check")
                {
                    if (int.TryParse(message.Text, out int regNumber))
                    {
                        EstimationResponseDto? result = await enrolledService.CheckStudentAsync(regNumber);
                        if ( result is null)
                        {
                            await notificationService.NotifyAsync(message.Chat.Id, "Вы не зачислены!", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await notificationService.NotifyAsync(message.Chat.Id, $"Поздравляю с зачислением! \n{result.FormatEstimationResponse()}", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await notificationService.NotifyAsync(message.Chat.Id, "Неверный формат регистрационного номера", cancellationToken: cancellationToken);
                    }
                }
                break;
        }
    }


    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        logger.LogError(ErrorMessage);
        return Task.CompletedTask;
    }
    private ReplyKeyboardMarkup MakeKeyboard(string[][] buttons)
    {
        KeyboardButton[][] keyboardButtons = buttons.Select(row => row.Select(text => new KeyboardButton(text)).ToArray()).ToArray();
        return new ReplyKeyboardMarkup(keyboardButtons){ResizeKeyboard = true};
    }
    
    
  
    
    
}