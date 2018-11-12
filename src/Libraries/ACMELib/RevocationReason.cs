namespace Kenc.ACMELib
{
    /// <summary>
    /// Valid revocation reasons.
    /// </summary>
    public enum RevocationReason
    {
        Unspecified = 0,
        KeyCompromise = 1,
        CACompromise = 2,
        AffiliationChanged = 3,
        Superseded = 4,
        cessationOfOperation = 5,
        CertificateHold = 6,
        // 7 unused
        removeFromCRL = 8,
        PriviledgeWithdrawn = 9,
        AACompromise = 10
    }
}