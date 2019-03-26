using nucoris.application.interfaces;
using nucoris.queries.PatientStateView;

namespace nucoris.application.queries
{
    public class PatientStateQuery : MaterializedViewQuery<PatientStateViewItem>
    {
        public PatientStateQuery(IMaterializedViewSpecification<PatientStateViewItem> specification)
            : base(specification)
        {
        }
    }
}
