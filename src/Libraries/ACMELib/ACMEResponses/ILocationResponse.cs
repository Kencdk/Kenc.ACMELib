using System;

namespace Kenc.ACMELib.ACMEResponses
{
    /// <summary>
    /// Interface used to describe responses that contains a location property.
    /// </summary>
    public interface ILocationResponse
    {
        Uri Location { get; set; }
    }
}