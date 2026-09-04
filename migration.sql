CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "Institutes" (
        "Id" uuid NOT NULL,
        "GRNumber" character varying(10) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Address" text,
        "City" text,
        "State" text,
        "Phone" text,
        "Email" text,
        "LogoPath" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        CONSTRAINT "PK_Institutes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Module" text NOT NULL,
        "Action" text NOT NULL,
        "Description" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "AcademicSessions" (
        "Id" uuid NOT NULL,
        "SessionYear" character varying(9) NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsLocked" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AcademicSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AcademicSessions_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "InstituteId" uuid,
        "Name" character varying(50) NOT NULL,
        "Description" text,
        "RoleType" integer NOT NULL,
        "IsSystemRole" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Roles_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Username" character varying(50) NOT NULL,
        "Email" text,
        "PasswordHash" character varying(200) NOT NULL,
        "FirstName" text NOT NULL,
        "LastName" text,
        "Phone" text,
        "ProfileImagePath" text,
        "IsActive" boolean NOT NULL,
        "IsLocked" boolean NOT NULL,
        "LockedUntil" timestamp with time zone,
        "FailedLoginAttempts" integer NOT NULL,
        "LastLoginAt" timestamp with time zone,
        "AcademicSessionId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Users_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id"),
        CONSTRAINT "FK_Users_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "RolePermissions" (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "PermissionId" uuid NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "UserName" text,
        "Action" integer NOT NULL,
        "EntityName" character varying(100) NOT NULL,
        "EntityId" uuid,
        "OldValues" text,
        "NewValues" text,
        "IpAddress" text,
        "UserAgent" text,
        "AdditionalData" text,
        "Timestamp" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(200) NOT NULL,
        "JwtId" character varying(100) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "ReplacedByToken" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "Trades" (
        "Id" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Code" character varying(20) NOT NULL,
        "DurationInMonths" integer NOT NULL,
        "TotalSeats" integer NOT NULL,
        "HeadUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DraftStatus" integer NOT NULL,
        "DraftSavedAt" timestamp with time zone,
        "DraftSavedBy" uuid,
        "SubmittedAt" timestamp with time zone,
        "SubmittedBy" uuid,
        "VerifiedAt" timestamp with time zone,
        "VerifiedBy" uuid,
        "LockedAt" timestamp with time zone,
        "LockedBy" uuid,
        "UnlockedAt" timestamp with time zone,
        "UnlockedBy" uuid,
        "UnlockReason" text,
        "FinalizedAt" timestamp with time zone,
        "FinalizedBy" uuid,
        "ArchivedAt" timestamp with time zone,
        "ArchivedBy" uuid,
        CONSTRAINT "PK_Trades" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Trades_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Trades_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Trades_Users_HeadUserId" FOREIGN KEY ("HeadUserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE TABLE "UserRoles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "InstituteId" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "TradeId" uuid,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserRoles_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id"),
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_AcademicSessions_InstituteId_SessionYear" ON "AcademicSessions" ("InstituteId", "SessionYear");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_InstituteId_Timestamp" ON "AuditLogs" ("InstituteId", "Timestamp");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Institutes_GRNumber" ON "Institutes" ("GRNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_RolePermissions_RoleId" ON "RolePermissions" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Roles_InstituteId_Name" ON "Roles" ("InstituteId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_Trades_AcademicSessionId" ON "Trades" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_Trades_HeadUserId" ON "Trades" ("HeadUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Trades_InstituteId_AcademicSessionId_Code" ON "Trades" ("InstituteId", "AcademicSessionId", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_AcademicSessionId" ON "UserRoles" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_InstituteId" ON "UserRoles" ("InstituteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_TradeId" ON "UserRoles" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_UserId" ON "UserRoles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE INDEX "IX_Users_AcademicSessionId" ON "Users" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_InstituteId_Username" ON "Users" ("InstituteId", "Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730132601_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730132601_InitialCreate', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    CREATE TABLE "Students" (
        "Id" uuid NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "MiddleName" character varying(100),
        "LastName" character varying(100) NOT NULL,
        "DateOfBirth" timestamp with time zone NOT NULL,
        "Gender" integer NOT NULL,
        "BloodGroup" character varying(10),
        "Phone" character varying(15),
        "Email" character varying(200),
        "Address" character varying(500),
        "City" character varying(100),
        "State" character varying(100),
        "PinCode" character varying(10),
        "FatherName" character varying(200),
        "MotherName" character varying(200),
        "GuardianPhone" character varying(15),
        "GuardianRelation" character varying(50),
        "AcademicSessionId" uuid NOT NULL,
        "TradeId" uuid NOT NULL,
        "RollNumber" character varying(20) NOT NULL,
        "AdmissionNumber" character varying(20) NOT NULL,
        "AdmissionDate" timestamp with time zone NOT NULL,
        "AnnualIncome" numeric(12,2) NOT NULL,
        "CasteCategory" character varying(50),
        "IsPhysicallyHandicapped" boolean NOT NULL,
        "PreviousSchool" character varying(200),
        "PreviousQualification" character varying(100),
        "PreviousPercentage" numeric(5,2),
        "Status" integer NOT NULL,
        "StatusReason" character varying(500),
        "StatusChangedAt" timestamp with time zone,
        "StatusChangedBy" uuid,
        "AadharNumber" character varying(12),
        "PhotoPath" character varying(500),
        "EmergencyContactName" character varying(200),
        "EmergencyContactPhone" character varying(15),
        "EmergencyContactRelation" character varying(50),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DraftStatus" integer NOT NULL,
        "DraftSavedAt" timestamp with time zone,
        "DraftSavedBy" uuid,
        "SubmittedAt" timestamp with time zone,
        "SubmittedBy" uuid,
        "VerifiedAt" timestamp with time zone,
        "VerifiedBy" uuid,
        "LockedAt" timestamp with time zone,
        "LockedBy" uuid,
        "UnlockedAt" timestamp with time zone,
        "UnlockedBy" uuid,
        "UnlockReason" text,
        "FinalizedAt" timestamp with time zone,
        "FinalizedBy" uuid,
        "ArchivedAt" timestamp with time zone,
        "ArchivedBy" uuid,
        CONSTRAINT "PK_Students" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Students_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Students_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Students_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    CREATE INDEX "IX_Students_AcademicSessionId" ON "Students" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    CREATE UNIQUE INDEX "IX_Students_InstituteId_AcademicSessionId_AdmissionNumber" ON "Students" ("InstituteId", "AcademicSessionId", "AdmissionNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    CREATE UNIQUE INDEX "IX_Students_InstituteId_AcademicSessionId_RollNumber" ON "Students" ("InstituteId", "AcademicSessionId", "RollNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    CREATE INDEX "IX_Students_TradeId" ON "Students" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730150819_AddStudentEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730150819_AddStudentEntity', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE TABLE "AttendanceRecords" (
        "Id" uuid NOT NULL,
        "InstituteId" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "TradeId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "Date" timestamp with time zone NOT NULL,
        "Status" integer NOT NULL,
        "Remarks" character varying(500),
        "MarkedBy" uuid NOT NULL,
        "IsLocked" boolean NOT NULL,
        "LockedAt" timestamp with time zone,
        "LockedBy" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AttendanceRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AttendanceRecords_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AttendanceRecords_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AttendanceRecords_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AttendanceRecords_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AttendanceRecords_Users_MarkedBy" FOREIGN KEY ("MarkedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE INDEX "IX_AttendanceRecords_AcademicSessionId" ON "AttendanceRecords" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE UNIQUE INDEX "IX_AttendanceRecords_InstituteId_AcademicSessionId_StudentId_D~" ON "AttendanceRecords" ("InstituteId", "AcademicSessionId", "StudentId", "Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE INDEX "IX_AttendanceRecords_InstituteId_AcademicSessionId_TradeId_Date" ON "AttendanceRecords" ("InstituteId", "AcademicSessionId", "TradeId", "Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE INDEX "IX_AttendanceRecords_MarkedBy" ON "AttendanceRecords" ("MarkedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE INDEX "IX_AttendanceRecords_StudentId" ON "AttendanceRecords" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    CREATE INDEX "IX_AttendanceRecords_TradeId" ON "AttendanceRecords" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730152655_AddAttendanceEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730152655_AddAttendanceEntity', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE TABLE "MonthlyPracticals" (
        "Id" uuid NOT NULL,
        "InstituteId" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "TradeId" uuid NOT NULL,
        "Month" integer NOT NULL,
        "Year" integer NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "TotalMarks" integer NOT NULL,
        "PassMarks" integer NOT NULL,
        "IsLocked" boolean NOT NULL,
        "LockedAt" timestamp with time zone,
        "LockedBy" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_MonthlyPracticals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MonthlyPracticals_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_MonthlyPracticals_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_MonthlyPracticals_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_MonthlyPracticals_Users_LockedBy" FOREIGN KEY ("LockedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE TABLE "PracticalMarks" (
        "Id" uuid NOT NULL,
        "MonthlyPracticalId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "MarksObtained" numeric(8,2) NOT NULL,
        "Remarks" character varying(500),
        "MarkedBy" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        CONSTRAINT "PK_PracticalMarks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PracticalMarks_MonthlyPracticals_MonthlyPracticalId" FOREIGN KEY ("MonthlyPracticalId") REFERENCES "MonthlyPracticals" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PracticalMarks_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PracticalMarks_Users_MarkedBy" FOREIGN KEY ("MarkedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE INDEX "IX_MonthlyPracticals_AcademicSessionId" ON "MonthlyPracticals" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE UNIQUE INDEX "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~" ON "MonthlyPracticals" ("InstituteId", "AcademicSessionId", "TradeId", "Month", "Year");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE INDEX "IX_MonthlyPracticals_LockedBy" ON "MonthlyPracticals" ("LockedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE INDEX "IX_MonthlyPracticals_TradeId" ON "MonthlyPracticals" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE INDEX "IX_PracticalMarks_MarkedBy" ON "PracticalMarks" ("MarkedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE UNIQUE INDEX "IX_PracticalMarks_MonthlyPracticalId_StudentId" ON "PracticalMarks" ("MonthlyPracticalId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    CREATE INDEX "IX_PracticalMarks_StudentId" ON "PracticalMarks" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730155437_AddMonthlyPracticalEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730155437_AddMonthlyPracticalEntities', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE TABLE "YearlyPracticals" (
        "Id" uuid NOT NULL,
        "InstituteId" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "TradeId" uuid NOT NULL,
        "Year" integer NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(1000),
        "TotalMarks" integer NOT NULL,
        "PassMarks" integer NOT NULL,
        "IsLocked" boolean NOT NULL,
        "LockedAt" timestamp with time zone,
        "LockedBy" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_YearlyPracticals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_YearlyPracticals_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_YearlyPracticals_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_YearlyPracticals_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_YearlyPracticals_Users_LockedBy" FOREIGN KEY ("LockedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE TABLE "YearlyPracticalMarks" (
        "Id" uuid NOT NULL,
        "YearlyPracticalId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "MarksObtained" numeric(8,2) NOT NULL,
        "Remarks" character varying(500),
        "MarkedBy" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        CONSTRAINT "PK_YearlyPracticalMarks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_YearlyPracticalMarks_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_YearlyPracticalMarks_Users_MarkedBy" FOREIGN KEY ("MarkedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_YearlyPracticalMarks_YearlyPracticals_YearlyPracticalId" FOREIGN KEY ("YearlyPracticalId") REFERENCES "YearlyPracticals" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE INDEX "IX_YearlyPracticalMarks_MarkedBy" ON "YearlyPracticalMarks" ("MarkedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE INDEX "IX_YearlyPracticalMarks_StudentId" ON "YearlyPracticalMarks" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE UNIQUE INDEX "IX_YearlyPracticalMarks_YearlyPracticalId_StudentId" ON "YearlyPracticalMarks" ("YearlyPracticalId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE INDEX "IX_YearlyPracticals_AcademicSessionId" ON "YearlyPracticals" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE UNIQUE INDEX "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Year" ON "YearlyPracticals" ("InstituteId", "AcademicSessionId", "TradeId", "Year");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE INDEX "IX_YearlyPracticals_LockedBy" ON "YearlyPracticals" ("LockedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    CREATE INDEX "IX_YearlyPracticals_TradeId" ON "YearlyPracticals" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730160344_AddYearlyPracticalEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730160344_AddYearlyPracticalEntities', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731064226_AddInstituteSettingsEntity') THEN
    CREATE TABLE "InstituteSettings" (
        "Id" uuid NOT NULL,
        "InstituteId" uuid NOT NULL,
        "AcademicYear" character varying(20),
        "AttendanceThresholdPercentage" integer NOT NULL,
        "PassMarksPercentage" integer NOT NULL,
        "MaxGraceMarks" integer NOT NULL,
        "AutoLockAttendanceAfterDays" boolean NOT NULL,
        "AttendanceLockDays" integer NOT NULL,
        "AutoLockPracticalAfterDays" boolean NOT NULL,
        "PracticalLockDays" integer NOT NULL,
        "AuditLogRetentionDays" integer NOT NULL,
        "EnableNotifications" boolean NOT NULL,
        "NotificationEmail" character varying(200),
        "AcademicSessionFormat" character varying(20),
        "MaxStudentsPerBatch" integer NOT NULL,
        "LogoPath" character varying(500),
        "Address" character varying(500),
        "City" character varying(100),
        "State" character varying(100),
        "Phone" character varying(20),
        "Email" character varying(200),
        "Website" character varying(200),
        "PrincipalName" character varying(200),
        "AffiliationNumber" character varying(100),
        "RecognitionNumber" character varying(100),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        CONSTRAINT "PK_InstituteSettings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InstituteSettings_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731064226_AddInstituteSettingsEntity') THEN
    CREATE UNIQUE INDEX "IX_InstituteSettings_InstituteId" ON "InstituteSettings" ("InstituteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731064226_AddInstituteSettingsEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731064226_AddInstituteSettingsEntity', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" DROP CONSTRAINT "FK_Trades_AcademicSessions_AcademicSessionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" DROP CONSTRAINT "FK_Trades_Institutes_InstituteId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" DROP CONSTRAINT "FK_UserRoles_AcademicSessions_AcademicSessionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" DROP CONSTRAINT "FK_UserRoles_Institutes_InstituteId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" DROP CONSTRAINT "FK_UserRoles_Trades_TradeId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    DROP INDEX "IX_UserRoles_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    DROP INDEX "IX_RolePermissions_RoleId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD "AcademicSessionId1" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD "InstituteId1" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Permissions" ALTER COLUMN "Module" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Permissions" ALTER COLUMN "Description" TYPE character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Permissions" ALTER COLUMN "Code" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Permissions" ALTER COLUMN "Action" TYPE character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    CREATE UNIQUE INDEX "IX_UserRoles_UserId_RoleId_AcademicSessionId" ON "UserRoles" ("UserId", "RoleId", "AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    CREATE INDEX "IX_Trades_AcademicSessionId1" ON "Trades" ("AcademicSessionId1");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    CREATE INDEX "IX_Trades_InstituteId1" ON "Trades" ("InstituteId1");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    CREATE UNIQUE INDEX "IX_RolePermissions_RoleId_PermissionId" ON "RolePermissions" ("RoleId", "PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    CREATE UNIQUE INDEX "IX_Permissions_Code" ON "Permissions" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD CONSTRAINT "FK_Trades_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD CONSTRAINT "FK_Trades_AcademicSessions_AcademicSessionId1" FOREIGN KEY ("AcademicSessionId1") REFERENCES "AcademicSessions" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD CONSTRAINT "FK_Trades_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "Trades" ADD CONSTRAINT "FK_Trades_Institutes_InstituteId1" FOREIGN KEY ("InstituteId1") REFERENCES "Institutes" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    ALTER TABLE "UserRoles" ADD CONSTRAINT "FK_UserRoles_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731124156_FinalStabilization') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731124156_FinalStabilization', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807155342_AddTradeImportHistory') THEN
    CREATE TABLE "TradeImportHistories" (
        "Id" uuid NOT NULL,
        "FileName" character varying(255) NOT NULL,
        "ImportDate" timestamp with time zone NOT NULL,
        "NumberOfTradesImported" integer NOT NULL,
        "Status" integer NOT NULL,
        "ValidationErrors" character varying(4000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        CONSTRAINT "PK_TradeImportHistories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TradeImportHistories_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807155342_AddTradeImportHistory') THEN
    CREATE INDEX "IX_TradeImportHistories_InstituteId" ON "TradeImportHistories" ("InstituteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807155342_AddTradeImportHistory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260807155342_AddTradeImportHistory', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    DELETE FROM "PracticalMarks";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    DROP INDEX "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" DROP COLUMN "MarksObtained";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" DROP COLUMN "PassMarks";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" DROP COLUMN "TotalMarks";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "ApplicationKnowledge" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "AttendancePunctuality" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "FollowInstructions" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "QualityWorkmanship" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "SafetyConsciousness" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "SignedByTrainee" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "SkillsToolsEquipment" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "SpeedDoingWork" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "Viva" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "PracticalMarks" ADD "WorkplaceHygiene" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" ADD "AssessorName" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" ADD "EndDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" ADD "LearningOutcome" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" ADD "ProfessionalSkillName" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    ALTER TABLE "MonthlyPracticals" ADD "StartDate" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    CREATE UNIQUE INDEX "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~" ON "MonthlyPracticals" ("InstituteId", "AcademicSessionId", "TradeId", "Month", "Year", "ProfessionalSkillName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260808153156_NSQFJobEvaluationSheet') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260808153156_NSQFJobEvaluationSheet', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809084203_AddYearlyPracticalMonthlyEntries') THEN
    ALTER TABLE "YearlyPracticalMarks" ADD "AnnualRemark" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809084203_AddYearlyPracticalMonthlyEntries') THEN
    ALTER TABLE "YearlyPracticalMarks" ADD "AnnualTotal" numeric(10,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809084203_AddYearlyPracticalMonthlyEntries') THEN
    ALTER TABLE "YearlyPracticalMarks" ADD "MonthlyManualEntries" character varying(4000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809084203_AddYearlyPracticalMonthlyEntries') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260809084203_AddYearlyPracticalMonthlyEntries', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809132856_AddHolidayEntity') THEN
    CREATE TABLE "Holidays" (
        "Id" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "Date" timestamp with time zone NOT NULL,
        "Name" character varying(200),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Holidays" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Holidays_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809132856_AddHolidayEntity') THEN
    CREATE INDEX "IX_Holidays_AcademicSessionId" ON "Holidays" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809132856_AddHolidayEntity') THEN
    CREATE UNIQUE INDEX "IX_Holidays_InstituteId_AcademicSessionId_Date" ON "Holidays" ("InstituteId", "AcademicSessionId", "Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809132856_AddHolidayEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260809132856_AddHolidayEntity', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809162402_FixHolidayUniqueIndex') THEN
    DROP INDEX "IX_Holidays_InstituteId_AcademicSessionId_Date";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809162402_FixHolidayUniqueIndex') THEN
    CREATE UNIQUE INDEX "IX_Holidays_InstituteId_AcademicSessionId_Date" ON "Holidays" ("InstituteId", "AcademicSessionId", "Date") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260809162402_FixHolidayUniqueIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260809162402_FixHolidayUniqueIndex', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810062853_AddInstituteEmailDomain') THEN
    DROP INDEX "IX_UserRoles_UserId_RoleId_AcademicSessionId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810062853_AddInstituteEmailDomain') THEN
    ALTER TABLE "UserRoles" ALTER COLUMN "AcademicSessionId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810062853_AddInstituteEmailDomain') THEN
    ALTER TABLE "Institutes" ADD "EmailDomain" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810062853_AddInstituteEmailDomain') THEN
    CREATE UNIQUE INDEX "IX_UserRoles_UserId_RoleId_InstituteId" ON "UserRoles" ("UserId", "RoleId", "InstituteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810062853_AddInstituteEmailDomain') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260810062853_AddInstituteEmailDomain', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810070120_AuditLogNullableInstituteId') THEN
    ALTER TABLE "AuditLogs" DROP CONSTRAINT "FK_AuditLogs_Institutes_InstituteId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810070120_AuditLogNullableInstituteId') THEN
    ALTER TABLE "AuditLogs" ALTER COLUMN "InstituteId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810070120_AuditLogNullableInstituteId') THEN
    ALTER TABLE "AuditLogs" ADD CONSTRAINT "FK_AuditLogs_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260810070120_AuditLogNullableInstituteId') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260810070120_AuditLogNullableInstituteId', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811122414_AddStudentAllottedRound') THEN
    ALTER TABLE "Students" ADD "AllottedRound" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811122414_AddStudentAllottedRound') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260811122414_AddStudentAllottedRound', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811145249_FinalPendingChanges') THEN
    CREATE TABLE "StudentImportHistories" (
        "Id" uuid NOT NULL,
        "FileName" character varying(255) NOT NULL,
        "ImportDate" timestamp with time zone NOT NULL,
        "NumberOfStudentsImported" integer NOT NULL,
        "Status" integer NOT NULL,
        "ValidationErrors" character varying(4000),
        "TradeId" uuid NOT NULL,
        "AcademicSessionId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "CreatedBy" uuid,
        "UpdatedBy" uuid,
        "InstituteId" uuid NOT NULL,
        CONSTRAINT "PK_StudentImportHistories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StudentImportHistories_AcademicSessions_AcademicSessionId" FOREIGN KEY ("AcademicSessionId") REFERENCES "AcademicSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_StudentImportHistories_Institutes_InstituteId" FOREIGN KEY ("InstituteId") REFERENCES "Institutes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_StudentImportHistories_Trades_TradeId" FOREIGN KEY ("TradeId") REFERENCES "Trades" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811145249_FinalPendingChanges') THEN
    CREATE INDEX "IX_StudentImportHistories_AcademicSessionId" ON "StudentImportHistories" ("AcademicSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811145249_FinalPendingChanges') THEN
    CREATE INDEX "IX_StudentImportHistories_InstituteId" ON "StudentImportHistories" ("InstituteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811145249_FinalPendingChanges') THEN
    CREATE INDEX "IX_StudentImportHistories_TradeId" ON "StudentImportHistories" ("TradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811145249_FinalPendingChanges') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260811145249_FinalPendingChanges', '9.0.2');
    END IF;
END $EF$;
COMMIT;

