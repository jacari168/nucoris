using nucoris.domain;
using nucoris.application.interfaces;
using patientQueries = nucoris.queries.PatientStateView;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace nucoris.persistence
{
    public class PatientRepository : PatientDescendentRepository<Patient>,
                            application.interfaces.repositories.IPatientRepository
    {
        private readonly IMaterializedViewRepository<patientQueries.PatientStateViewItem, patientQueries.PatientStateViewSpecification> _patientQueryRepository;

        public PatientRepository(DbSession dbSession,
                IMaterializedViewRepository<patientQueries.PatientStateViewItem, patientQueries.PatientStateViewSpecification> patientQueryRepository) 
            : base(dbSession)
        {
            _patientQueryRepository = patientQueryRepository;
        }

        public async Task<Patient> GetAsync(System.Guid id)
        {
            return await base.GetAsync( patientId: id, itemId: id);
        }

        public async Task<Patient> GetByMrnAsync(string mrn)
        {
            // We take advantage of the query that tracks patients and their admissions:
            var specification = new patientQueries.PatientStateViewSpecification(mrn);
            var patients = await _patientQueryRepository.GetManyAsync(specification);
            
            if( patients.Any())
            {
                var id = patients.First().QueryPatient.Id;
                return await GetAsync(id);
            }

            return null;
        }

        public async Task<List<IPatientPersistable>> GetFullPatientRecordAsync(Guid patientId)
        {
            return await _dbSession.LoadManyDerivedAsync<IPatientPersistable>(patientId, null);
        }
    }
}
