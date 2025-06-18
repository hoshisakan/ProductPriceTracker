import axios from 'axios';

const API_BASE = 'http://localhost/api';

export const register = (username, password) => {
    return axios.post(`${API_BASE}/auth/register`, { username, password });
};

export const login = (username, password) => {
    return axios.post(`${API_BASE}/auth/login`, { username, password }).then((res) => {
        if (res.data.accessToken) {
            localStorage.setItem('token', res.data.accessToken);
            localStorage.setItem('refreshToken', res.data.refreshToken);
        }
        return res.data;
    });
};

export const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
};

export const getToken = () => localStorage.getItem('token');

export const isAuthenticated = () => !!getToken();
