namespace ITI.ERP.Application.DTOs.TradeMaster;

public class TradeMasterValidationError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
