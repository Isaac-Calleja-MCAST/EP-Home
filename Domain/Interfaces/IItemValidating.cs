using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IItemValidating
    {
        // We need a common way to get the ID for the checkboxes (SE1.3 multi-select)
        // Since Restaurant uses Int and MenuItem uses Guid, we return a string here.
        string GetIdString();

        string Status { get; set; }

        // Required by Brief SE1.3
        List<string> GetValidators();
        string GetCardPartial();
    }
}