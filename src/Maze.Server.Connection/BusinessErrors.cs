using Maze.Server.Connection.Error;

namespace Maze.Server.Connection
{
    public static class BusinessErrors
    {
        public static RestError FieldNullOrEmpty(string name)
            => new RestError(ErrorTypes.ValidationError, $"The field {name} must not be null or empty.",
                (int) ErrorCode.FieldNullOrEmpty);

        public static RestError InvalidSha256Hash => CreateValidationError(
            "The value must be a valid SHA256 hash. A SHA256 hash consists of 64 hexadecimal characters.",
            ErrorCode.InvalidSha256Hash);

        public static RestError InvalidMacAddress => CreateValidationError(
            "The value must be a valid mac address. A mac address must consist of 6 bytes.",
            ErrorCode.InvalidSha256Hash);

        public static RestError InvalidCultureName => CreateValidationError(
            "The given culture is not supported.", ErrorCode.InvalidCultureName);

        public static class Account
        {
            public static RestError InvalidJwt =>
                CreateAuthenticationError("The JSON Web Token is invalid.", ErrorCode.Account_InvalidJwt);

            public static RestError UsernameNotFound =>
                CreateAuthenticationError("The username was not found.", ErrorCode.Account_UsernameNotFound);

            public static RestError InvalidPassword =>
                CreateAuthenticationError("The password is invalid for this account.",
                    ErrorCode.Account_InvalidPassword);

            public static RestError AccountDisabled =>
                CreateAuthenticationError(
                    "The account was disabled. If you think this is an error, please contact our support.",
                    ErrorCode.Account_Disabled);

            public static RestError UsernameTooLong =>
                CreateValidationError("The username is too long.", ErrorCode.Account_UsernameTooLong);

            public static RestError InvalidCharsInUsername =>
                CreateValidationError("The username contains invalid chars. Please note that it may only contain alphanumeric characters (a-z, 0-9)",
                    ErrorCode.Account_UsernameContainsInvalidChars);

            public static RestError UsernameAlreadyExists =>
                CreateValidationError("The username already exists.", ErrorCode.Account_UsernameAlreadyExists);

            public static RestError NotFound =>
                CreateAuthenticationError("The account was not found.", ErrorCode.Account_NotFound);

            private static RestError CreateAuthenticationError(string message, ErrorCode code)
            {
                return new RestError(ErrorTypes.AuthenticationError, message, (int) code);
            }
        }

        public static class Commander
        {
            public static RestError ClientNotFound =>
                CreateNotFoundError("The client was not found.", ErrorCode.Commander_ClientNotFound);

            public static RestError SingleCommandTargetRequired =>
                CreateValidationError("A single command target is required for this operation.",
                    ErrorCode.Commander_SingleCommandTargetRequired);

            public static RestError CommandTransmissionFailed =>
                CreateInvalidOperationError("The transmission of the command failed. Maybe the client disconnected.",
                    ErrorCode.Commander_CommandTransmissionFailed);

            public static RestError RouteNotFound(string path) =>
                CreateNotFoundError($"The route '{path}' was not found", ErrorCode.Commander_RouteNotFound);

            public static RestError ActionError(string exception, string methodName, string message) =>
                CreateNotFoundError($"An {exception} occurred on executing {methodName}: {message}",
                    ErrorCode.Commander_ActionError);

            public static RestError ResultExecutionError(string exception, string name, string message) =>
                CreateNotFoundError($"An {exception} occurred on executing result {name}: {message}",
                    ErrorCode.Commander_ResultError);
        }

        public static class ClientGroups
        {
            public static RestError GroupNotFound => CreateNotFoundError("The client group was not found.", ErrorCode.ClientGroups_NotFound);
        }

        public static class ClientConfigurations
        {
            public static RestError InvalidJson =>
                CreateValidationError("The json of the configuration is invalid.", ErrorCode.ClientConfigurations_InvalidJson);

            public static RestError CannotCreateGlobalConfig =>
                CreateValidationError("Cannot create a global config as it is created by the server itself and undeletable.",
                    ErrorCode.ClientConfigurations_CannotCreateGlobalConfig);

            public static RestError ConfigAlreadyExists =>
                CreateValidationError("The client configuration already exists.",
                    ErrorCode.ClientConfigurations_AlreadyExists);

            public static RestError NotFound =>
                CreateNotFoundError("The client configuration was not found.",
                    ErrorCode.ClientConfigurations_NotFound);
        }

        private static RestError CreateValidationError(string message, ErrorCode code) =>
            new RestError(ErrorTypes.ValidationError, message, (int) code);

        private static RestError CreateInvalidOperationError(string message, ErrorCode code) =>
            new RestError(ErrorTypes.InvalidOperationError, message, (int) code);

        private static RestError CreateNotFoundError(string message, ErrorCode code) =>
            new RestError(ErrorTypes.NotFoundError, message, (int) code);
    }
}