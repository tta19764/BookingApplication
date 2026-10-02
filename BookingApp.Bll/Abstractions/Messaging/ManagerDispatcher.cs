using System.Collections;
using System.Reflection;
using BookingApp.Bll.Exceptions;
using BookingApp.Bll.Common.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BookingApp.Bll.Abstractions.Messaging;

internal sealed class ManagerDispatcher(
    IServiceProvider serviceProvider,
    ILogger<ManagerDispatcher> logger) : IManagerDispatcher
{
    public async Task<TResponse> Send<TResponse>(
        IManagerRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var requestName = requestType.Name;

        try
        {
            logger.LogInformation("Executing request {Request}", requestName);
            await ValidateAsync(request, requestType, cancellationToken);

            var handlerType = typeof(IRequestManager<,>)
                .MakeGenericType(requestType, typeof(TResponse));
            var handler = serviceProvider.GetRequiredService(handlerType);
            var handleMethod = handlerType.GetMethod("Handle")!;
            var response = await (Task<TResponse>)handleMethod.Invoke(
                handler,
                [request, cancellationToken])!;

            LogResult(requestName, response);
            return response;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            logger.LogError(exception.InnerException, "Request {Request} processing failed", requestName);
            throw exception.InnerException;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {Request} processing failed", requestName);
            throw;
        }
    }

    private async Task ValidateAsync<TResponse>(
        IManagerRequest<TResponse> request,
        Type requestType,
        CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validatorsType = typeof(IEnumerable<>).MakeGenericType(validatorType);
        var validators = (IEnumerable)serviceProvider.GetRequiredService(validatorsType);
        var context = new ValidationContext<object>(request);
        var errors = new List<ValidationError>();

        foreach (IValidator validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            errors.AddRange(result.Errors.Select(error =>
                new ValidationError(error.PropertyName, error.ErrorMessage)));
        }

        if (errors.Count > 0)
        {
            throw new Exceptions.ValidationException(errors);
        }
    }

    private void LogResult<TResponse>(string requestName, TResponse response)
    {
        if (response is not Result result || result.IsSuccess)
        {
            logger.LogInformation("Request {Request} processed successfully", requestName);
            return;
        }

        using (LogContext.PushProperty("Error", result.Error, true))
        {
            logger.LogError("Request {Request} processed with error", requestName);
        }
    }
}
