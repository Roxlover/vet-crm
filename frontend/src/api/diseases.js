import http from './http';

export const getDiseases = (params = {}) => {
  return http.get('/api/diseases', { params });
};

export const getDisease = (id) => {
  return http.get(`/api/diseases/${id}`);
};

export const createDisease = (data) => {
  return http.post('/api/diseases', data);
};

export const updateDisease = (id, data) => {
  return http.put(`/api/diseases/${id}`, data);
};

export const deleteDisease = (id) => {
  return http.delete(`/api/diseases/${id}`);
};
