// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Maze.Modules.Api.Formatters;

namespace Maze.Service.Commander.Commanding.Formatters
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter : IOutputFormatter
    {
        /// <summary>
        /// Gets the mutable collection of media type elements supported by
        /// this <see cref="OutputFormatter"/>.
        /// </summary>
        public MediaTypeCollection SupportedMediaTypes { get; } = new MediaTypeCollection();

        /// <summary>
        /// Returns a value indicating whether or not the given type can be written by this serializer.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <returns><c>true</c> if the type can be written, otherwise <c>false</c>.</returns>
        protected virtual bool CanWriteType(Type type)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<string> GetSupportedContentTypes(
            string contentType,
            Type objectType)
        {
            if (SupportedMediaTypes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No media types found in '{GetType().FullName}.{nameof(SupportedMediaTypes)}'. Add at least one media type to the list of supported media types.");
            }

            if (!CanWriteType(objectType))
            {
                return null;
            }

            List<string> mediaTypes = null;

            var parsedContentType = contentType != null ? new MediaType(contentType) : default(MediaType);

            foreach (var mediaType in SupportedMediaTypes)
            {
                var parsedMediaType = new MediaType(mediaType);
                if (parsedMediaType.HasWildcard)
                {
                    // For supported media types that are wildcard patterns, confirm that the requested
                    // media type satisfies the wildcard pattern (e.g., if "text/entity+json;v=2" requested
                    // and formatter supports "text/*+json").
                    // Treat contentType like it came from a [Produces] attribute.
                    if (contentType != null && parsedContentType.IsSubsetOf(parsedMediaType))
                    {
                        if (mediaTypes == null)
                        {
                            mediaTypes = new List<string>();
                        }

                        mediaTypes.Add(contentType);
                    }
                }
                else
                {
                    // Confirm this formatter supports a more specific media type than requested e.g. OK if "text/*"
                    // requested and formatter supports "text/plain". Treat contentType like it came from an Accept header.
                    if (contentType == null || parsedMediaType.IsSubsetOf(parsedContentType))
                    {
                        if (mediaTypes == null)
                        {
                            mediaTypes = new List<string>();
                        }

                        mediaTypes.Add(mediaType);
                    }
                }
            }

            return mediaTypes;
        }

        /// <inheritdoc />
        public virtual bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (SupportedMediaTypes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No media types found in '{GetType().FullName}.{nameof(SupportedMediaTypes)}'. Add at least one media type to the list of supported media types.");
            }

            if (!CanWriteType(context.ObjectType))
            {
                return false;
            }

            if (!context.ContentType.HasValue)
            {
                // If the desired content type is set to null, then the current formatter can write anything
                // it wants.
                context.ContentType = new StringSegment(SupportedMediaTypes[0]);
                return true;
            }
            else
            {
                var parsedContentType = new MediaType(context.ContentType);
                for (var i = 0; i < SupportedMediaTypes.Count; i++)
                {
                    var supportedMediaType = new MediaType(SupportedMediaTypes[i]);
                    if (supportedMediaType.HasWildcard)
                    {
                        // For supported media types that are wildcard patterns, confirm that the requested
                        // media type satisfies the wildcard pattern (e.g., if "text/entity+json;v=2" requested
                        // and formatter supports "text/*+json").
                        // We only do this when comparing against server-defined content types (e.g., those
                        // from [Produces] or Response.ContentType), otherwise we'd potentially be reflecting
                        // back arbitrary Accept header values.
                        if (context.ContentTypeIsServerDefined
                            && parsedContentType.IsSubsetOf(supportedMediaType))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // For supported media types that are not wildcard patterns, confirm that this formatter
                        // supports a more specific media type than requested e.g. OK if "text/*" requested and
                        // formatter supports "text/plain".
                        // contentType is typically what we got in an Accept header.
                        if (supportedMediaType.IsSubsetOf(parsedContentType))
                        {
                            context.ContentType = new StringSegment(SupportedMediaTypes[i]);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public virtual Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            WriteResponseHeaders(context);
            return WriteResponseBodyAsync(context);
        }

        /// <summary>
        /// Sets the headers on <see cref="Microsoft.AspNetCore.Http.HttpResponse"/> object.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        public virtual void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.MazeContext.Response;
            response.ContentType = context.ContentType.Value;
        }

        /// <summary>
        /// Writes the response body.
        /// </summary>
        /// <param name="context">The formatter context associated with the call.</param>
        /// <returns>A task which can write the response body.</returns>
        public abstract Task WriteResponseBodyAsync(OutputFormatterWriteContext context);
    }
}
