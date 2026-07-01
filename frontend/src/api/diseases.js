import { http } from './http';

export const getDiseases = (params = {}) => {
  return http.get('/diseases', { params });
};

export const getDisease = (id) => {
  return http.get(`/diseases/${id}`);
};

export const createDisease = (data) => {
  return http.post('/diseases', data);
};

export const updateDisease = (id, data) => {
  return http.put(`/diseases/${id}`, data);
};

export const deleteDisease = (id) => {
  return http.delete(`/diseases/${id}`);
};
