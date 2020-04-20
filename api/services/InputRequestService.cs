﻿using LiteralLifeChurch.LiveStreamingApi.models.input;
using LiteralLifeChurch.LiveStreamingApi.services.validators;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services
{
    public class InputRequestService
    {
        private const string EndpointQuery = "endpoint";
        private const string EventsQuery = "events";
        private readonly InputValidator InputValidator;
        private readonly ServiceValidator ServiceValidator;

        public InputRequestService()
        {
            InputValidator = new InputValidator();
            ServiceValidator = new ServiceValidator();
        }

        public async Task<InputRequestModel> GetInputRequestModel(HttpRequest request)
        {
            InputValidator.Validate(request);

            InputRequestModel model = new InputRequestModel()
            {
                LiveEvents = request.Query[EventsQuery]
                    .ToString()
                    .Split(',')
                    .Select(eventName => eventName.Trim())
                    .Where(eventName => !string.IsNullOrEmpty(eventName))
                    .ToList(),

                StreamingEndpoint = request.Query[EndpointQuery]
                    .ToString()
                    .Trim()
            };

            await ServiceValidator.Validate(model);
            return model;
        }
    }
}
