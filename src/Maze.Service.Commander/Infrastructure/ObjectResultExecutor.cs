// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Maze.Modules.Api;
using Maze.Modules.Api.Formatters;
using Maze.Modules.Api.Response;
using Maze.Modules.Api.Services;
using Maze.Service.Commander.Commanding.Formatters.Abstractions;
using Maze.Service.Commander.Logging;

namespace Maze.Service.Commander.Infrastructure
{
    /// <summary>
    ///     Executes an <see cref="ObjectResult" /> to write to the response.
    /// </summary>
    public class ObjectResultExecutor : IActionResultExecutor<ObjectResult>
    {
        /// <summary>
        ///     Creates a new <see cref="ObjectResultExecutor" />.
        /// </summary>
        /// <param name="formatterSelector">The <see cref="OutputFormatterSelector" />.</param>
        /// <param name="writerFactory">The <see cref="IHttpResponseStreamWriterFactory" />.</param>
        /// <param name="logger">The <see cref="ILogger"/></param>
        public ObjectResultExecutor(OutputFormatterSelector formatterSelector,
            IHttpResponseStreamWriterFactory writerFactory, ILogger<ObjectResultExecutor> logger)
        {
            if (writerFactory == null) throw new ArgumentNullException(nameof(writerFactory));

            FormatterSelector = formatterSelector ?? throw new ArgumentNullException(nameof(formatterSelector));
            WriterFactory = writerFactory.CreateWriter;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Gets the <see cref="ILogger" />.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        ///     Gets the <see cref="OutputFormatterSelector" />.
        /// </summary>
        protected OutputFormatterSelector FormatterSelector { get; }

        /// <summary>
        ///     Gets the writer factory delegate.
        /// </summary>
        protected Func<Stream, Encoding, TextWriter> WriterFactory { get; }

        /// <summary>
        ///     Executes the <see cref="ObjectResult" />.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext" /> for the current request.</param>
        /// <param name="result">The <see cref="ObjectResult" />.</param>
        /// <returns>
        ///     A <see cref="Task" /> which will complete once the <see cref="ObjectResult" /> is written to the response.
        /// </returns>
        public virtual Task ExecuteAsync(ActionContext context, ObjectResult result)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (result == null) throw new ArgumentNullException(nameof(result));

            // If the user sets the content type both on the ObjectResult (example: by Produces) and Response object,
            // then the one set on ObjectResult takes precedence over the Response object
            if (result.ContentTypes == null || result.ContentTypes.Count == 0)
            {
                var responseContentType = context.Context.Response.ContentType;
                if (!string.IsNullOrEmpty(responseContentType))
                {
                    if (result.ContentTypes == null) result.ContentTypes = new MediaTypeCollection();
                    result.ContentTypes.Add(responseContentType);
                }
            }

            var objectType = result.DeclaredType;
            if (objectType == null || objectType == typeof(object))
                objectType = result.Value?.GetType();

            var formatterContext = new OutputFormatterWriteContext(
                context.Context,
                WriterFactory,
                objectType,
                result.Value);

            var selectedFormatter = FormatterSelector.SelectFormatter(formatterContext,
                result.Formatters ?? Array.Empty<IOutputFormatter>(), result.ContentTypes);

            if (selectedFormatter == null)
            {
                // No formatter supports this.
                Logger.NoFormatter(formatterContext);

                context.Context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return Task.CompletedTask;
            }

            Logger.ObjectResultExecuting(result.Value);
            
            return selectedFormatter.WriteAsync(formatterContext);
        }
    }
}