using LiteralLifeChurch.LiveStreamingApi.Controllers;
using LiteralLifeChurch.LiveStreamingApi.Enums;
using LiteralLifeChurch.LiveStreamingApi.Exceptions;
using LiteralLifeChurch.LiveStreamingApi.Models.Bootstrapping;
using LiteralLifeChurch.LiveStreamingApi.Models.Input;
using LiteralLifeChurch.LiveStreamingApi.Models.Output;
using LiteralLifeChurch.LiveStreamingApi.Services;
using LiteralLifeChurch.LiveStreamingApi.Services.Common;
using LiteralLifeChurch.LiveStreamingApi.Services.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi
{
    public static class Stop
    {
        [Function("Stop")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "broadcaster")] HttpRequestData request,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Stop");

            using (LoggerService.Init(logger))
            {
                ConfigurationModel config = ConfigurationService.GetConfiguration();

                try
                {
                    AzureMediaServicesClient client = await AuthenticationService.GetClientAsync(config);

                    InputRequestService inputRequestService = new(client, config);
                    StopController stopController = new(client, config);

                    InputRequestModel inputModel = await inputRequestService.GetInputRequestModelAsync(request);
                    StatusChangeOutputModel outputModel = await stopController.StopServicesAsync(inputModel);

                    await WebhookService.CallWebhookAsync(config.WebhookStartSuccess, ActionEnum.Stop, outputModel.Status.Summary.Name);
                    return await SuccessResponseService.CreateResponse(request, outputModel);
                }
                catch (AppException e)
                {
                    return await ReportErrorAsync(request, config, e);
                }
                catch (Exception e)
                {
                    return await ReportErrorAsync(request, config, e);
                }
            }
        }

        private static async Task<HttpResponseData> ReportErrorAsync(HttpRequestData request, ConfigurationModel config, Exception exception)
        {
            LoggerService.CaptureException(exception);
            await WebhookService.CallWebhookAsync(config.WebhookStartFailure, ActionEnum.Start, ResourceStatusEnum.Error);
            return await ErrorResponseService.CreateResponse(request, exception);
        }
    }
}
