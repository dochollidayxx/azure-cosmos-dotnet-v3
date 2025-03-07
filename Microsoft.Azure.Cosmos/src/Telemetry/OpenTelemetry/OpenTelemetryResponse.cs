﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System.IO;
    using System.Net;
    using Telemetry;

    internal sealed class OpenTelemetryResponse : OpenTelemetryAttributes
    {
        internal OpenTelemetryResponse(TransactionalBatchResponse responseMessage, string containerName = null, string databaseName = null)
           : this(
                  statusCode: responseMessage.StatusCode,
                  requestCharge: responseMessage.Headers?.RequestCharge,
                  responseContentLength: null,
                  diagnostics: responseMessage.Diagnostics,
                  itemCount: responseMessage.Headers?.ItemCount,
                  databaseName: databaseName,
                  containerName: containerName,
                  requestMessage: null)
        {
        }

        internal OpenTelemetryResponse(ResponseMessage responseMessage, string containerName = null, string databaseName = null)
           : this(
                  statusCode: responseMessage.StatusCode,
                  requestCharge: responseMessage.Headers?.RequestCharge,
                  responseContentLength: OpenTelemetryResponse.GetPayloadSize(responseMessage),
                  diagnostics: responseMessage.Diagnostics,
                  itemCount: responseMessage.Headers?.ItemCount,
                  databaseName: databaseName,
                  containerName: containerName,
                  requestMessage: responseMessage.RequestMessage)
        {
        }

        private OpenTelemetryResponse(
            HttpStatusCode statusCode, 
            double? requestCharge,
            string responseContentLength,
            CosmosDiagnostics diagnostics,
            string itemCount,
            string databaseName,
            string containerName,
            RequestMessage requestMessage)
            : base(requestMessage, containerName, databaseName)
        {
            this.StatusCode = statusCode;
            this.RequestCharge = requestCharge;
            this.ResponseContentLength = responseContentLength ?? OpenTelemetryAttributes.NotAvailable;
            this.Diagnostics = diagnostics;
            this.ItemCount = itemCount ?? OpenTelemetryAttributes.NotAvailable;
        }

        private static string GetPayloadSize(ResponseMessage response)
        {
            if (response?.Content != null
                    && response.Content.CanSeek
                    && response.Content is MemoryStream)
            {
                return response.Content.Length.ToString();
            }
            return response?.Headers?.ContentLength ?? OpenTelemetryAttributes.NotAvailable;
        }
    }
}
