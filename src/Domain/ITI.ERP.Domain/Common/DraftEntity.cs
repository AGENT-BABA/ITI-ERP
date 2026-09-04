using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Common;

public abstract class DraftEntity : SoftDeletableEntity
{
    public DraftStatus DraftStatus { get; set; }

    public DateTime? DraftSavedAt { get; set; }
    public Guid? DraftSavedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }

    public DateTime? LockedAt { get; set; }
    public Guid? LockedBy { get; set; }

    public DateTime? UnlockedAt { get; set; }
    public Guid? UnlockedBy { get; set; }
    public string? UnlockReason { get; set; }

    public DateTime? FinalizedAt { get; set; }
    public Guid? FinalizedBy { get; set; }

    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    private static readonly Dictionary<DraftStatus, HashSet<DraftStatus>> AllowedTransitions = new()
    {
        [DraftStatus.Draft] = new() { DraftStatus.Submitted },
        [DraftStatus.Submitted] = new() { DraftStatus.Verified, DraftStatus.Draft },
        [DraftStatus.Verified] = new() { DraftStatus.Locked, DraftStatus.Submitted },
        [DraftStatus.Locked] = new() { DraftStatus.Finalized, DraftStatus.Unlocked },
        [DraftStatus.Unlocked] = new() { DraftStatus.Draft, DraftStatus.Submitted },
        [DraftStatus.Finalized] = new() { DraftStatus.Archived },
        [DraftStatus.Archived] = new(),
    };

    public bool CanTransitionTo(DraftStatus targetStatus)
    {
        return AllowedTransitions.TryGetValue(DraftStatus, out var allowed) && allowed.Contains(targetStatus);
    }

    public string? ValidateTransition(DraftStatus targetStatus, Guid userId, DateTime utcNow, string? reason = null)
    {
        if (!CanTransitionTo(targetStatus))
            return $"Cannot transition from {DraftStatus} to {targetStatus}.";

        DraftStatus = targetStatus;

        switch (targetStatus)
        {
            case DraftStatus.Submitted:
                SubmittedAt = utcNow;
                SubmittedBy = userId;
                break;
            case DraftStatus.Verified:
                VerifiedAt = utcNow;
                VerifiedBy = userId;
                break;
            case DraftStatus.Locked:
                LockedAt = utcNow;
                LockedBy = userId;
                break;
            case DraftStatus.Unlocked:
                UnlockedAt = utcNow;
                UnlockedBy = userId;
                UnlockReason = reason;
                break;
            case DraftStatus.Finalized:
                FinalizedAt = utcNow;
                FinalizedBy = userId;
                break;
            case DraftStatus.Archived:
                ArchivedAt = utcNow;
                ArchivedBy = userId;
                break;
        }

        return null;
    }
}
