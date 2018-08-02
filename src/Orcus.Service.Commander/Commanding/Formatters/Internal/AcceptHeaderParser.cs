﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Orcus.Service.Commander.Commanding.Formatters.Internal
{
    public static class AcceptHeaderParser
    {
        public static IList<MediaTypeSegmentWithQuality> ParseAcceptHeader(IList<string> acceptHeaders)
        {
            var parsedValues = new List<MediaTypeSegmentWithQuality>();
            ParseAcceptHeader(acceptHeaders, parsedValues);

            return parsedValues;
        }

        public static void ParseAcceptHeader(IList<string> acceptHeaders, IList<MediaTypeSegmentWithQuality> parsedValues)
        {
            if (acceptHeaders == null)
            {
                throw new ArgumentNullException(nameof(acceptHeaders));
            }

            if (parsedValues == null)
            {
                throw new ArgumentNullException(nameof(parsedValues));
            }
            for (var i = 0; i < acceptHeaders.Count; i++)
            {
                var charIndex = 0;
                var value = acceptHeaders[i];

                while (!string.IsNullOrEmpty(value) && charIndex < value.Length)
                {
                    var startCharIndex = charIndex;

                    if (TryParseValue(value, ref charIndex, out var output))
                    {
                        // The entry may not contain an actual value, like Accept: application/json, , */*
                        if (output.MediaType.HasValue)
                        {
                            parsedValues.Add(output);
                        }
                    }

                    if (charIndex <= startCharIndex)
                    {
                        Debug.Fail("ParseAcceptHeader should advance charIndex, this is a bug.");
                        break;
                    }
                }
            }
        }

        private static bool TryParseValue(string value, ref int index, out MediaTypeSegmentWithQuality parsedValue)
        {
            parsedValue = default(MediaTypeSegmentWithQuality);

            // The accept header may be added multiple times to the request/response message. E.g.
            // Accept: text/xml; q=1
            // Accept:
            // Accept: text/plain; q=0.2
            // In this case, do not fail parsing in case one of the values is the empty string.
            if (string.IsNullOrEmpty(value) || (index == value.Length))
            {
                return true;
            }

            var currentIndex = GetNextNonEmptyOrWhitespaceIndex(value, index, out var separatorFound);

            if (currentIndex == value.Length)
            {
                index = currentIndex;
                return true;
            }

            // We deliberately want to ignore media types that we are not capable of parsing.
            // This is due to the fact that some browsers will send invalid media types like 
            // ; q=0.9 or */;q=0.2, etc.
            // In this scenario, our recovery action consists of advancing the pointer to the
            // next separator and moving on.
            // In case we don't find the next separator, we simply advance the cursor to the
            // end of the string to signal that we are done parsing.
            var result = default(MediaTypeSegmentWithQuality);
            var length = 0;
            try
            {
                length = GetMediaTypeWithQualityLength(value, currentIndex, out result);
            }
            catch
            {
                length = 0;
            }

            if (length == 0)
            {
                // The parsing failed.
                currentIndex = value.IndexOf(',', currentIndex);
                if (currentIndex == -1)
                {
                    index = value.Length;
                    return false;
                }
                index = currentIndex;
                return false;
            }

            currentIndex = currentIndex + length;
            currentIndex = GetNextNonEmptyOrWhitespaceIndex(value, currentIndex, out separatorFound);

            // If we've not reached the end of the string, then we must have a separator.
            // E. g application/json, text/plain <- We must be at ',' otherwise, we've failed parsing.
            if (!separatorFound && (currentIndex < value.Length))
            {
                index = currentIndex;
                return false;
            }

            index = currentIndex;
            parsedValue = result;
            return true;
        }

        private static int GetNextNonEmptyOrWhitespaceIndex(
            string input,
            int startIndex,
            out bool separatorFound)
        {
            Debug.Assert(input != null);
            Debug.Assert(startIndex <= input.Length); // it's OK if index == value.Length.

            separatorFound = false;
            var current = startIndex + HttpTokenParsingRules.GetWhitespaceLength(input, startIndex);

            if ((current == input.Length) || (input[current] != ','))
            {
                return current;
            }

            // If we have a separator, skip the separator and all following whitespaces, and
            // continue until the current character is neither a separator nor a whitespace.
            separatorFound = true;
            current++; // skip delimiter.
            current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);

            while ((current < input.Length) && (input[current] == ','))
            {
                current++; // skip delimiter.
                current = current + HttpTokenParsingRules.GetWhitespaceLength(input, current);
            }

            return current;
        }

        private static int GetMediaTypeWithQualityLength(
            string input,
            int start,
            out MediaTypeSegmentWithQuality result)
        {
            result = MediaType.CreateMediaTypeSegmentWithQuality(input, start);
            if (result.MediaType.HasValue)
            {
                return result.MediaType.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}
