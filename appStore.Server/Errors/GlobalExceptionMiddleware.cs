using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace appStore.Server.Errors
{
	public class GlobalExceptionMiddleware(
		RequestDelegate next,
		ILogger<GlobalExceptionMiddleware> logger )
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};


		public async Task InvokeAsync( HttpContext context )
		{
			try
			{
				await next( context );
			}
			catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
			{
				if (logger.IsEnabled( LogLevel.Information ))
				{
					logger.LogInformation(
						"Request was canceled by the client. TraceId: {TraceId}",
						context.TraceIdentifier
					);
				}
			}
			catch (Exception exception)
			{
				await HandleExceptionAsync( context, exception );
			}
		}

		private async Task HandleExceptionAsync( HttpContext context, Exception exception )
		{
			if (context.Response.HasStarted)
			{
				if (logger.IsEnabled( LogLevel.Error ))
				{
					logger.LogError(
						exception,
						"An exception occurred after the response had already started. TraceId: {TraceId}",
						context.TraceIdentifier );
				}

				throw exception;
			}

			var response = CreateErrorResponse( context, exception );

			LogException( context, exception, response );

			context.Response.Clear();
			context.Response.StatusCode = response.Status;
			context.Response.ContentType = "application/json";

			await context.Response.WriteAsync( JsonSerializer.Serialize( response, JsonOptions ) );
		}

		private static ErrorResponse CreateErrorResponse( HttpContext context, Exception exception )
		{
			return exception switch
			{
				AppException appException =>
					new ErrorResponse
					{
						Status = appException.StatusCode,
						Code = appException.Code,
						Message = appException.Message,
						TraceId = context.TraceIdentifier,
						Details = appException.Details
					},

				ValidationException validationException =>
					new ErrorResponse
					{
						Status = StatusCodes.Status400BadRequest,
						Code = ErrorCodes.VALIDATION_ERROR,
						Message = validationException.Message,
						TraceId = context.TraceIdentifier
					},

				ArgumentException argumentException =>
					new ErrorResponse
					{
						Status = StatusCodes.Status400BadRequest,
						Code = ErrorCodes.INVALID_REQUEST,
						Message = argumentException.Message,
						TraceId = context.TraceIdentifier
					},

				_ =>
					new ErrorResponse
					{
						Status = StatusCodes.Status500InternalServerError,
						Code = ErrorCodes.INTERNAL_ERROR,
						Message = "An unexpected error occurred.",
						TraceId = context.TraceIdentifier
					}
			};
		}

		private void LogException(
			HttpContext context,
			Exception exception,
			ErrorResponse response )
		{
			if (response.Status >= 500)
			{
				if (logger.IsEnabled( LogLevel.Error ))
				{
					logger.LogError(
						exception,
						"Unhandled server exception. Code: {Code}, TraceId: {TraceId}, Path: {Path}",
						response.Code,
						context.TraceIdentifier,
						context.Request.Path );
				}
			}
			else
			{
				if (logger.IsEnabled( LogLevel.Warning ))
				{
					logger.LogWarning(
						"Application error. Code: {Code}, Status: {Status}, TraceId: {TraceId}, Path: {Path}, Message: {Message}",
						response.Code,
						response.Status,
						context.TraceIdentifier,
						context.Request.Path,
						response.Message );
				}
			}
		}
	}
}