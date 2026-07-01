import { http } from './http';

export const getPetDiagnoses = (petId) => {
  return http.get(`/pets/${petId}/diagnoses`);
};

export const createPetDiagnosis = (petId, data) => {
  return http.post(`/pets/${petId}/diagnoses`, data);
};

export const updateDiagnosisStatus = (id, status) => {
  return http.put(`/petdiagnoses/${id}`, { status });
};
