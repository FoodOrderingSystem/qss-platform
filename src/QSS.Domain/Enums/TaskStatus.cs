namespace QSS.Domain.Enums;

public enum QssTaskStatus
{
    Open = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3
}

public enum TaskFrequency
{
    Once = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Yearly = 5
}

public enum ProcessCategory
{
    Hygiene = 0,
    Maintenance = 1,
    Administrative = 2,
    Clinical = 3,
    Safety = 4,
    Training = 5,
    Other = 6
}

public enum NotificationType
{
    TaskAssigned = 0,
    TaskDue = 1,
    TaskOverdue = 2,
    TaskCompleted = 3,
    NewMessage = 4,
    NewComment = 5,
    InventoryAlert = 6,
    ExpirationAlert = 7
}

public enum MaterialUnit
{
    Pieces = 0,
    Boxes = 1,
    Bottles = 2,
    Packs = 3,
    Liters = 4,
    Milliliters = 5,
    Grams = 6,
    Kilograms = 7
}
