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
}

export const refreshToken = () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
        return Promise.reject(new Error('No refresh token available'));
    }
    return axios.post(`${API_BASE}/auth/refresh`, { refreshToken }).then((res) => {
        if (res.data.accessToken) {
            localStorage.setItem('token', res.data.accessToken);
        }
        return res.data;
    });
}

export async function createCrawlRequest({ mode, keyword, maxPage, token }) {
    return await axios.post(
        `${API_BASE}/crawlrequests`,
        { mode, keyword, maxPage: Number(maxPage) },
        { headers: { Authorization: `Bearer ${token}` } }
    );
}

export async function getProductHistory(taskId, token) {
    return await axios.post(
        `${API_BASE}/producthistory/get-history`,
        { taskId },
        { headers: { Authorization: `Bearer ${token}` } }
    );
}

export async function getCrawlerTask({ token }) {
    return await axios.post(
        `${API_BASE}/crawlertask/get-crawler-task`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
    );
}

export const getToken = () => localStorage.getItem('token');

export const isAuthenticated = () => !!getToken();
