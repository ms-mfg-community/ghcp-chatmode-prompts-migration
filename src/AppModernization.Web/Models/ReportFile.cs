namespace AppModernization.Web.Models;

public class ReportFile
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime LastModified { get; set; }
    public long Size { get; set; }
    public string Location { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string PhaseName { get; set; } = "";
}
