﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.CosmosElements;
    using Microsoft.Azure.Cosmos.Tracing;

    internal sealed class FeedIteratorInlineCore<T> : FeedIteratorInternal<T>
    {
        private readonly FeedIteratorInternal<T> feedIteratorInternal;
        private readonly CosmosClientContext clientContext;

        internal FeedIteratorInlineCore(
            FeedIterator<T> feedIterator,
            CosmosClientContext clientContext)
        {
            if (!(feedIterator is FeedIteratorInternal<T> feedIteratorInternal))
            {
                throw new ArgumentNullException(nameof(feedIterator));
            }

            this.feedIteratorInternal = feedIteratorInternal;
            this.clientContext = clientContext;

            this.container = feedIteratorInternal.container;
            this.databaseName = feedIteratorInternal.databaseName;
        }

        internal FeedIteratorInlineCore(
            FeedIteratorInternal<T> feedIteratorInternal,
            CosmosClientContext clientContext)
        {
            this.feedIteratorInternal = feedIteratorInternal ?? throw new ArgumentNullException(nameof(feedIteratorInternal));
            this.clientContext = clientContext;

            this.container = feedIteratorInternal.container;
            this.databaseName = feedIteratorInternal.databaseName;
        }

        public override bool HasMoreResults => this.feedIteratorInternal.HasMoreResults;

        public override Task<FeedResponse<T>> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            return this.clientContext.OperationHelperAsync("Typed FeedIterator ReadNextAsync",
                        requestOptions: null,
                        task: trace => this.feedIteratorInternal.ReadNextAsync(trace, cancellationToken),
                        openTelemetry: (response) =>
                        {
                            if (this.container == null)
                            {
                                return new OpenTelemetryResponse<T>(
                                    responseMessage: response, 
                                    containerName: null,
                                    databaseName: this.databaseName);
                            }
                            return new OpenTelemetryResponse<T>(
                                    responseMessage: response, 
                                    containerName: this.container?.Id,
                                    databaseName: this.container?.Database?.Id ?? this.databaseName);
                        });
        }

        public override Task<FeedResponse<T>> ReadNextAsync(ITrace trace, CancellationToken cancellationToken)
        {
            return TaskHelper.RunInlineIfNeededAsync(() => this.feedIteratorInternal.ReadNextAsync(trace, cancellationToken));
        }

        public override CosmosElement GetCosmosElementContinuationToken()
        {
            return this.feedIteratorInternal.GetCosmosElementContinuationToken();
        }

        protected override void Dispose(bool disposing)
        {
            this.feedIteratorInternal.Dispose();
            base.Dispose(disposing);
        }
    }
}
