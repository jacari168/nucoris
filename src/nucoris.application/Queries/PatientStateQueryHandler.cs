using nucoris.queries.PatientStateView;
using nucoris.application.interfaces;

namespace nucoris.application.queries
{
    public class PatientStateQueryHandler : MaterializedViewQueryHandler<PatientStateQuery, PatientStateViewItem>
    {
        public PatientStateQueryHandler(
            IMaterializedViewRepository<PatientStateViewItem, IMaterializedViewSpecification<PatientStateViewItem>> queryRepository)
            : base(queryRepository)
        {
        }
    }


}
