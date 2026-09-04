namespace ITI.ERP.Domain.Constants
{
    public static class Permissions
    {
        public static class Institute
        {
            public const string View = "Institute.View";
            public const string Edit = "Institute.Edit";
            public const string Delete = "Institute.Delete";
        }

        public static class AcademicSession
        {
            public const string View = "AcademicSession.View";
            public const string Create = "AcademicSession.Create";
            public const string Edit = "AcademicSession.Edit";
            public const string Delete = "AcademicSession.Delete";
            public const string Lock = "AcademicSession.Lock";
            public const string Activate = "AcademicSession.Activate";
        }

        public static class Trade
        {
            public const string View = "Trade.View";
            public const string Create = "Trade.Create";
            public const string Edit = "Trade.Edit";
            public const string Archive = "Trade.Archive";
            public const string AssignHead = "Trade.AssignHead";
            public const string Import = "Trade.Import";
        }

        public static class Batch
        {
            public const string View = "Batch.View";
            public const string Create = "Batch.Create";
            public const string Edit = "Batch.Edit";
            public const string Archive = "Batch.Archive";
        }

        public static class Student
        {
            public const string View = "Student.View";
            public const string Create = "Student.Create";
            public const string Edit = "Student.Edit";
            public const string Delete = "Student.Delete";
            public const string Archive = "Student.Archive";
            public const string Transfer = "Student.Transfer";
            public const string Photo = "Student.Photo";
            public const string Import = "Student.Import";
        }

        public static class Attendance
        {
            public const string View = "Attendance.View";
            public const string Mark = "Attendance.Mark";
            public const string Unlock = "Attendance.Unlock";
            public const string Export = "Attendance.Export";
        }

        public static class Practical
        {
            public const string View = "Practical.View";
            public const string Create = "Practical.Create";
            public const string Edit = "Practical.Edit";
            public const string Delete = "Practical.Delete";
            public const string Lock = "Practical.Lock";
            public const string Unlock = "Practical.Unlock";
        }

        public static class Reports
        {
            public const string View = "Reports.View";
            public const string Export = "Reports.Export";
        }

        public static class Users
        {
            public const string CreateTradeHead = "Users.CreateTradeHead";
            public const string Edit = "Users.Edit";
            public const string ToggleStatus = "Users.ToggleStatus";
            public const string View = "Users.View";
            public const string Manage = "Users.Manage";
            public const string ResetPassword = "Users.ResetPassword";
            public const string Unlock = "Users.Unlock";
        }

        public static class Settings
        {
            public const string Manage = "Settings.Manage";
        }

        public static class Dashboard
        {
            public const string View = "Dashboard.View";
        }

        public static class Holiday
        {
            public const string View = "Holiday.View";
            public const string Create = "Holiday.Create";
            public const string Delete = "Holiday.Delete";
        }
    }
}
