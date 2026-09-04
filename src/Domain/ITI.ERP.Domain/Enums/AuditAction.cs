namespace ITI.ERP.Domain.Enums
{
    public enum AuditAction
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        Login = 3,
        Logout = 4,
        PasswordChange = 5,
        PasswordReset = 6,
        Unlock = 7,
        AttendanceMark = 8,
        PracticalCreate = 9,
        DraftSave = 10,
        DraftSubmit = 11,
        DraftLock = 12,
        DraftUnlock = 13,
        DraftFinalize = 14,
        DocumentVerify = 15,
        Export = 16,
        Archive = 17,
        Restore = 18,
        RetentionCleanup = 19,
        PasswordResetRequested = 20,
        PasswordResetCompleted = 21
    }
}
