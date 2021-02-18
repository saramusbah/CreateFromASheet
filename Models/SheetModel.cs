namespace CreateFromASheet.Models
{
    using System.Collections.Generic;

    public class SheetModel
    {
        public Dictionary<string, object> Columns { get; set; } = new Dictionary<string, object>();
    }
}
