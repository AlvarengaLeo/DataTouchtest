namespace DataTouch.Web.Models;

/// <summary>
/// Settings for the reservations (date range) template block.
/// Serialized to Card.ReservationSettingsJson.
/// </summary>
public class ReservationSettingsModel
{
    // Block copy
    public string BlockTitle { get; set; } = "RESERVAR";
    public string BlockSubtitle { get; set; } = "Elige tus fechas y envía la solicitud.";
    
    // Constraints
    public int MinNights { get; set; } = 1;
    public int MaxGuests { get; set; } = 10;
    public int MinAdvanceDays { get; set; } = 0; // 0 = same-day allowed
    public bool SeparateChildCount { get; set; } = false; // true = show Adults + Children
    
    // Check-in/out time (picker-bound)
    public TimeSpan? CheckInTime { get; set; } = new TimeSpan(15, 0, 0); // 3:00 PM default
    public TimeSpan? CheckOutTime { get; set; } = new TimeSpan(11, 0, 0); // 11:00 AM default
    
    // Check-in/out display strings (auto-formatted from TimeSpan, kept for backward compat)
    public string? CheckInInfo { get; set; } = "check-in: 3:00 PM";
    public string? CheckOutInfo { get; set; } = "check-out 11:00 AM";
    
    /// <summary>Format a TimeSpan as 12h AM/PM string (e.g. "3:00 PM")</summary>
    public static string FormatTime(TimeSpan? time)
    {
        if (time == null) return "";
        var dt = DateTime.Today.Add(time.Value);
        return dt.ToString("h:mm tt");
    }
    
    /// <summary>Sync display strings from TimeSpan values</summary>
    public void SyncDisplayStrings()
    {
        CheckInInfo = CheckInTime.HasValue ? $"check-in: {FormatTime(CheckInTime)}" : null;
        CheckOutInfo = CheckOutTime.HasValue ? $"check-out: {FormatTime(CheckOutTime)}" : null;
    }
    
    /// <summary>Migrate legacy string-only values to TimeSpan (called after deserialization)</summary>
    public void MigrateFromLegacy()
    {
        if (CheckInTime == null && !string.IsNullOrEmpty(CheckInInfo))
            CheckInTime = TryParseTimeFromString(CheckInInfo);
        if (CheckOutTime == null && !string.IsNullOrEmpty(CheckOutInfo))
            CheckOutTime = TryParseTimeFromString(CheckOutInfo);
    }
    
    private static TimeSpan? TryParseTimeFromString(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        // Strip prefixes like "check-in: " or "check-out "
        var clean = text.Replace("check-in:", "").Replace("check-out:", "")
                        .Replace("check-in", "").Replace("check-out", "")
                        .Trim().TrimStart(':').Trim();
        if (DateTime.TryParse(clean, out var dt))
            return dt.TimeOfDay;
        return null;
    }
    
    // Policies
    public string? PoliciesText { get; set; }
    public bool ShowPolicies { get; set; } = false;
    
    // Extras (JSON list of {Name, Price?})
    public string? ExtrasJson { get; set; } // [{"Name":"Desayuno","Price":15},{"Name":"Late check-out","Price":25}]
    
    // Confirmation
    public string ConfirmationTimeText { get; set; } = "Confirmación en menos de 24h";
    public string SuccessMessage { get; set; } = "¡Solicitud enviada! Te confirmaremos pronto.";
    
    // Form fields
    public bool EmailRequired { get; set; } = false;
    public bool PhoneRequired { get; set; } = true;
}
