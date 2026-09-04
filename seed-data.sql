-- ITI ERP Seed Data Script
-- Run with: docker exec -i iti-erp-postgres psql -U postgres -d iti_erp -f /tmp/seed-data.sql

BEGIN;

-- Fixed GUIDs for referential integrity
DO $$
DECLARE
    v_institute_id UUID := 'a0000000-0000-0000-0000-000000000001';
    v_session_id   UUID := 'b0000000-0000-0000-0000-000000000001';
    v_admin_role_id UUID := 'c0000000-0000-0000-0000-000000000001';
    v_tradehead_role_id UUID := 'c0000000-0000-0000-0000-000000000002';
    v_admin_user_id UUID := 'd0000000-0000-0000-0000-000000000001';
    v_now TIMESTAMP := (NOW() AT TIME ZONE 'UTC');
BEGIN

-- Skip if data already exists
IF EXISTS (SELECT 1 FROM "Users" LIMIT 1) THEN
    RAISE NOTICE 'Seed data already exists, skipping.';
    RETURN;
END IF;

-- 1. Institute (no IsDeleted column - extends AuditableEntity only)
INSERT INTO "Institutes" ("Id", "GRNumber", "Name", "IsActive", "CreatedAt")
VALUES (v_institute_id, 'GR0001', 'Satana Institute', true, v_now);

-- 2. Academic Session (has IsDeleted, StartDate, EndDate)
INSERT INTO "AcademicSessions" ("Id", "SessionYear", "StartDate", "EndDate", "IsActive", "IsLocked", "CreatedAt", "InstituteId", "IsDeleted")
VALUES (v_session_id, '2025-26', '2025-04-01', '2026-03-31', true, false, v_now, v_institute_id, false);

-- 3. Permissions (34 total)
INSERT INTO "Permissions" ("Id", "Code", "Module", "Action", "Description", "IsActive", "CreatedAt") VALUES
('e0000000-0000-0000-0000-000000000001', 'Institute.View',          'Institute',          'View',         'Institute.View',          true, v_now),
('e0000000-0000-0000-0000-000000000002', 'Institute.Edit',          'Institute',          'Edit',         'Institute.Edit',          true, v_now),
('e0000000-0000-0000-0000-000000000003', 'AcademicSession.View',    'AcademicSession',    'View',         'AcademicSession.View',    true, v_now),
('e0000000-0000-0000-0000-000000000004', 'AcademicSession.Create',  'AcademicSession',    'Create',       'AcademicSession.Create',  true, v_now),
('e0000000-0000-0000-0000-000000000005', 'AcademicSession.Edit',    'AcademicSession',    'Edit',         'AcademicSession.Edit',    true, v_now),
('e0000000-0000-0000-0000-000000000006', 'AcademicSession.Lock',    'AcademicSession',    'Lock',         'AcademicSession.Lock',    true, v_now),
('e0000000-0000-0000-0000-000000000007', 'AcademicSession.Activate','AcademicSession',    'Activate',     'AcademicSession.Activate',true, v_now),
('e0000000-0000-0000-0000-000000000008', 'Trade.View',              'Trade',              'View',         'Trade.View',              true, v_now),
('e0000000-0000-0000-0000-000000000009', 'Trade.Create',            'Trade',              'Create',       'Trade.Create',            true, v_now),
('e0000000-0000-0000-0000-000000000010', 'Trade.Edit',              'Trade',              'Edit',         'Trade.Edit',              true, v_now),
('e0000000-0000-0000-0000-000000000011', 'Trade.Archive',           'Trade',              'Archive',      'Trade.Archive',           true, v_now),
('e0000000-0000-0000-0000-000000000012', 'Trade.AssignHead',        'Trade',              'AssignHead',   'Trade.AssignHead',        true, v_now),
('e0000000-0000-0000-0000-000000000013', 'Student.View',            'Student',            'View',         'Student.View',            true, v_now),
('e0000000-0000-0000-0000-000000000014', 'Student.Create',          'Student',            'Create',       'Student.Create',          true, v_now),
('e0000000-0000-0000-0000-000000000015', 'Student.Edit',            'Student',            'Edit',         'Student.Edit',            true, v_now),
('e0000000-0000-0000-0000-000000000016', 'Student.Archive',         'Student',            'Archive',      'Student.Archive',         true, v_now),
('e0000000-0000-0000-0000-000000000017', 'Student.Transfer',        'Student',            'Transfer',     'Student.Transfer',        true, v_now),
('e0000000-0000-0000-0000-000000000018', 'Student.Photo',           'Student',            'Photo',        'Student.Photo',           true, v_now),
('e0000000-0000-0000-0000-000000000019', 'Attendance.View',         'Attendance',         'View',         'Attendance.View',         true, v_now),
('e0000000-0000-0000-0000-000000000020', 'Attendance.Mark',         'Attendance',         'Mark',         'Attendance.Mark',         true, v_now),
('e0000000-0000-0000-0000-000000000021', 'Attendance.Unlock',       'Attendance',         'Unlock',       'Attendance.Unlock',       true, v_now),
('e0000000-0000-0000-0000-000000000022', 'Attendance.Export',       'Attendance',         'Export',       'Attendance.Export',       true, v_now),
('e0000000-0000-0000-0000-000000000023', 'Practical.View',          'Practical',          'View',         'Practical.View',          true, v_now),
('e0000000-0000-0000-0000-000000000024', 'Practical.Create',        'Practical',          'Create',       'Practical.Create',        true, v_now),
('e0000000-0000-0000-0000-000000000025', 'Practical.Edit',          'Practical',          'Edit',         'Practical.Edit',          true, v_now),
('e0000000-0000-0000-0000-000000000026', 'Practical.Lock',          'Practical',          'Lock',         'Practical.Lock',          true, v_now),
('e0000000-0000-0000-0000-000000000027', 'Practical.Unlock',        'Practical',          'Unlock',       'Practical.Unlock',        true, v_now),
('e0000000-0000-0000-0000-000000000028', 'Reports.View',            'Reports',            'View',         'Reports.View',            true, v_now),
('e0000000-0000-0000-0000-000000000029', 'Reports.Export',          'Reports',            'Export',       'Reports.Export',          true, v_now),
('e0000000-0000-0000-0000-000000000030', 'Users.Manage',            'Users',              'Manage',       'Users.Manage',            true, v_now),
('e0000000-0000-0000-0000-000000000031', 'Users.ResetPassword',     'Users',              'ResetPassword','Users.ResetPassword',     true, v_now),
('e0000000-0000-0000-0000-000000000032', 'Users.Unlock',            'Users',              'Unlock',       'Users.Unlock',            true, v_now),
('e0000000-0000-0000-0000-000000000033', 'Settings.Manage',         'Settings',           'Manage',       'Settings.Manage',         true, v_now),
('e0000000-0000-0000-0000-000000000034', 'Dashboard.View',          'Dashboard',          'View',         'Dashboard.View',          true, v_now);

-- 4. Roles (RoleType: 0=Admin, IsSystemRole, IsActive)
INSERT INTO "Roles" ("Id", "Name", "RoleType", "IsSystemRole", "IsActive", "CreatedAt", "InstituteId")
VALUES
(v_admin_role_id, 'Admin', 0, true, true, v_now, v_institute_id),
(v_tradehead_role_id, 'TradeHead', 0, true, true, v_now, v_institute_id);

-- 5. Admin RolePermissions (all permissions)
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "IsActive", "CreatedAt")
SELECT gen_random_uuid(), v_admin_role_id, "Id", true, v_now FROM "Permissions";

-- 6. TradeHead RolePermissions (subset)
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "IsActive", "CreatedAt")
SELECT gen_random_uuid(), v_tradehead_role_id, "Id", true, v_now
FROM "Permissions"
WHERE "Code" IN ('Trade.View', 'Trade.Create', 'Trade.Edit', 'Student.View', 'Student.Create', 'Student.Edit');

-- 7. Admin User (password: Admin@123)
INSERT INTO "Users" ("Id", "Username", "PasswordHash", "FirstName", "LastName", "Email", "IsActive", "IsLocked", "FailedLoginAttempts", "IsDeleted", "CreatedAt", "InstituteId")
VALUES (v_admin_user_id, 'admin', '$2a$11$1gIWc./jQe8q.3RMgaSYpeqpd383YMU6TUWqyzZkd1opaxnkVN/nS', 'System', 'Administrator', 'admin@satana.edu', true, false, 0, false, v_now, v_institute_id);

-- 8. UserRole
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "InstituteId", "AcademicSessionId", "IsActive", "CreatedAt")
VALUES (gen_random_uuid(), v_admin_user_id, v_admin_role_id, v_institute_id, v_session_id, true, v_now);

RAISE NOTICE 'Seed data inserted successfully.';
END $$;

COMMIT;
