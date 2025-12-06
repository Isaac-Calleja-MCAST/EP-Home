using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IItemsRepository
    {
        // Get and Save,
        void Save(IEnumerable<IItemValidating> items);
        IEnumerable<IItemValidating> Get();

        // Needed for the Approval feature
        void Approve(string idString);
    }
}