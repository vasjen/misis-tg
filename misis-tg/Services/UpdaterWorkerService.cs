using misis_tg.Extensions;
using misis_tg.Models;

namespace misis_tg.Services;

public class UpdaterWorkerService(IServiceScopeFactory scopeFactory) : BackgroundWorkerService
{
    protected override int ExecutionInterval { get; } = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        NotificationService notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
        EnrolledService enrolledService = scope.ServiceProvider.GetRequiredService<EnrolledService>();
        SubscriptionService subscriptionService = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
        ILogger<UpdaterWorkerService> logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdaterWorkerService>>();
        
        await enrolledService.UpdateEnrolledAsync();
        List<Subscriber> subscribers = await subscriptionService.GetAllActiveSubscribers();
        foreach (Subscriber subscriber in subscribers)
        {
            EstimationResponseDto? result = await enrolledService.CheckStudentAsync(subscriber.RegistrationNumber);
            if (result == null)
            {
                continue;
            }
            await notificationService.NotifyAsync(subscriber.ChatId, $"Поздравляю с зачислением! \n{result.FormatEstimationResponse()}", cancellationToken: cancellationToken);
            await subscriptionService.MakeSubscriberNotifiedAsync(subscriber);
            logger.LogInformation($"Сообщение отправлено пользователю {subscriber.ChatId} с кодом {subscriber.RegistrationNumber}");
        }
        logger.LogInformation($"Парсинг завершен! {DateTime.UtcNow}");
    }
}