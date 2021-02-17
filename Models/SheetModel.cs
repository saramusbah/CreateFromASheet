namespace CreateFromASheet.Models
{
    using System.Collections.Generic;

    public class SheetModel
    {
        public Dictionary<object, object> Columns { get; set; } = new Dictionary<object, object>();
    }
}
