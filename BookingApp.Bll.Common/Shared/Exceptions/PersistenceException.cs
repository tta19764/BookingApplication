namespace BookingApp.Bll.Common.Shared.Exceptions;

/// <summary>Classifies infrastructure failures without exposing database provider types.</summary>
public enum PersistenceError { Timeout, Unavailable, AccessDenied, ConstraintViolation, SchemaMismatch, Conflict, Failure }

/// <summary>A sanitized database failure. Provider diagnostics are logged inside DAL, not attached as an inner exception.</summary>
public sealed class PersistenceException : Exception
{
    /// <summary>Gets the provider-independent failure category.</summary>
    public PersistenceError Error { get; }
    /// <summary>Gets the logical persistence operation that failed.</summary>
    public string Operation { get; }
    /// <summary>Gets the identifier correlating this failure with its DAL log entry.</summary>
    public Guid IncidentId { get; }
    /// <summary>Creates a sanitized persistence failure.</summary>
    public PersistenceException(PersistenceError error, string operation, Guid incidentId)
        : base($"Persistence operation '{operation}' failed ({error}). Incident: {incidentId}.")
    { Error = error; Operation = operation; IncidentId = incidentId; }
}
