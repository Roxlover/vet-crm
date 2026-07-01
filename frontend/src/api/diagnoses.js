import http from './http';

export const getPetDiagnoses = (petId) => {
  return http.get(`/api/pets/${petId}/diagnoses`);
};

export const createPetDiagnosis = (petId, data) => {
  return http.post(`/api/pets/${petId}/diagnoses`, data);
};

export const updateDiagnosisStatus = (id, status) => {
  return http.put(`/api/petdiagnoses/${id}`, { status });
};
