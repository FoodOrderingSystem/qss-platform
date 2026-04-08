namespace QSS.API.Authorization;

/// <summary>
/// Permission policy names used across the API layer.
/// Each permission maps to one or more roles via policies registered in Program.cs.
/// </summary>
public static class Permissions
{
    // Tasks
    public const string TasksCreate = "Tasks.Create";
    public const string TasksEdit = "Tasks.Edit";
    public const string TasksDelete = "Tasks.Delete";
    public const string TasksUpdateStatus = "Tasks.UpdateStatus";
    public const string TasksAddComment = "Tasks.AddComment";

    // Processes
    public const string ProcessesManage = "Processes.Manage";

    // Devices
    public const string DevicesView = "Devices.View";
    public const string DevicesManage = "Devices.Manage";

    // Materials
    public const string MaterialsView = "Materials.View";
    public const string MaterialsManage = "Materials.Manage";

    // Medications
    public const string MedicationsView = "Medications.View";
    public const string MedicationsManage = "Medications.Manage";
    public const string MedicationsLogUsage = "Medications.LogUsage";

    // Rooms
    public const string RoomsView = "Rooms.View";
    public const string RoomsManage = "Rooms.Manage";

    // Users
    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";

    // Reports
    public const string ReportsView = "Reports.View";

    // Learning
    public const string LearningManage = "Learning.Manage";

    // Process Categories
    public const string CategoriesManage = "Categories.Manage";
}
