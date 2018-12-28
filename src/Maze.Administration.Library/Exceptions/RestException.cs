using System;
using Maze.Server.Connection;

namespace Maze.Administration.Library.Exceptions
{
    public abstract class RestException : Exception
    {
        protected RestException(RestError error) : base(error.Message)
        {
            ErrorId = error.Code;
        }

        public int ErrorId { get; }
    }
}