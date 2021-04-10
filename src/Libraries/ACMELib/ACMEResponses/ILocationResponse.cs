namespace Kenc.ACMELib.ACMEResponses
{
    using System;

    /// <summary>
    /// Interface used to describe responses that contains a location property.
    /// </summary>
    public interface ILocationResponse
    {
        Uri Location { get; set; }
    }
}